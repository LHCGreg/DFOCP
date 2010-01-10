using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dfo.Login
{
	public enum LaunchState
	{
		None,
		Login,
		Launching,
		GameInProgress
	}

	/// <summary>
	/// This class allows launching of DFO with a variety of options.
	/// </summary>
	public class DfoLauncher : IDisposable
	{
		private object m_syncHandle = new object();

		private Thread m_dfoMonitorThread = null; // Not needed? Maybe hold onto it in case we ever want it
		private AutoResetEvent m_monitorCancelEvent = new AutoResetEvent( false ); // tied to m_cancelMonitorThread
		private AutoResetEvent m_monitorFinishedEvent = new AutoResetEvent( false );
		private AutoResetEvent m_launcherDoneEvent = new AutoResetEvent( false ); // Set when the launcher process that checks for patches terminates

		private Process m_launcherProcess = null;

		private bool m_disposed = false;

		/// <summary>
		/// Gets the parameters to use when launching the game. You may change the parameters, but the changes
		/// will only take effect when launching the game. Changes will not effect an existing launch.
		/// </summary>
		public LaunchParams Params { get; private set; }

		/// <summary>
		/// Raised when the State property changes. The event may be raised inside a method called by the caller or
		/// from a background thread. Only the State property may be safely accessed in the event handler.
		/// No other properties or methods may be called without synchronizing access to this object.
		/// </summary>
		public event EventHandler<EventArgs> LaunchStateChanged
		{
			add
			{
				lock ( m_LaunchStateChangedLock ) { m_LaunchStateChangedDelegate += value; }
			}
			remove
			{
				lock ( m_LaunchStateChangedLock ) { m_LaunchStateChangedDelegate -= value; }
			}
		}

		#region thread-safe event stuff
		private object m_LaunchStateChangedLock = new object();
		private EventHandler<EventArgs> m_LaunchStateChangedDelegate;

		protected virtual void OnLaunchStateChanged( EventArgs e )
		{
			EventHandler<EventArgs> currentDelegate;
			lock ( m_LaunchStateChangedLock )
			{
				currentDelegate = m_LaunchStateChangedDelegate;
			}
			if ( currentDelegate != null )
			{
				currentDelegate( this, e );
			}
		}
		#endregion

		public event EventHandler<CancelErrorEventArgs> WindowModeFailed
		{
			add
			{
				lock ( m_WindowModeFailedLock ) { m_WindowModeFailedDelegate += value; }
			}
			remove
			{
				lock ( m_WindowModeFailedLock ) { m_WindowModeFailedDelegate -= value; }
			}
		}
		#region thread-safe event stuff
		private object m_WindowModeFailedLock = new object();
		private EventHandler<CancelErrorEventArgs> m_WindowModeFailedDelegate;

		protected virtual void OnWindowModeFailed( CancelErrorEventArgs e )
		{
			EventHandler<CancelErrorEventArgs> currentDelegate;
			lock ( m_WindowModeFailedLock )
			{
				currentDelegate = m_WindowModeFailedDelegate;
			}
			if ( currentDelegate != null )
			{
				currentDelegate( this, e );
			}
		}
		#endregion

		// If state is None, the monitor thread either does not exist or is on its way out.
		private LaunchState m_state = LaunchState.None;

		/// <summary>
		/// Gets the current state of launching. This property is thread-safe.
		/// </summary>
		public LaunchState State
		{
			get
			{
				LaunchState state;
				lock ( m_syncHandle )
				{
					state = m_state;
				}
				return state;
			}
			private set
			{
				bool stateChanged = false;

				lock ( m_syncHandle )
				{
					if ( m_state != value )
					{
						m_state = value;
						stateChanged = true;
					}
				}

				if ( stateChanged )
				{
					OnLaunchStateChanged( EventArgs.Empty );
				}
			}
		}

		public DfoLauncher()
		{
			// Set defaults for properties
			Params = new LaunchParams();
		}

		/// <summary>
		/// Launches DFO using the parameters in the <c>Params</c> property.
		/// </summary>
		/// 
		/// <exception cref="System.InvalidOperationException">The game has already been launched.</exception>
		/// <exception cref="System.ArgumentNullException">Params.Username, Params.Password, or
		/// Params.DfoDir is null, or Params.SwitchSoundpacks is true and Params.SoundpackDir,
		/// Params.CustomSoundpackDir, or Params.TempSoundpackDir is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Params.LoginTimeoutInMs or
		/// Params.PollingIntervalInMs was negative.</exception>
		/// <exception cref="System.SecurityException">The caller does not have permission to connect to the DFO
		/// URI.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="DfoLogin.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// <exception cref="DfoLogin.DfoLaunchException">The game could not be launched.</exception>
		/// <exception cref="System.ObjectDisposedException">This object has been Disposed of.</exception>
		public void Launch()
		{
			if ( State != LaunchState.None )
			{
				throw new InvalidOperationException( "The game has already been launched" );
			}

			try
			{
				StartDfo();
			}
			catch ( Exception )
			{
				Reset(); // TODO: Filter the exceptions to only the relevant ones?

				throw;
			}
		}

		/// <summary>
		/// Launches DFO.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Username, password, or DfoDir was null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Params.LoginTimeoutInMs or
		/// Params.PollingIntervalInMs was negative.</exception>
		/// <exception cref="System.SecurityException">The caller does not have permission to connect to the DFO
		/// URI.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="DfoLogin.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// <exception cref="DfoLogin.DfoLaunchException">The game could not be launched.</exception>
		/// <exception cref="System.ObjectDisposedException">This object has been Disposed of.</exception>
		private void StartDfo()
		{
			if ( m_disposed )
			{
				throw new ObjectDisposedException( "DfoLauncher" );
			}
			if ( Params.DfoDir == null )
			{
				throw new ArgumentNullException( "DfoDir cannot be null." );
			}
			//if ( Params.PollingIntervalInMs < 0 )
			//{
			//    throw new ArgumentOutOfRangeException( "PollingIntervalInMs cannot be negative." );
			//}

			bool ok = EnforceWindowedSetting();
			if ( !ok )
			{
				return;
			}

			State = LaunchState.Login; // We are now logging in

			string dfoLauncherPath = ""; // assignment to shut the compiler up. I know that if Win32Exception is thrown, this has been set.
			try
			{
				string dfoArg = DfoLogin.GetDfoArg( Params.Username, Params.Password, Params.LoginTimeoutInMs ); // Log in
				State = LaunchState.Launching; // If we reach this line, we successfully logged in. Now we're launching.

				m_monitorCancelEvent.Reset();
				m_monitorFinishedEvent.Reset();
				m_launcherDoneEvent.Reset(); // Make sure to reset this before starting the launcher process

				// Start the launcher process
				lock ( m_syncHandle )
				{
					m_launcherProcess = new Process();
				}
				m_launcherProcess.StartInfo.FileName = Params.DfoLauncherExe;
				m_launcherProcess.StartInfo.Arguments = dfoArg; // This argument contains the authentication token we got from logging in
				m_launcherProcess.EnableRaisingEvents = true;
				m_launcherProcess.Exited += LauncherProcessExitedHandler; // Use async notification instead of synchronous waiting so the we can cancel while the launcher process is going
				m_launcherProcess.Start();

				m_dfoMonitorThread = new Thread( BackgroundThreadEntryPoint ); // Start the thread that monitors the state of DFO
				m_dfoMonitorThread.IsBackground = true;
				m_dfoMonitorThread.Start( Params.Clone() ); // Give it a copy of the launch params so the caller can change the Params property while the game is running with no effects for the next time they launch
			}
			catch ( System.Net.WebException ex )
			{
				throw new System.Net.WebException( string.Format(
					"There was a problem connecting. Check your Internet connection. Details: {0}", ex.Message ), ex );
			}
			catch ( DfoAuthenticationException ex )
			{
				throw new DfoAuthenticationException( string.Format(
					"Error while authenticating: {0}", ex.Message ), ex );
			}
			catch ( System.ComponentModel.Win32Exception ex )
			{
				throw new DfoLaunchException( string.Format(
					"Error while starting DFO using {0}: {1} (Did you forget to put this program in the DFO directory?)",
					dfoLauncherPath, ex.Message ), ex );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>True if everything went ok or if there was an error and the caller's event handler did not
		/// tell us to stop.</returns>
		private bool EnforceWindowedSetting()
		{
			if ( Params.LaunchInWindowed.HasValue )
			{
				string magicWindowModeDirectory = "zo3mo4";
				Exception error = null;

				if ( Params.LaunchInWindowed.Value )
				{
					try
					{
						Directory.CreateDirectory( Path.Combine( Params.DfoDir, magicWindowModeDirectory ) );
					}
					catch ( Exception ex )
					{
						if ( ex is System.IO.IOException || ex is System.UnauthorizedAccessException
						  || ex is System.ArgumentException || ex is System.IO.PathTooLongException
						  || ex is System.IO.DirectoryNotFoundException || ex is System.NotSupportedException )
						{
							error = new IOException( string.Format(
								"Error while trying to create directory {0}: {1}",
								magicWindowModeDirectory, ex.Message ), ex );
						}
						else
						{
							throw;
						}
					}
				}
				else
				{
					try
					{
						Directory.Delete( Path.Combine( Params.DfoDir, magicWindowModeDirectory ), true );
					}
					catch ( DirectoryNotFoundException )
					{
						; // It's ok if the directory doesn't exist
					}
					catch ( Exception ex )
					{
						if ( ex is System.IO.IOException || ex is System.UnauthorizedAccessException
						  || ex is System.ArgumentException || ex is System.IO.PathTooLongException
						  || ex is System.ArgumentException )
						{
							error = new IOException( string.Format(
								"Error while trying to remove directory {0}: {1}",
								magicWindowModeDirectory, ex.Message ), ex );
						}
						else
						{
							throw;
						}
					}
				}

				if ( error != null )
				{
					CancelErrorEventArgs e = new CancelErrorEventArgs( error );
					OnWindowModeFailed( e );
					if ( e.Cancel )
					{
						return false; // false = not ok
					}
					else
					{
						return true; // true = ok
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true; // true = ok
			}
		}

		/// <summary>
		/// Resets to an unattached state. This function may block if it needs to do any cleanup.
		/// </summary>
		public void Reset()
		{
			if ( m_disposed )
			{
				return;
			}

			// Send cancel signal to monitor thread if it's running and wait for it to finish terminating
			//if ( State != LaunchState.None )
			if ( MonitorThreadIsRunning() )
			{
				m_monitorCancelEvent.Set();
				m_monitorFinishedEvent.WaitOne();
				// m_dfoMonitorThread got set to null as it was terminating itself
				// m_launcherProcess got disposed of and set to null as monitor thread was exiting if it hadn't done it already
			}
			else
			{
				// If an exception happened while launching but before the monitor thread is started,
				// we need to set the state back to None. Normally the monitor thread does that as it's exiting.
				State = LaunchState.None;
			}

			lock ( m_syncHandle )
			{
				// Need to set m_launcherProcess to null if there was an exception while launching.
				// Dispose for good measure, but if it needs to get Disposed, that means the monitor thread was started.
				if ( m_launcherProcess != null )
				{
					m_launcherProcess.Dispose();
					m_launcherProcess = null;
				}
			}
		}

		private bool MonitorThreadIsRunning()
		{
			lock ( m_syncHandle )
			{
				if ( m_dfoMonitorThread != null )
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		// Was going to implement this, but it doesn't seem like it'd be very useful
		//public void AttachToExistingDfo()
		//{

		//}

		/// <summary>
		/// Entry point for the monitor thread
		/// </summary>
		/// <param name="threadArgs">A <c>LaunchParams</c> object containing a copy of the Params property.</param>
		private void BackgroundThreadEntryPoint( object threadArgs )
		{
			BackgroundThreadEntryPoint( (LaunchParams)threadArgs );
		}

		[DllImport( "user32.dll", SetLastError = true )]
		static extern IntPtr FindWindow( string lpClassName, string lpWindowName );

		private void BackgroundThreadEntryPoint( LaunchParams copiedParams )
		{
			// Once this thread is created, it has full control over the State property. No other thread can
			// change it until this thread sets it back to None.

			// This thread can be canceled by calling the Reset() method. Reset() will put m_monitorCancelEvent
			// in the Set state.
			bool canceled = false;

			// Wait for launcher process to end or a cancel notice
			// We have a process handle to the launcher process and if we get an async notification that it completed,
			// m_launcherDoneEvent is set. Better than polling. :)
			int setHandleIndex = WaitHandle.WaitAny( new WaitHandle[] { m_launcherDoneEvent, m_monitorCancelEvent } );
			if ( setHandleIndex == 0 )
			{
				lock ( m_syncHandle )
				{
					//// The launcher failed so the game will never come up.
					//if ( m_launcherProcess.ExitCode != copiedParams.LauncherSuccessCode )
					//{
					//    canceled = true;
					//}

					m_launcherProcess.Dispose();
					m_launcherProcess = null;
				}
			}
			else if ( setHandleIndex == 1 )
			{
				canceled = true;

				lock ( m_syncHandle )
				{
					m_launcherProcess.Dispose();
					m_launcherProcess = null;
				}
			}

			bool soundpacksSwitched = false;
			if ( !canceled )
			{
				// Switch soundpacks if we were told to do so
				if ( copiedParams.SwitchSoundpacks )
				{
					soundpacksSwitched = SwitchSoundpacks( copiedParams.SoundpackDir, copiedParams.CustomSoundpackDir, copiedParams.TempSoundpackDir );
				}

				// Game is up.
				State = LaunchState.GameInProgress;
			}

			IntPtr dfoMainWindowHandle = IntPtr.Zero;
			if ( !canceled )
			{
				// Wait for DFO window to be created, the DFO process to not exist, or a cancel notice.
				Pair<IntPtr, bool> pollResults = PollUntilCanceled<IntPtr>( copiedParams.GameWindowCreatedPollingIntervalInMs,
					() =>
					{
						IntPtr dfoWindowHandle = FindWindow( copiedParams.DfoWindowClassName, null );
						if ( dfoWindowHandle != IntPtr.Zero )
						{
							return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // Window exists, done polling
						}
						else
						{
							// Check if the DFO process is running. Under normal conditions, it certainly would be
							// by now becauseit gets started by the launcher process and the launcher process has ended.
							// If it is not, that means the launcher ended unsuccessfully or the DFO process ended very quickly.
							// In either case, our job is done here.

							Process[] dfoProcesses = Process.GetProcessesByName( Path.GetFileNameWithoutExtension( copiedParams.DfoExe ) );
							if ( dfoProcesses.Length > 0 )
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, false ); // Window does not exist, keep polling
							}
							else
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // DFO process doesn't exist anymore, treat this like a cancel request
							}
						}
					} );

				canceled = pollResults.Second;
				dfoMainWindowHandle = pollResults.First; // Try not to use this handle - what if the DFO window closes and then some other window gets the same handle value?
			}

			if ( !canceled )
			{
				// Wait for DFO game window to be closed or a cancel notice
				Pair<IntPtr, bool> pollResults = PollUntilCanceled<IntPtr>( copiedParams.GameDonePollingIntervalInMs,
					() =>
					{
						IntPtr dfoWindowHandle = FindWindow( copiedParams.DfoWindowClassName, null );
						if ( dfoWindowHandle == IntPtr.Zero )
						{
							return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // Window does not exist, done polling
						}
						else
						{
							return new Pair<IntPtr, bool>( dfoWindowHandle, false ); // Window exists, keep polling
						}
					} );

				canceled = pollResults.Second;
			}

			if ( !canceled )
			{
				// Kill the DFO process to kill the popup.
				Process[] dfoProcesses = Process.GetProcessesByName( Path.GetFileNameWithoutExtension( copiedParams.DfoExe ) );
				if ( dfoProcesses.Length > 0 )
				{
					try
					{
						dfoProcesses[ 0 ].Kill(); // What to do if there's more than one?
					}
					catch ( Exception ex )
					{
						if ( ex is System.ComponentModel.Win32Exception
						  || ex is InvalidOperationException )
						{
							// Log or something
						}
						else
						{
							throw;
						}
					}
				}
			}

			// Done, clean up.
			// Switch back soundpacks if they were switched
			if ( soundpacksSwitched )
			{
				bool switchbackSuccess = SwitchBackSoundpacks( copiedParams.SoundpackDir, copiedParams.CustomSoundpackDir, copiedParams.TempSoundpackDir );
			}

			lock ( m_syncHandle )
			{
				m_dfoMonitorThread = null;
			}
			State = LaunchState.None;

			m_monitorFinishedEvent.Set();
		}

		/// <summary>
		/// For use by the monitor thread only.
		/// </summary>
		/// <typeparam name="TReturn"></typeparam>
		/// <param name="pollingFunction"></param>
		/// <returns>A Pair&lt;<typeparamref name="TReturn"/>, bool&gt; containing the acceptable polled value or
		/// the default value of the type if the thread is canceled in the first value of the pair and a boolean
		/// that is true if the thread is canceled, false if not.</returns>
		private Pair<TReturn, bool> PollUntilCanceled<TReturn>( int pollingIntervalInMs, Func<Pair<TReturn, bool>> pollingFunction )
		{
			bool canceled = m_monitorCancelEvent.WaitOne( 0 );

			TReturn polledValue = default( TReturn );
			bool polledValueAcceptable = false;

			while ( !polledValueAcceptable && !canceled )
			{
				Pair<TReturn, bool> poll = pollingFunction(); // first = polled value, second = whether value is acceptable
				polledValue = poll.First;
				polledValueAcceptable = poll.Second;

				if ( !polledValueAcceptable )
				{
					// Sleep for a bit or until canceled
					canceled = m_monitorCancelEvent.WaitOne( pollingIntervalInMs );
				}
			}

			if ( canceled )
			{
				return new Pair<TReturn, bool>( default( TReturn ), true );
			}
			else
			{
				return new Pair<TReturn, bool>( polledValue, false );
			}
		}

		private bool SwitchSoundpacks( string soundpackDir, string customSoundpackDir, string tempSoundpackDir )
		{
			// Move soundpackDir to tempSoundpackDir
			// Move customSoundpackDir to soundpackDir
			return false; // TODO
		}

		private bool SwitchBackSoundpacks( string soundpackDir, string customSoundpackDir, string tempSoundpackDir )
		{
			// Move soundpackDir to customSoundpackDir
			// Move tempSoundpackDir to soundpackDir
			return false; // TODO
		}

		private void LauncherProcessExitedHandler( object sender, EventArgs e )
		{
			lock ( m_syncHandle )
			{
				if ( m_launcherProcess == sender )
				{
					m_launcherDoneEvent.Set();
				}
			}
		}

		public void ResizeDfoWindow( int x, int y )
		{
			// TODO
		}

		public void Dispose()
		{
			if ( !m_disposed )
			{
				Reset();
				m_monitorCancelEvent.Close();
				m_monitorFinishedEvent.Close();
				m_launcherDoneEvent.Close();
				m_disposed = true;
			}
		}
	}
}

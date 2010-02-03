using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;

namespace Dfo.Controlling
{
	/// <summary>
	/// Represents a state of DFO.
	/// </summary>
	public enum LaunchState
	{
		/// <summary>
		/// The game has not been started.
		/// </summary>
		None,

		/// <summary>
		/// The user is being logged in.
		/// </summary>
		Login,

		/// <summary>
		/// The game is in the process of starting.
		/// </summary>
		Launching,

		/// <summary>
		/// The main game window has been created.
		/// </summary>
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
		private AutoResetEvent m_monitorFinishedEvent = new AutoResetEvent( false ); // I guess it would have been easier to do a Join on the thread, but oh well, this is done and works
		private AutoResetEvent m_launcherDoneEvent = new AutoResetEvent( false ); // Set when the launcher process that checks for patches terminates

		private Process m_launcherProcess = null;

		private bool m_disposed = false;

		/// <summary>
		/// Gets or sets the parameters to use when launching the game. Changes will not effect an existing launch.
		/// </summary>
		public LaunchParams Params { get; set; }

		/// <summary>
		/// Raised when the State property changes. The event may be raised inside a method called by the caller or
		/// from another thread. Only the State property may be safely accessed in the event handler.
		/// No other properties or methods may be called without synchronizing access to this object.
		/// 
		/// Only a caller-initiated launch can take the state out of <c>LaunchStateNone</c>.
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

		/// <summary>
		/// Raises the <c>LaunchStateChanged</c> event.
		/// </summary>
		/// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
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

		/// <summary>
		/// Raised when the window mode setting could not be used. If the <c>Cancel</c> property of the event args
		/// is set to true, the launch is cancelled.
		/// </summary>
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

		/// <summary>
		/// Raises the <c>WindowModeFailed</c> event.
		/// </summary>
		/// <param name="e">A <c>CancelErrorEventArgs</c> that contains the event data.</param>
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

		/// <summary>
		/// Raised when the soundpacks could not be switched or switched back.
		/// </summary>
		public event EventHandler<ErrorEventArgs> SoundpackSwitchFailed
		{
			add
			{
				lock ( m_SoundpackSwitchFailedLock ) { m_SoundpackSwitchFailedDelegate += value; }
			}
			remove
			{
				lock ( m_SoundpackSwitchFailedLock ) { m_SoundpackSwitchFailedDelegate -= value; }
			}
		}
		#region thread-safe event stuff
		private object m_SoundpackSwitchFailedLock = new object();
		private EventHandler<ErrorEventArgs> m_SoundpackSwitchFailedDelegate;

		/// <summary>
		/// Raises the <c>SoundpackSwitchFailed</c> event.
		/// </summary>
		/// <param name="e">An <c>ErrorEventArgs</c> that contains the event data.</param>
		protected virtual void OnSoundpackSwitchFailed( ErrorEventArgs e )
		{
			EventHandler<ErrorEventArgs> currentDelegate;
			lock ( m_SoundpackSwitchFailedLock )
			{
				currentDelegate = m_SoundpackSwitchFailedDelegate;
			}
			if ( currentDelegate != null )
			{
				currentDelegate( this, e );
			}
		}
		#endregion

		/// <summary>
		/// Raised when there is an error while trying to automatically close the popup at the end of the game.
		/// </summary>
		public event EventHandler<ErrorEventArgs> PopupKillFailed
		{
			add
			{
				lock ( m_PopupKillFailedLock ) { m_PopupKillFailedDelegate += value; }
			}
			remove
			{
				lock ( m_PopupKillFailedLock ) { m_PopupKillFailedDelegate -= value; }
			}
		}
		#region thread-safe event stuff
		private object m_PopupKillFailedLock = new object();
		private EventHandler<ErrorEventArgs> m_PopupKillFailedDelegate;

		/// <summary>
		/// Raises the <c>PopupKillFailed</c> event.
		/// </summary>
		/// <param name="e">An <c>ErrorEventArgs</c> that contains the event data.</param>
		protected virtual void OnPopupKillFailed( ErrorEventArgs e )
		{
			EventHandler<ErrorEventArgs> currentDelegate;
			lock ( m_PopupKillFailedLock )
			{
				currentDelegate = m_PopupKillFailedDelegate;
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

		/// <summary>
		/// Constructs a new <c>DfoLauncher</c> with default parameters.
		/// </summary>
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
		/// <exception cref="System.ArgumentOutOfRangeException">Params.LoginTimeoutInMs or was negative.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the DFO
		/// URI.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// <exception cref="Dfo.Controlling.DfoLaunchException">The game could not be launched.</exception>
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
		/// <exception cref="System.ArgumentNullException">Username, Password, or Params was null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Params.LoginTimeoutInMs was negative.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the DFO
		/// URI.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// <exception cref="Dfo.Controlling.DfoLaunchException">The game could not be launched.</exception>
		/// <exception cref="System.ObjectDisposedException">This object has been Disposed of.</exception>
		private void StartDfo()
		{
			if ( m_disposed )
			{
				throw new ObjectDisposedException( "DfoLauncher" );
			}
			if ( Params == null )
			{
				throw new ArgumentNullException( "Params" );
			}
			if ( Params.LoginTimeoutInMs < 0 )
			{
				throw new ArgumentOutOfRangeException( "LoginTimeoutInMs cannot be negative." );
			}
			if ( Params.Username == null )
			{
				throw new ArgumentNullException( "Params.Username" );
			}
			if ( Params.Password == null )
			{
				throw new ArgumentNullException( "Params.Password" );
			}

			bool ok = EnforceWindowedSetting();
			if ( !ok )
			{
				return;
			}

			State = LaunchState.Login; // We are now logging in

			try
			{
				string dfoArg = DfoLogin.GetDfoArg( Params.Username, Params.Password, Params.LoginTimeoutInMs ); // Log in
				//string dfoArg = "abc"; // DEBUG
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

				lock ( m_syncHandle )
				{
					// Start the thread that monitors the state of DFO
					m_dfoMonitorThread = new Thread( BackgroundThreadEntryPoint );
					m_dfoMonitorThread.IsBackground = true;
					m_dfoMonitorThread.Name = "DFO monitor";

					// Give it a copy of the launch params so the caller can change the Params property while
					// the game is running with no effects for the next time they launch
					m_dfoMonitorThread.Start( Params.Clone() );
				}
			}
			catch ( System.Security.SecurityException ex )
			{
				throw new System.Security.SecurityException( string.Format(
					"This program does not have the permssions needed to log in to the game. {0}", ex.Message ), ex );
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
					"Error while starting DFO using {0}: {1}",
					Params.DfoLauncherExe, ex.Message ), ex );
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

		[DllImport( "user32.dll" )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool IsWindowVisible( IntPtr hWnd );

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

			lock ( m_syncHandle )
			{
				m_launcherProcess.Dispose();
				m_launcherProcess = null;
			}

			if ( setHandleIndex == 1 ) // canceled
			{
				canceled = true;
			}

			bool soundpacksSwitched = false;
			if ( !canceled )
			{
				// Switch soundpacks if we were told to do so
				if ( copiedParams.SwitchSoundpacks )
				{
					soundpacksSwitched = SwitchSoundpacks( copiedParams.SoundpackDir, copiedParams.CustomSoundpackDir, copiedParams.TempSoundpackDir );
				}
			}

			IntPtr dfoMainWindowHandle = IntPtr.Zero;
			if ( !canceled )
			{
				// Wait for DFO window to be created AND be visible, the DFO process to not exist, or a cancel notice.
				Pair<IntPtr, bool> pollResults = PollUntilCanceled<IntPtr>( copiedParams.GameWindowCreatedPollingIntervalInMs,
					() =>
					{
						IntPtr dfoWindowHandle = GetDfoWindowHandle( copiedParams );
						if ( dfoWindowHandle != IntPtr.Zero )
						{
							if ( DfoWindowIsOpen( dfoWindowHandle ) )
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // Window exists nd is visible, done polling
							}
							else
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, false ); // Window exists but is not visible yet, keep polling
							}
						}
						else
						{
							// Check if the DFO process is running. Under normal conditions, it certainly would be
							// by now because it gets started by the launcher process and the launcher process has ended.
							// If it is not, that means the launcher ended unsuccessfully or the DFO process
							// ended very quickly. In either case, our job is done here.

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
				if ( !canceled )
				{
					if ( pollResults.First == IntPtr.Zero )
					{
						canceled = true; // Treat a premature closing of the DFO process the same as a cancel request.
					}
				}
				dfoMainWindowHandle = pollResults.First; // Try not to use this handle - what if the DFO window closes and then some other window gets the same handle value?
			}

			if ( !canceled )
			{
				// Game is up.
				State = LaunchState.GameInProgress;

				// Wait for DFO game window to be closed or a cancel notice
				// Note that there is a distinction between a window existing and a window being visible.
				// When the popup is displayed, the DFO window still "exists", but it is hidden
				Pair<IntPtr, bool> pollResults = PollUntilCanceled<IntPtr>( copiedParams.GameDonePollingIntervalInMs,
					() =>
					{
						IntPtr dfoWindowHandle = GetDfoWindowHandle( copiedParams );
						if ( dfoWindowHandle == IntPtr.Zero )
						{
							return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // Window does not exist, done polling
						}
						else
						{
							if ( !DfoWindowIsOpen( dfoWindowHandle ) )
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // Window "exists" but is not visible, done polling
							}
							else
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, false ); // Window still open, keep polling
							}
						}
					} );

				canceled = pollResults.Second;
			}

			if ( !canceled )
			{
				if ( copiedParams.ClosePopup )
				{
					// Kill the DFO process to kill the popup.

					// A normal Process.Kill gets a Win32Exception with "Access is denied", possibly because
					// of HackShield.
					// This WMI stuff works, although I'm not entirely sure why.
					//
					// http://stackoverflow.com/questions/2069157/what-is-the-difference-between-these-two-methods-of-killing-a-process
					// - "WMI calls are not performed within the security context of your process. They are
					// handled in another process (I'm guessing the Winmgmt service). This service runs under
					// the SYSTEM account, and HackShield may be allowing the termination continue due to this."
					//
					// Thanks to Tomato (author of DFOAssist) for his help with this!
					try
					{
						ConnectionOptions options = new ConnectionOptions();
						options.Impersonation = ImpersonationLevel.Impersonate;
						ManagementScope scope = new ManagementScope( @"\\.\root\cimv2", options );
						scope.Connect();
						ObjectQuery dfoProcessQuery = new ObjectQuery(
							string.Format( "Select * from Win32_Process Where Name = '{0}'", Path.GetFileName( copiedParams.DfoExe ) ) );
						using ( ManagementObjectSearcher dfoProcessSearcher = new ManagementObjectSearcher( scope, dfoProcessQuery ) )
						using ( ManagementObjectCollection dfoProcessCollection = dfoProcessSearcher.Get() )
						{
							if ( dfoProcessCollection.Count == 0 )
							{
								OnPopupKillFailed( new ErrorEventArgs( new ManagementException( "No DFO processes found." ) ) );
							}
							else
							{
								foreach ( ManagementObject dfoProcess in dfoProcessCollection )
								{
									try
									{
										using ( dfoProcess )
										{
											object ret = dfoProcess.InvokeMethod( "Terminate", new object[] { } );
										}
									}
									catch ( ManagementException ex )
									{
										OnPopupKillFailed( new ErrorEventArgs( new ManagementException( string.Format(
											"Could not kill {0}: {1}", Path.GetFileName( copiedParams.DfoExe ), ex.Message ), ex ) ) );
									}
								}
							}
						}
					}
					catch ( ManagementException ex )
					{
						OnPopupKillFailed( new ErrorEventArgs( new ManagementException( string.Format(
							"Error while doing WMI stuff: {0}", ex.Message ), ex ) ) );
					}
				}
			}

			// Done, clean up.
			// Switch back soundpacks if they were switched
			if ( soundpacksSwitched )
			{
				// Wait for DFO process to end, otherwise the OS won't let us move the soundpack directory
				
				Process[] dfoProcesses;
				do
				{
					dfoProcesses = Process.GetProcessesByName( Path.GetFileNameWithoutExtension( copiedParams.DfoExe ) );
					if ( dfoProcesses.Length > 0 )
					{
						Thread.Sleep( copiedParams.GameDeadPollingIntervalInMs );
					}
				} while ( dfoProcesses.Length > 0 );

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
		/// For use by the monitor thread only. Poll for some value until the value is acceptable or we are
		/// canceled.
		/// </summary>
		/// <typeparam name="TReturn"></typeparam>
		/// <param name="pollingIntervalInMs"></param>
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

		private IntPtr GetDfoWindowHandle( LaunchParams copiedParams )
		{
			return FindWindow( copiedParams.DfoWindowClassName, null );
			//return FindWindow( null, "DFO" ); // DEBUG
		}

		private bool DfoWindowIsOpen( IntPtr dfoWindowHandle )
		{
			return IsWindowVisible( dfoWindowHandle );
		}

		private bool SwitchSoundpacks( string soundpackDir, string customSoundpackDir, string tempSoundpackDir )
		{
			bool firstMoveSuccessful = false;
			try
			{
				// Move soundpackDir to tempSoundpackDir
				Directory.Move( soundpackDir, tempSoundpackDir );
				firstMoveSuccessful = true;

				// Move customSoundpackDir to soundpackDir
				Directory.Move( customSoundpackDir, soundpackDir );
			}
			catch ( Exception ex )
			{
				// If the first move was successful, we can probably move it back
				bool undoSuccess = false;
				Exception undoError = null;
				if ( firstMoveSuccessful )
				{
					try
					{
						Directory.Move( tempSoundpackDir, soundpackDir );
						undoSuccess = true;
					}
					catch ( Exception ex2 )
					{
						undoError = ex2;
					}
				}

				ErrorEventArgs e;
				if ( !firstMoveSuccessful )
				{
					e = new ErrorEventArgs( new IOException( string.Format(
					"Could not move main soundpack directory {0} to temp soundpack directory {1}. {2}",
					soundpackDir, tempSoundpackDir, ex.Message ), ex ) );
				}
				else
				{
					string undoMessage;
					if ( undoSuccess )
					{
						undoMessage = string.Format(
						"Main soundpack in {0} successfully moved back to {1}. Soundpack directories are in a consistent state.",
						tempSoundpackDir, soundpackDir );
					}
					else
					{
						undoMessage = string.Format(
						"Main soundpack in {0} could not be moved back to {1}. {2} Soundpack directories are in an inconsistent state! You must manually fix them.",
						tempSoundpackDir, soundpackDir, undoError.Message );
					}
					e = new ErrorEventArgs( new IOException( string.Format(
					"Could not move custom soundpack directory {0} to main soundpack directory {1}. {2} {3}",
					customSoundpackDir, soundpackDir, ex.Message, undoMessage ), ex ) );
				}

				OnSoundpackSwitchFailed( e );

				return false;
			}

			return true;
		}

		private bool SwitchBackSoundpacks( string soundpackDir, string customSoundpackDir, string tempSoundpackDir )
		{
			bool firstMoveSuccessful = false;
			try
			{
				// Move soundpackDir to customSoundpackDir
				Directory.Move( soundpackDir, customSoundpackDir );
				firstMoveSuccessful = true;

				// Move tempSoundpackDir to soundpackDir
				Directory.Move( tempSoundpackDir, soundpackDir );
			}
			catch ( Exception ex )
			{
				ErrorEventArgs e;
				if ( !firstMoveSuccessful )
				{
					e = new ErrorEventArgs( new IOException( string.Format(
					"Could not move back custom soundpack in {0} back to {1}. {2}",
					soundpackDir, customSoundpackDir, ex.Message ), ex ) );
				}
				else
				{
					e = new ErrorEventArgs( new IOException( string.Format(
					"Could not move back main soundpack in {0} back to {1}. {2}",
					tempSoundpackDir, soundpackDir, ex.Message ), ex ) );
				}

				OnSoundpackSwitchFailed( e );

				return false;
			}
			return true;
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
		
		// The soundpack directories go through the following states:
		
		// normal soundpacks: soundpackDir
		// custom soundpacks: customSoundpackDir

		// Rename soundpackDir to tempSoundpackDir
		
		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: customSoundpackDir

		// Rename customSoundpackDir to soundpackDir

		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: soundpackDir

		// Game runs...
		// Game stops...

		// Rename soundpackDir to customSoundpackDir

		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: customSoundpackDir

		// Rename tempSoundpackDir to soundpackDir

		// normal soundpacks: soundpackDir
		// custom soundpacks: customSoundpackDir

		// That makes the abnormal states:
		//
		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: customSoundpackDir
		//
		// (this is the common (unfortunately) case of a crash while the game is running
		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: soundpackDir
		
		/// <summary>
		/// Checks if the soundpack directories (normal and custom) are not where they should be (perhaps because
		/// of a crash). This only checks if it is something that can be fixed by <c>FixBrokenSoundpacks()</c>.
		/// For example, a completely missing soundpack directory with no temp soundpack directory is not
		/// considered to be "broken". If the game process is currently running, the soundpacks are not
		/// considered to be "broken".
		/// </summary>
		/// <returns>True if the soundpack directories are in an abnormal state that can be fixed by
		/// <c>FixBrokenSoundpacks()</c>.</returns>
		public bool SoundpacksBroken()
		{
			Process[] dfoProcesses = Process.GetProcessesByName( Path.GetFileNameWithoutExtension( Params.DfoExe ) );
			if ( dfoProcesses.Length > 0 )
			{
				return false;
			}

			if ( Directory.Exists( Params.TempSoundpackDir ) && Directory.Exists( Params.SoundpackDir ) )
			{
				return true;
			}
			else if ( Directory.Exists( Params.TempSoundpackDir ) && Directory.Exists( Params.CustomSoundpackDir ) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Fixes soundpack directory mixup usually caused by a system or DFOCP crash. Does nothing if no problems
		/// are detected. Does nothing if the game is currently running.
		/// </summary>
		/// <exception cref="Sytem.IO.IOException">Something went wrong while trying to fix the mixup.</exception>
		public void FixBrokenSoundpacks()
		{
			if ( !SoundpacksBroken() )
			{
				return;
			}

			if ( Directory.Exists( Params.TempSoundpackDir ) && Directory.Exists( Params.SoundpackDir ) )
			{
				// Rename soundpackDir to customSoundpackDir
				try
				{
					Directory.Move( Params.SoundpackDir, Params.CustomSoundpackDir );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format(
						"Could not move back custom soundpack in {0} back to {1}. {2}",
						Params.SoundpackDir, Params.CustomSoundpackDir, ex.Message ),
						ex );
				}

				// Rename tempSoundpackDir to soundpackDir
				try
				{
					Directory.Move( Params.TempSoundpackDir, Params.SoundpackDir );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format(
						"Could not move back main soundpack in {0} back to {1}. {2}",
						Params.TempSoundpackDir, Params.SoundpackDir, ex.Message ),
						ex );
				}
			}
			else if ( Directory.Exists( Params.TempSoundpackDir ) && Directory.Exists( Params.CustomSoundpackDir ) )
			{
				// Rename tempSoundpackDir to soundpackDir
				try
				{
					Directory.Move( Params.TempSoundpackDir, Params.SoundpackDir );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format(
						"Could not move back main soundpack in {0} back to {1}. {2}",
						Params.TempSoundpackDir, Params.SoundpackDir, ex.Message ),
						ex );
				}
			}
			else
			{
				return;
			}
		}

		//public void ResizeDfoWindow( int x, int y )
		//{
		//    // TODO
		//}

		/// <summary>
		/// Frees unmanaged resources. This function may block.
		/// </summary>
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

/*
 Copyright 2010 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
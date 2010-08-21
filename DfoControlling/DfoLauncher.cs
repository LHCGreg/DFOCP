using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;
using Microsoft.Win32;
using System.ComponentModel;

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

		private LaunchParams m_workingParams = null;
		/// <summary>
		/// Gets or sets m_workingParams in a thread-safe manner. Properties of WorkingParams should not be set.
		/// It should be treated as read-only.
		/// </summary>
		private LaunchParams WorkingParams
		{
			get
			{
				lock ( m_syncHandle )
				{
					return m_workingParams;
				}
			}
			set
			{
				lock ( m_syncHandle )
				{
					m_workingParams = value;
				}
			}
		}

		private IntPtr m_gameWindowHandle = IntPtr.Zero;

		private IntPtr GameWindowHandle
		{
			get
			{
				lock ( m_syncHandle )
				{
					return m_gameWindowHandle;
				}
			}
			set
			{
				lock ( m_syncHandle )
				{
					m_gameWindowHandle = value;
				}
			}
		}

		private LaunchParams m_params = new LaunchParams();
		/// <summary>
		/// Gets or sets the parameters to use when launching the game.
		/// Changes will not effect an existing launch.
		/// </summary>
		/// <exception cref="ArgumentNullException">Attempted to set the value to null.</exception>
		public LaunchParams Params
		{
			get { return m_params; }
			set
			{
				value.ThrowIfNull( "Params" );
				m_params = value;
			}
		}

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
			else
			{
				Logging.Log.ErrorFormat( "Error while applying the window mode setting: {0}", e.Error.Message );
				Logging.Log.DebugFormat( "Exception detail: ", e.Error );
			}
		}
		#endregion

		/// <summary>
		/// Raised when a file requested to be switched could not be switched or switched back.
		/// </summary>
		public event EventHandler<ErrorEventArgs> FileSwitchFailed
		{
			add
			{
				lock ( m_FileSwitchFailedLock ) { m_FileSwitchFailedDelegate += value; }
			}
			remove
			{
				lock ( m_FileSwitchFailedLock ) { m_FileSwitchFailedDelegate -= value; }
			}
		}
		#region thread-safe event stuff
		private object m_FileSwitchFailedLock = new object();
		private EventHandler<ErrorEventArgs> m_FileSwitchFailedDelegate;

		/// <summary>
		/// Raises the <c>FileSwitchFailed</c> event.
		/// </summary>
		/// <param name="e">An <c>ErrorEventArgs</c> that contains the event data.</param>
		protected virtual void OnFileSwitchFailed( ErrorEventArgs e )
		{
			EventHandler<ErrorEventArgs> currentDelegate;
			lock ( m_FileSwitchFailedLock )
			{
				currentDelegate = m_FileSwitchFailedDelegate;
			}
			if ( currentDelegate != null )
			{
				currentDelegate( this, e );
			}
			else
			{
				Logging.Log.ErrorFormat( "Error while switching files: {0}", e.Error.Message );
				Logging.Log.DebugFormat( "Exception details: ", e.Error );
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
			else
			{
				Logging.Log.WarnFormat( "Error while trying to kill the ad popup: {0}", e.Error.Message );
				Logging.Log.DebugFormat( "Exception details: ", e.Error );
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
				LaunchState oldState = LaunchState.None;
				LaunchState newState = LaunchState.None;

				lock ( m_syncHandle )
				{
					if ( m_state != value )
					{
						oldState = m_state;
						newState = value;

						m_state = value;
						stateChanged = true;
					}
				}

				if ( stateChanged )
				{
					// Do logging outside of the lock
					Logging.Log.DebugFormat( "State change from {0} to {1}.", oldState, newState );
					OnLaunchStateChanged( EventArgs.Empty );
				}
			}
		}

		/// <summary>
		/// Constructs a new <c>DfoLauncher</c> with default parameters.
		/// </summary>
		public DfoLauncher()
		{
			;
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
			Logging.Log.DebugFormat( "Starting game. Launch parameters:{0}{1}",
					Environment.NewLine, Params );
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
		/// <exception cref="System.ArgumentNullException">Params.Username or Params.Password was null.</exception>
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
			if ( Params.LoginTimeoutInMs < 0 )
			{
				throw new ArgumentOutOfRangeException( "LoginTimeoutInMs cannot be negative." );
			}
			Params.Username.ThrowIfNull( "Params.Username" );
			Params.Password.ThrowIfNull( "Params.Password" );
			Params.FilesToSwitch.ThrowIfNull( "Params.FilesToSwitch" );

			bool ok = EnforceWindowedSetting();
			if ( !ok )
			{
				Logging.Log.DebugFormat( "Error while applying window mode setting; aborting launch." );
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
				m_launcherProcess.Exited += LauncherProcessExitedHandler; // Use async notification instead of synchronous waiting so we can cancel while the launcher process is going

				Logging.Log.DebugFormat( "Starting game process '{0}' with arguments '{1}'",
					m_launcherProcess.StartInfo.FileName,
					m_launcherProcess.StartInfo.Arguments.HideSensitiveData( SensitiveData.LoginCookies ) );

				m_launcherProcess.Start();

				lock ( m_syncHandle )
				{
					// Start the thread that monitors the state of DFO
					m_dfoMonitorThread = new Thread( BackgroundThreadEntryPoint );
					m_dfoMonitorThread.IsBackground = true;
					m_dfoMonitorThread.Name = "DFO monitor";

					Logging.Log.DebugFormat( "Starting monitor thread." );
					// Give it a copy of the launch params so the caller can change the Params property while
					// the game is running with no effects for the next time they launch
					WorkingParams = Params.Clone();
					m_dfoMonitorThread.Start();
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
			Logging.Log.DebugFormat( "Applying window mode setting." );
			string magicWindowModeDirectoryName = "zo3mo4";
			string magicWindowModeDirectoryPath = Path.Combine( Params.GameDir, magicWindowModeDirectoryName );
			Exception error = null;

			if ( Params.LaunchInWindowed )
			{
				Logging.Log.DebugFormat( "Forcing window mode by creating directory {0}.", magicWindowModeDirectoryPath );
				try
				{
					Directory.CreateDirectory( magicWindowModeDirectoryPath );
					Logging.Log.DebugFormat( "{0} created.", magicWindowModeDirectoryPath );
				}
				catch ( Exception ex )
				{
					if ( ex is System.IO.IOException || ex is System.UnauthorizedAccessException
					  || ex is System.ArgumentException || ex is System.IO.PathTooLongException
					  || ex is System.IO.DirectoryNotFoundException || ex is System.NotSupportedException )
					{
						error = new IOException( string.Format(
							"Error while trying to create directory {0}: {1}",
							magicWindowModeDirectoryPath, ex.Message ), ex );
					}
					else
					{
						throw;
					}
				}
			}
			else
			{
				Logging.Log.DebugFormat( "Forcing full-screen mode by deleting directory {0}.", magicWindowModeDirectoryPath );
				try
				{
					Directory.Delete( magicWindowModeDirectoryPath, true );
					Logging.Log.DebugFormat( "{0} removed.", magicWindowModeDirectoryPath );
				}
				catch ( DirectoryNotFoundException )
				{
					// It's ok if the directory doesn't exist
					Logging.Log.DebugFormat( "{0} does not exist.", magicWindowModeDirectoryPath );
				}
				catch ( Exception ex )
				{
					if ( ex is System.IO.IOException || ex is System.UnauthorizedAccessException
					  || ex is System.ArgumentException || ex is System.IO.PathTooLongException
					  || ex is System.ArgumentException )
					{
						error = new IOException( string.Format(
							"Error while trying to remove directory {0}: {1}",
							magicWindowModeDirectoryPath, ex.Message ), ex );
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

		/// <summary>
		/// Resets to an unattached state. This function may block if it needs to do any cleanup.
		/// </summary>
		public void Reset()
		{
			Logging.Log.DebugFormat( "Resetting to an unattached state." );

			if ( m_disposed )
			{
				Logging.Log.DebugFormat( "Already disposed so already unattached." );
				return;
			}

			// Send cancel signal to monitor thread if it's running and wait for it to finish terminating
			if ( MonitorThreadIsRunning() )
			{
				Logging.Log.DebugFormat( "Monitor thread is running, sending it a reset signal and waiting for it to finish." );
				m_monitorCancelEvent.Set();
				m_monitorFinishedEvent.WaitOne();
				// m_dfoMonitorThread got set to null as it was terminating itself
				// m_launcherProcess got disposed of and set to null as monitor thread was exiting if it hadn't done it already

				Logging.Log.DebugFormat( "Monitor thread reported completion." );
			}
			else
			{
				Logging.Log.DebugFormat( "Monitor thread not running. Setting state to None." );
				// If an exception happened while launching but before the monitor thread is started,
				// we need to set the state back to None. Normally the monitor thread does that as it's exiting.
				State = LaunchState.None;
				WorkingParams = null;
			}

			lock ( m_syncHandle )
			{
				// Need to set m_launcherProcess to null if there was an exception while launching.
				if ( m_launcherProcess != null )
				{
					Logging.Log.DebugFormat( "Disposing of the launcher process." );
					m_launcherProcess.Dispose();
					m_launcherProcess = null;
				}
			}

			Logging.Log.DebugFormat( "Reset complete." );
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

		[DllImport( "user32.dll", SetLastError = true )]
		static extern IntPtr FindWindow( string lpClassName, string lpWindowName );

		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool IsWindowVisible( IntPtr hWnd );

		/// <summary>
		/// Entry point for the monitor thread.
		/// </summary>
		private void BackgroundThreadEntryPoint()
		{
			Logging.Log.DebugFormat( "Monitor thread started." );

			LaunchParams copiedParams = WorkingParams;

			// Once this thread is created, it has full control over the State property. No other thread can
			// change it until this thread sets it back to None.

			// This thread can be canceled by calling the Reset() method. Reset() will put m_monitorCancelEvent
			// in the Set state.
			bool canceled = false;

			Logging.Log.DebugFormat( "Waiting for the game launcher process to stop or a cancel signal." );
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
				Logging.Log.DebugFormat( "Got cancel signal." );
				canceled = true;
			}
			else
			{
				Logging.Log.DebugFormat( "Launcher process ended." );
			}

			List<SwitchedFile> switchedFiles = new List<SwitchedFile>();
			if ( !canceled )
			{
				// Switch any files we need to
				foreach ( FileSwitcher fileToSwitch in copiedParams.FilesToSwitch )
				{
					switchedFiles.Add( SwitchFile( fileToSwitch ) );
				}
			}

			IntPtr dfoMainWindowHandle = IntPtr.Zero;
			if ( !canceled )
			{
				Logging.Log.DebugFormat( "Waiting for DFO window to be created AND be visible, the DFO process to not exist, or a cancel notice." );
				// Wait for DFO window to be created AND be visible, the DFO process to not exist, or a cancel notice.
				Pair<IntPtr, bool> pollResults = PollUntilCanceled<IntPtr>( copiedParams.GameWindowCreatedPollingIntervalInMs,
					() =>
					{
						IntPtr dfoWindowHandle = GetGameWindowHandle( copiedParams.GameToLaunch );
						if ( dfoWindowHandle != IntPtr.Zero )
						{
							if ( DfoWindowIsOpen( dfoWindowHandle ) )
							{
								return new Pair<IntPtr, bool>( dfoWindowHandle, true ); // Window exists and is visible, done polling
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

				if ( pollResults.Second )
				{
					Logging.Log.DebugFormat( "Received a cancel signal." );
				}
				else
				{
					if ( pollResults.First != IntPtr.Zero )
					{
						Logging.Log.DebugFormat( "Game window created and visible." );
					}
					else
					{
						Logging.Log.DebugFormat( "Game process does not exist." );
					}
				}

				canceled = pollResults.Second;
				if ( !canceled )
				{
					if ( pollResults.First == IntPtr.Zero )
					{
						canceled = true; // Treat a premature closing of the DFO process the same as a cancel request.
					}
				}

				GameWindowHandle = pollResults.First;
			}

			if ( !canceled )
			{
				// Game is up.
				State = LaunchState.GameInProgress;

				if ( copiedParams.LaunchInWindowed &&
					(copiedParams.WindowHeight.HasValue || copiedParams.WindowWidth.HasValue) )
				{
					const int defaultWidth = 640;
					const int defaultHeight = 480;
					const double defaultAspectRatio = ( ( (double)defaultWidth ) / ( (double)defaultHeight ) );

					int width = 0;
					int height = 0;

					if ( !copiedParams.WindowHeight.HasValue )
					{
						width = copiedParams.WindowWidth.Value;

						height = (int)( copiedParams.WindowWidth.Value * ( 1 / defaultAspectRatio ) );
						height = Math.Max( 1, height );
					}
					else if ( !copiedParams.WindowWidth.HasValue )
					{
						height = copiedParams.WindowHeight.Value;

						width = (int)( copiedParams.WindowHeight.Value * defaultAspectRatio );
						width = Math.Max( 1, width );
					}

					try
					{
						ResizeDfoWindow( width, height );
					}
					catch ( Win32Exception ex )
					{
						Logging.Log.ErrorFormat( "Could not resize the game window to {0}x{1}: {2}",
							width, height, ex.Message );
						Logging.Log.Debug( "Exception details:", ex );
					}
				}

				Logging.Log.DebugFormat( "Waiting for the main game window to not exist or to not be visible, or for a cancel signal." );
				// Wait for DFO game window to be closed or a cancel notice
				// Note that there is a distinction between a window existing and a window being visible.
				// When the popup is displayed, the DFO window still "exists", but it is hidden
				Pair<IntPtr, bool> pollResults = PollUntilCanceled<IntPtr>( copiedParams.GameDonePollingIntervalInMs,
					() =>
					{
						IntPtr dfoWindowHandle = GetGameWindowHandle( copiedParams.GameToLaunch );
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

				if ( pollResults.Second )
				{
					Logging.Log.DebugFormat( "Received a cancel signal." );
				}
				else
				{
					if ( pollResults.First != IntPtr.Zero )
					{
						Logging.Log.DebugFormat( "Game window exists but is not visible." );
					}
					else
					{
						Logging.Log.DebugFormat( "Game window does not exist." );
					}
				}

				canceled = pollResults.Second;
			}

			GameWindowHandle = IntPtr.Zero;

			if ( !canceled )
			{
				if ( copiedParams.ClosePopup )
				{
					// Kill the DFO process to kill the popup.
					Logging.Log.DebugFormat( "Killing the game process to kill the popup." );

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
							foreach ( ManagementObject dfoProcess in dfoProcessCollection )
							{
								try
								{
									using ( dfoProcess )
									{
										Logging.Log.DebugFormat( "Killing a game process." );
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
					catch ( ManagementException ex )
					{
						OnPopupKillFailed( new ErrorEventArgs( new ManagementException( string.Format(
							"Error while doing WMI stuff: {0}", ex.Message ), ex ) ) );
					}

					Logging.Log.DebugFormat( "Done killing popup." );
				}
			}

			// Done, clean up.
			// Switch back any switched files.
			if ( switchedFiles.Count > 0 )
			{
				// Wait for DFO process to end, otherwise the OS won't let us move the files that are used by the game

				string gameProcessName = Path.GetFileNameWithoutExtension( copiedParams.DfoExe );
				Logging.Log.DebugFormat( "Waiting for there to be no processes called '{0}' so that switched files can be switched back.", gameProcessName );

				Process[] dfoProcesses;
				do
				{
					dfoProcesses = Process.GetProcessesByName( gameProcessName );
					if ( dfoProcesses.Length > 0 )
					{
						Thread.Sleep( copiedParams.GameDeadPollingIntervalInMs );
					}
				} while ( dfoProcesses.Length > 0 );

				Logging.Log.DebugFormat( "No more proccesses called '{0}'; switching files back now.", gameProcessName );

				foreach ( SwitchedFile switchedFile in switchedFiles )
				{
					SwitchBackFile( switchedFile );
					switchedFile.Dispose();
				}
			}

			lock ( m_syncHandle )
			{
				m_dfoMonitorThread = null;
			}

			State = LaunchState.None;

			WorkingParams = null;

			m_monitorFinishedEvent.Set();

			Logging.Log.DebugFormat( "Monitor thread exiting." );
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

		/// <summary>
		/// Gets a window handle to the window of the given game.
		/// </summary>
		/// <param name="game"></param>
		/// <returns></returns>
		private IntPtr GetGameWindowHandle( Game game )
		{
			switch ( game )
			{
				case Game.DFO:
					return FindWindow( "DFO", null );
				//return FindWindow( null, "DFO" ); // DEBUG
				default:
					throw new Exception( "Oops, missed a game type." );
			}
		}

		private bool DfoWindowIsOpen( IntPtr dfoWindowHandle )
		{
			return IsWindowVisible( dfoWindowHandle );
		}

		/// <summary>
		/// Switches the given FileSwitcher. Returns the resulting SwitchedFile on success, calls
		/// OnFileSwitchFailed() and returns null on failure.
		/// </summary>
		/// <param name="fileToSwitch"></param>
		/// <returns></returns>
		private SwitchedFile SwitchFile( FileSwitcher fileToSwitch )
		{
			if ( fileToSwitch == null )
			{
				return null;
			}

			try
			{
				return fileToSwitch.Switch();
			}
			catch ( IOException ex )
			{
				OnFileSwitchFailed( new ErrorEventArgs( ex ) );
				return null;
			}
		}

		private void SwitchBackFile( SwitchedFile fileToSwitchBack )
		{
			if ( fileToSwitchBack == null )
			{
				return;
			}

			try
			{
				fileToSwitchBack.SwitchBack();
			}
			catch ( IOException ex )
			{
				OnFileSwitchFailed( new ErrorEventArgs( ex ) );
			}
		}

		private void LauncherProcessExitedHandler( object sender, EventArgs e )
		{
			bool launcherDoneSet = false;
			lock ( m_syncHandle )
			{
				if ( m_launcherProcess == sender )
				{
					m_launcherDoneEvent.Set();
					launcherDoneSet = true;
				}
			}

			if ( launcherDoneSet )
			{
				Logging.Log.DebugFormat( "Launcher process exited, launcher done event set." );
			}
			else
			{
				Logging.Log.DebugFormat( "A launcher process exited but does not match the saved launcher process." );
			}
		}

		[DllImport( "user32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
			uint uFlags );

		static readonly IntPtr HWND_TOPMOST = new IntPtr( -1 );
		static readonly IntPtr HWND_NOTOPMOST = new IntPtr( -2 );
		static readonly IntPtr HWND_TOP = new IntPtr( 0 );
		static readonly IntPtr HWND_BOTTOM = new IntPtr( 1 );

		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;
		const UInt32 SWP_NOZORDER = 0x0004;
		const UInt32 SWP_NOREDRAW = 0x0008;
		const UInt32 SWP_NOACTIVATE = 0x0010;
		const UInt32 SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
		const UInt32 SWP_DRAWFRAME = 0x0020;
		const UInt32 SWP_SHOWWINDOW = 0x0040;
		const UInt32 SWP_HIDEWINDOW = 0x0080;
		const UInt32 SWP_NOCOPYBITS = 0x0100;
		const UInt32 SWP_NOREPOSITION = 0x0200;
		const UInt32 SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
		const UInt32 SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
		const UInt32 SWP_DEFERERASE = 0x2000;
		const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;


		/// <summary>
		/// Resizes the game window.
		/// </summary>
		/// <param name="width">Width, in pixels, to resize to.</param>
		/// <param name="height">Height, in pixels, to resize to.</param>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/>
		/// is not a positive number.</exception>
		/// <exception cref="System.InvalidOperationException">This object is not currently attached to a
		/// game instance (State is not GameInProgress) or the game window does not exist.</exception>
		/// <exception cref="System.ComponentModel.Win32Exception">The resize operation failed.</exception>
		public void ResizeDfoWindow( int width, int height )
		{
			Logging.Log.DebugFormat( "Resizing game window to {0}x{1}.", width, height );

			if ( width <= 0 )
			{
				throw new ArgumentOutOfRangeException( "width", "Window width must be positive." );
			}
			if ( height <= 0 )
			{
				throw new ArgumentOutOfRangeException( "height", "Window height must be positive." );
			}

			IntPtr gameWindowHandle = GameWindowHandle;
			if ( gameWindowHandle == IntPtr.Zero )
			{
				if ( WorkingParams == null )
				{
					throw new InvalidOperationException( "Not attached to a game instance." );
				}
				else
				{
					throw new InvalidOperationException( "The game window does not exist." );
				}
			}

			ResizeWindow( gameWindowHandle, width, height );

			Logging.Log.DebugFormat( "Game window resized." );
		}

		/// <summary>
		/// Resizes the window with the given window handle to the given size. Arguments are not checked.
		/// </summary>
		/// <param name="windowHandle"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <exception cref="System.ComponentModel.Win32Exception">The resize operation failed.</exception>
		private void ResizeWindow( IntPtr windowHandle, int x, int y )
		{
			// TODO: center window
			bool success = SetWindowPos( windowHandle, IntPtr.Zero, 0, 0, x, y,
				SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOZORDER );
			if ( !success )
			{
				throw new Win32Exception(); // Magically gets the Windows error message from the SetWindowPos call
			}
		}

		/// <summary>
		/// Frees unmanaged resources. This function may block.
		/// </summary>
		public void Dispose()
		{
			Logging.Log.DebugFormat( "Disposing a DfoLauncher object." );
			if ( !m_disposed )
			{
				Reset();

				Logging.Log.DebugFormat( "Disposing of synchronization objects." );
				m_monitorCancelEvent.Close();
				m_monitorFinishedEvent.Close();
				m_launcherDoneEvent.Close();
				m_disposed = true;
				Logging.Log.DebugFormat( "DfoLauncher Dispose complete." );
			}
			else
			{
				Logging.Log.DebugFormat( "Already disposed." );
			}
		}

		/// <summary>
		/// Tries to figure out where the game directory for a given game is. The returned path is
		/// guaranteed to be a valid path string (no invalid characters).
		/// </summary>
		/// <exception cref="System.IO.IOException">The game directory could not be detected.</exception>
		public static string AutoDetectGameDir( Game game )
		{
			object gameRoot = null;

			string keyName;
			string valueName;

			if ( game == Game.DFO )
			{
				keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Nexon\DFO";
				valueName = "RootPath";
			}
			else
			{
				throw new Exception( "Oops, missed a game." );
			}

			Logging.Log.DebugFormat( "Detecting {0} directory by getting registry value '{1}' in registry key '{2}'",
				game, valueName, keyName );
			try
			{
				gameRoot = Registry.GetValue( keyName, valueName, null );
			}
			catch ( System.Security.SecurityException ex )
			{
				ThrowAutoDetectException( keyName, valueName, ex );
			}
			catch ( IOException ex )
			{
				ThrowAutoDetectException( keyName, valueName, ex );
			}

			if ( gameRoot == null )
			{
				ThrowAutoDetectException( keyName, valueName, "The registry value does not exist." );
			}

			string gameRootDir = gameRoot.ToString();
			if ( Utilities.PathIsValid( gameRootDir ) )
			{
				Logging.Log.DebugFormat( "Game directory is {0}", gameRootDir );
				return gameRootDir;
			}
			else
			{
				throw new IOException( string.Format( "Registry value {0} in {1} is not a valid path." ) );
			}
		}

		private static void ThrowAutoDetectException( string keyname, string valueName, Exception ex )
		{
			throw new IOException( string.Format( "Could not read registry value {0} in {1}. {2}",
				valueName, keyname, ex.Message ), ex );
		}

		private static void ThrowAutoDetectException( string keyname, string valueName, string message )
		{
			throw new IOException( string.Format( "Could not read registry value {0} in {1}. {2}",
				valueName, keyname, message ) );
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
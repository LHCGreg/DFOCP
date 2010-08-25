using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Dfo.Controlling;
using System.IO;
using System.Linq;

namespace Dfo.ControlPanel
{
	public partial class ctlMainForm : Form
	{
		private NotifyIcon m_notifyIcon = new NotifyIcon();
		private FormWindowState m_stateToRestoreTo = FormWindowState.Normal;

		private bool m_preventLaunch = false;
		/// <summary>
		/// Use to prevent the user from launching the game after they start launching
		/// to prevent multiple launch attempts.
		/// </summary>
		private bool PreventLaunch { get { return m_preventLaunch; } set { m_preventLaunch = value; UpdateLaunchVisibility(); } }

		// Thread-safe
		private void UpdateLaunchVisibility()
		{
			if ( PreventLaunch )
			{
				ctlLaunch.InvokeIfRequiredAsync( () => ctlLaunch.Enabled = false );
			}
			else
			{
				ctlLaunch.InvokeIfRequiredAsync( () => ctlLaunch.Enabled = ( Username != "" && Password != "" ) );
			}
		}

		private DfoLauncher m_launcher = new DfoLauncher();
		private string m_stateNoneText = "Ready"; // So the text for this only needs to be changed in one place.
		private Thread m_launcherThread = null;
		private AutoResetEvent m_stateBecameNoneEvent = new AutoResetEvent( false ); // State change thread -> launcher thread
		private AutoResetEvent m_launcherThreadCanceledEvent = new AutoResetEvent( false ); // UI thread -> launcher thread
		private ManualResetEvent m_launcherThreadFinishedEvent = new ManualResetEvent( true ); // launcher thread -> UI thread
		private object m_syncHandle = new object();
		private bool m_closeWhenDone = false;
		private CommandLineArgs m_parsedArgs;
		private StartupSettings m_savedSettings = new StartupSettings();

		private static readonly string s_exportEnabledTooltip = "Creates a .bat file you can double-click to start the game immediately.";
		private static readonly string s_exportDisabledTooltip = s_exportEnabledTooltip + " You must enter a username and password.";

		private bool ClosePopup { get { return ctlClosePopup.Checked; } set { ctlClosePopup.Checked = value; } }
		private bool LaunchWindowed { get { return ctlLaunchWindowed.Checked; } set { ctlLaunchWindowed.Checked = value; } }
		private bool RememberMe { get { return ctlRememberMe.Checked; } set { ctlRememberMe.Checked = value; } }

		// Modify this method to bind a new switchable file to a checkbox.
		private IDictionary<string, CheckBox> GetSwitchableCheckboxes()
		{
			return new Dictionary<string, CheckBox>()
			{
				{ SwitchableFile.Soundpacks, ctlSwitchSoundpacks },
				{ SwitchableFile.AudioXml, ctlSwitchAudioXml },
			};
		}

		private Dictionary<string, IUiSwitchableFile> m_switchableFiles = new Dictionary<string, IUiSwitchableFile>();
		private Dictionary<string, IUiSwitchableFile> SwitchableFiles { get { return m_switchableFiles; } }

		private ICollection<FileSwitcher> GetFileSwitchers()
		{
			return new List<FileSwitcher>( from file in SwitchableFiles.Values
										   where file.Switch
										   select file.AsFileSwitcher() );
		}

		private string Username { get { return ctlUsername.Text; } set { ctlUsername.Text = value; } }
		private string Password { get { return ctlPassword.Text; } set { ctlPassword.Text = value; } }

		private string DefaultDfoDir { get { return AppDomain.CurrentDomain.BaseDirectory; } }
		private string m_dfoDir;
		/// <summary>
		/// Gets or sets the base directory to use for relative paths.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">This property is attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">This property is attempted to be set to a path containing
		/// invalid characters.</exception>
		private string DfoDir // Gets set when applying settings
		{
			get
			{
				return m_dfoDir;
			}
			set
			{
				ValidatePath( value, "DfoDir" );
				m_dfoDir = value;
				foreach ( IUiSwitchableFile switchableFile in SwitchableFiles.Values )
				{
					switchableFile.RelativeRoot = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the width to start the game window at if starting in windowed mode. Setting to a value
		/// outside the allowed range has undefined behavior.
		/// </summary>
		private int? GameWindowStartingWidth
		{
			get
			{
				// Use value from width textbox if valid, otherwise use slider value
				int widthFromTextbox;
				if ( int.TryParse( ctlWindowWidth.Text, out widthFromTextbox ) )
				{
					if ( widthFromTextbox > ctlWindowSizeSlider.Maximum ||
						widthFromTextbox < ctlWindowSizeSlider.Minimum )
					{
						return ctlWindowSizeSlider.Value;
					}
					else
					{
						return widthFromTextbox;
					}
				}
				else
				{
					return ctlWindowSizeSlider.Value;
				}
			}
			set
			{
				if ( value != null )
				{
					//int valueToSetTo = value.Value.Clamp( ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );
					//ctlWindowSizeSlider.Value = valueToSetTo;
					ctlWindowWidth.Text = value.Value.ToString();
				}
				else
				{
					ctlWindowSizeSlider.Value = DfoLauncher.DefaultGameWindowWidth;
				}
			}
		}

		private int? GameWindowStartingHeight
		{
			get
			{
				// Use value from height textbox if valid, otherwise use slider value
				int widthFromTextbox;
				if ( int.TryParse( ctlWindowWidth.Text, out widthFromTextbox ) )
				{
					if ( widthFromTextbox > ctlWindowSizeSlider.Maximum ||
						widthFromTextbox < ctlWindowSizeSlider.Minimum )
					{
						return DfoLauncher.GetHeightFromWidth( ctlWindowSizeSlider.Value );
					}
					else
					{
						//return widthFromTextbox;
						int heightFromTextbox;
						if ( int.TryParse( ctlWindowHeight.Text, out heightFromTextbox ) )
						{
							return heightFromTextbox;
						}
						else
						{
							return DfoLauncher.GetHeightFromWidth( ctlWindowSizeSlider.Value );
						}
					}
				}
				else
				{
					return DfoLauncher.GetHeightFromWidth( ctlWindowSizeSlider.Value );
				}
			}
			set
			{
				if ( value != null )
				{
					ctlWindowHeight.Text = value.ToString();
				}
			}
		}

		private int? GameWindowWidth { get { return GameWindowStartingWidth; } set { GameWindowStartingWidth = value; } }
		private int? GameWindowHeight { get { return GameWindowStartingHeight; } set { GameWindowStartingHeight = value; } }

		/// <summary>
		/// Autodetects the DFO directory and sets DfoDir.
		/// </summary>
		/// <exception cref="System.IO.IOException">The DFO directory could not be detected.</exception>
		private void AutoDetectDfoDir()
		{
			DfoDir = DfoLauncher.AutoDetectGameDir( Game.DFO );
		}

		/// <summary>
		/// Throws an ArgumentException if <paramref name="path"/> contains invalid characters.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="propertyName">Argument name to pass to ArgumentException.</param>
		private static void ValidatePath( string path, string propertyName )
		{
			if ( path == null )
			{
				throw new ArgumentNullException( propertyName );
			}
			if ( !Utilities.PathIsValid( path ) )
			{
				throw new ArgumentException( string.Format(
					"{0} contains characters that are invalid in a path.", path ) );
			}
		}

		// Prefer Control.BeginInvoke to Control.Invoke. Using Control.Invoke when the UI thread is waiting for the
		// current thread to finish results in a deadlock.

		private void SetLauncherParams()
		{
			// Sets the launcher params to the values of the GUI controls. This is easier and more efficient
			// than handling the "changed" events of all the relevant controls

			m_launcher.Params.ClosePopup = ClosePopup;
			m_launcher.Params.GameDir = DfoDir;
			m_launcher.Params.LaunchInWindowed = LaunchWindowed;
			m_launcher.Params.Password = Password;
			m_launcher.Params.FilesToSwitch = GetFileSwitchers();
			m_launcher.Params.Username = Username;
			m_launcher.Params.WindowWidth = GameWindowStartingWidth;
			m_launcher.Params.WindowHeight = GameWindowStartingHeight;

			Logging.Log.DebugFormat( "GameWindowStartingWidth = {0}, textbox value = {1}, slider value = {2}",
				m_launcher.Params.WindowWidth, ctlWindowWidth.Text, ctlWindowSizeSlider.Value );
			Logging.Log.DebugFormat( "GameWindowStartingHeight = {0}, textbox value = {1}",
				m_launcher.Params.WindowHeight, ctlWindowHeight.Text );
		}

		// Left for designer support only
		[Obsolete( "The default constructor is for designer support only.", true )]
		public ctlMainForm()
		{
			InitializeComponent();
		}

		internal ctlMainForm( CommandLineArgs parsedArgs )
		{
			Logging.Log.Debug( "Creating main window." );
			InitializeComponent();

			m_notifyIcon.Icon = this.Icon;
			m_notifyIcon.Text = "DFO Control Panel";
			m_notifyIcon.DoubleClick += ( object sender, EventArgs e ) =>
				{
					this.ShowInTaskbar = true;
					this.WindowState = m_stateToRestoreTo;
				};

			exportToolStripMenuItem.ToolTipText = s_exportDisabledTooltip;
			UpdateLaunchVisibility();

			m_parsedArgs = parsedArgs;

			m_launcher.WindowModeFailed += WindowModeFailedHandler;
			m_launcher.LaunchStateChanged += StateChangedHandler;
			m_launcher.FileSwitchFailed += FileSwitchFailedHandler;
			m_launcher.PopupKillFailed += PopupKillFailedHandler;

			ctlStatusLabel.Text = m_stateNoneText;

			Logging.Log.Debug( "Main window created." );
		}

		private void ctlMainForm_Load( object sender, EventArgs e )
		{
			Logging.Log.Debug( "Loading main window." );

			Rectangle primaryScreenSize = Screen.PrimaryScreen.Bounds;
			Logging.Log.DebugFormat( "Primary screen size is {0}x{1}",
				primaryScreenSize.Width, primaryScreenSize.Height );

			int maxWidth = Math.Min( primaryScreenSize.Width, DfoLauncher.GetWidthFromHeight( primaryScreenSize.Height ) );
			ctlWindowSizeSlider.Minimum = DfoLauncher.DefaultGameWindowWidth;
			ctlWindowSizeSlider.Maximum = Math.Max( maxWidth, DfoLauncher.DefaultGameWindowWidth );

			Logging.Log.DebugFormat( "Allowable game window width: {0}-{1}",
				ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );

			// Initialize window size UI
			ctlWindowSizeSlider_ValueChanged( ctlWindowSizeSlider, EventArgs.Empty );

			// Bind switchable files to checkboxes
			IDictionary<string, CheckBox> switchableFileCheckboxes = GetSwitchableCheckboxes();
			foreach ( ISwitchableFile switchableFile in m_parsedArgs.Settings.SwitchableFiles.Values )
			{
				if ( switchableFileCheckboxes.ContainsKey( switchableFile.Name ) )
				{
					SwitchableFiles.Add( switchableFile.Name,
						new CheckboxSwitchableFile( switchableFileCheckboxes[ switchableFile.Name ],
							switchableFile ) );
				}
				else
				{
					// Hmmm...No UI binding for a switchable file. Add it as a switchable file and let
					// setting/command-line behavior take effect.
					SwitchableFiles.Add( switchableFile.Name, new PlainUiSwitchableFile( switchableFile ) );
				}
			}

			m_savedSettings = SettingsLoader.Load();
			ApplySettingsAndArguments();

			FixSwitchableFilesIfNeeded();

			ctlUsername.Select();
			Logging.Log.Debug( "Main window loaded." );
		}

		/// <summary>
		/// Attempts to fix mixed-up switchable files usually caused by a system crash while the game is running.
		/// The SwitchableFiles collection is used. All switchable files are attempted to be fixed even if
		/// they are not currently selected to be switched.
		/// </summary>
		private void FixSwitchableFilesIfNeeded()
		{
			Logging.Log.DebugFormat( "Checking all switchable files." );

			bool anyFailed = false;
			bool anyFixed = false;
			foreach ( IUiSwitchableFile switchableFile in SwitchableFiles.Values )
			{
				try
				{
					bool wasBroken;
					switchableFile.FixBrokenFilesIfNeeded( out wasBroken );
					if ( wasBroken )
					{
						switchableFile.Refresh();
						anyFixed = true;
					}
				}
				catch ( IOException ex )
				{
					// XXX: Should the program exit?
					anyFailed = true;
					switchableFile.Refresh();
					DisplayError( string.Format(
						"Error while trying to fix switchable files. {0} I guess you'll have to fix them yourself.",
						ex.Message ),
						"Couldn't fix switchable files" );

				}
			}

			if ( anyFixed && !anyFailed )
			{
				DisplayInfo( "Some switchable files were detected to be mixed up (this is usually caused by a system crash). They have been fixed.",
						"Switchable files fixed" );
			}

			Logging.Log.DebugFormat( "Done checking switchable files." );
		}

		/// <summary>
		/// Applies the command-line arguments and saved settings to the form's properties.
		/// Command-line arguments take precedence, following by saved settings, followed by a default.
		/// </summary>
		private void ApplySettingsAndArguments()
		{
			Logging.Log.Debug( "Applying settings and arguments." );

			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.ClosePopup, m_savedSettings.ClosePopup, null,
				"Close popup", ( bool closePopup ) => ClosePopup = closePopup, m_launcher.Params.ClosePopup,
				SensitiveData.None );

			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.LaunchWindowed, m_savedSettings.LaunchWindowed, null,
				"Launch windowed", ( bool windowed ) => LaunchWindowed = windowed, false, SensitiveData.None );

			SettingsLoader.ApplySettingStruct( null, m_savedSettings.RememberUsername, null, "Remember username",
				( bool remember ) => RememberMe = remember, false, SensitiveData.None );

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.Username, m_savedSettings.Username, null,
				"Username", ( string user ) => Username = user, "", SensitiveData.Usernames );

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.Password, m_savedSettings.Password, null,
				"Password", ( string pass ) => Password = pass, "", SensitiveData.Passwords );

			Func<int, string> validateWindowSize = ( int size ) =>
			{
				if ( size > 0 )
				{
					return null;
				}
				else
				{
					return string.Format( "{0} is not a positive integer.", size );
				}
			};

			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.GameWindowWidth, m_savedSettings.GameWindowWidth,
				validateWindowSize, "Starting game window width", ( int width ) => GameWindowStartingWidth = width,
				DfoLauncher.DefaultGameWindowWidth, SensitiveData.None );

			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.GameWindowHeight,
				m_savedSettings.GameWindowHeight, validateWindowSize, "Starting game window height",
				( int height ) => GameWindowStartingHeight = height,
				DfoLauncher.DefaultGameWindowHeight, SensitiveData.None );

			Func<string, string> validatePath = ( string dir ) =>
			{
				if ( Utilities.PathIsValid( dir ) )
				{
					return null;
				}
				else
				{
					return string.Format( "{0} is not a valid path.", dir );
				}
			};

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.DfoDir, m_savedSettings.DfoDir, validatePath,
				"DFO directory", ( string dfodir ) => { if ( dfodir != null ) DfoDir = dfodir; }, null,
				SensitiveData.None );

			if ( DfoDir == null )
			{
				try
				{
					AutoDetectDfoDir();
				}
				catch ( IOException ex )
				{
					Logging.Log.ErrorFormat( "Could not autodetect the DFO directory. {0} Using {1} as a fallback.",
						ex.Message, DefaultDfoDir );
					DfoDir = DefaultDfoDir;
				}
			}

			foreach ( string switchableName in m_parsedArgs.Settings.SwitchableFiles.Keys )
			{
				ISwitchableFile switchableFromArgs = m_parsedArgs.Settings.SwitchableFiles[ switchableName ];
				bool? switchFromArgs = m_parsedArgs.Settings.SwitchFile[ switchableName ];
				ISwitchableFile switchableFromSettings = m_savedSettings.SwitchableFiles[ switchableName ];
				bool? switchFromSettings = m_savedSettings.SwitchFile[ switchableName ];

				SwitchableFiles[ switchableName ].RelativeRoot = DfoDir;

				SettingsLoader.ApplySettingClass( switchableFromArgs.CustomFile, switchableFromSettings.CustomFile,
					validatePath,
					string.Format( "Custom file for {0}", switchableFromArgs.NormalFile ),
					( string customFile ) => SwitchableFiles[ switchableName ].CustomFile = customFile,
					switchableFromArgs.DefaultCustomFile, SensitiveData.None );

				SettingsLoader.ApplySettingClass( switchableFromArgs.TempFile, switchableFromSettings.TempFile,
					validatePath,
					string.Format( "Temp file for {0}", switchableFromArgs.NormalFile ),
					( string tempFile ) => SwitchableFiles[ switchableName ].TempFile = tempFile,
					switchableFromArgs.DefaultTempFile, SensitiveData.None );

				SettingsLoader.ApplySettingStruct( switchFromArgs, switchFromSettings, null,
					string.Format( "Switch {0}", switchableFromArgs.NormalFile ),
					( bool switchFile ) => SwitchableFiles[ switchableName ].SwitchIfFilesOk = switchFile,
					false, SensitiveData.None );
			}

			Logging.Log.Debug( "Done applying settings and arguments." );
		}

		private void WindowModeFailedHandler( object sender, CancelErrorEventArgs e )
		{
			DisplayError( string.Format( "The window mode setting could not be applied. {0}", e.Error.Message ),
				"Windowed Setting Error" );
			e.Cancel = true;
		}

		private void FileSwitchFailedHandler( object sender, Dfo.Controlling.ErrorEventArgs e )
		{
			DisplayError( string.Format( "Switching file failed. {0}", e.Error.Message ),
				"File Switch Error" );
		}

		private void PopupKillFailedHandler( object sender, Dfo.Controlling.ErrorEventArgs e )
		{
			Logging.Log.Warn( string.Format( "Could not kill the popup. {0}", e.Error.Message ) );
		}

		private void StateChangedHandler( object sender, EventArgs e )
		{
			LaunchState currentState = ( (DfoLauncher)sender ).State;
			Logging.Log.DebugFormat( "Handling state change to {0}.", currentState );

			switch ( currentState )
			{
				case LaunchState.None:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = m_stateNoneText );
					ctlResizeButton.BeginInvoke( () => ctlResizeButton.Visible = false );
					lock ( m_syncHandle )
					{
						if ( !m_closeWhenDone )
						{
							this.BeginInvoke( () => this.WindowState = m_stateToRestoreTo );
						}
					}
					m_stateBecameNoneEvent.Set();
					break;
				case LaunchState.Login:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Logging in..." );
					break;
				case LaunchState.Launching:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Launching..." );
					this.BeginInvoke( () => this.WindowState = FormWindowState.Minimized );
					break;
				case LaunchState.GameInProgress:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Game in progress" );
					ctlResizeButton.BeginInvoke( () => ctlResizeButton.Visible = true );
					lock ( m_syncHandle )
					{
						m_closeWhenDone = true;
					}
					break;
			}

			if ( currentState == LaunchState.Login )
			{
				ShowProgressBar();
			}
			else
			{
				HideProgressBar();
			}
		}

		/// <summary>
		/// Shows an "unknown progress" progress bar. Can be called outside the UI thread.
		/// </summary>
		private void ShowProgressBar()
		{
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Visible = true );
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Style = ProgressBarStyle.Marquee );
		}

		/// <summary>
		/// Hides the progress bar from ShowProgressBar(). Can be called outside the UI thread.
		/// </summary>
		private void HideProgressBar()
		{
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Visible = false );
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Style = ProgressBarStyle.Blocks ); // Don't animate when it's not visible - dunno if this actually saves any cpu or not
		}

		private void ctlLaunch_Click( object sender, EventArgs e )
		{
			Logging.Log.DebugFormat( "Launch button clicked." );
			PreventLaunch = true;

			SetLauncherParams();

			m_launcherThreadCanceledEvent.Reset();

			m_launcherThread = new Thread( LaunchThreadStart );
			m_launcherThread.IsBackground = true;
			m_launcherThread.Name = "Launcher";

			Logging.Log.DebugFormat( "Starting launch thread." );
			m_launcherThread.Start();
		}

		private void LaunchThreadStart()
		{
			Logging.Log.Debug( "Launch thread started." );

			m_stateBecameNoneEvent.Reset();

			try
			{
				m_launcher.Launch();
			}
			catch ( Exception ex )
			{
				if ( ex is System.Security.SecurityException
				  || ex is System.Net.WebException
				  || ex is DfoAuthenticationException
				  || ex is DfoLaunchException )
				{
					DisplayError( ex.Message, "Launch Error" );
				}
				else
				{
					throw;
				}
			}

			// Wait for the launcher state to become None or a cancel request. Then we can enable the button again.
			if ( m_launcher.State != LaunchState.None )
			{
				Logging.Log.Debug( "Waiting for state to become None or a cancel request." );
				int conditionIndex = WaitHandle.WaitAny(
					new WaitHandle[] { m_stateBecameNoneEvent, m_launcherThreadCanceledEvent } );
				if ( conditionIndex == 0 )
				{
					Logging.Log.Debug( "State became None, done waiting." );
				}
				else
				{
					Logging.Log.Debug( "Canceled, done waiting." );
				}

				m_launcher.Reset();
				Logging.Log.Debug( "Launcher object reset." );
			}

			PreventLaunch = false;

			lock ( m_syncHandle )
			{
				if ( m_closeWhenDone )
				{
					// This has to be an async invoke because the form closing handler waits for this thread to finish!
					this.BeginInvoke( () => this.Close() );
				}
			}

			Logging.Log.Debug( "End of launch thread." );
		}

		/// <summary>
		/// Displays an error message.
		/// </summary>
		/// <param name="errorMessage"></param>
		/// <param name="secondaryText"></param>
		private void DisplayError( string errorMessage, string secondaryText )
		{
			Logging.Log.Error( errorMessage );
			MessageBox.Show( errorMessage, secondaryText, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}

		/// <summary>
		/// Displays an informational message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="secondaryText"></param>
		private void DisplayInfo( string message, string secondaryText )
		{
			Logging.Log.Info( message );
			MessageBox.Show( message, secondaryText, MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

		/// <summary>
		/// Shows the user a message and gets either an "OK" response or a "Cancel" response.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="secondaryText"></param>
		/// <param name="defaultChoice"></param>
		/// <returns></returns>
		private bool GetOkCancel( string message, string secondaryText, bool defaultChoice )
		{
			MessageBoxDefaultButton defaultButton;
			if ( defaultChoice == true )
			{
				defaultButton = MessageBoxDefaultButton.Button1;
			}
			else
			{
				defaultButton = MessageBoxDefaultButton.Button2;
			}

			DialogResult result = MessageBox.Show( message, secondaryText, MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, defaultButton );

			if ( result == DialogResult.OK )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private void ctlMainForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			if ( m_launcher.State == LaunchState.GameInProgress && m_launcher.Params.FilesToSwitch.Count > 0 )
			{
				DisplayError( "You cannot close this program while the game is in progress if you chose to switch any files. You must wait until the game finishes so the files can be switched back.",
					"Sorry, you can't exit yet" );
				e.Cancel = true;
				return;
			}

			Logging.Log.Debug( "Main window closing." );

			m_launcherThreadCanceledEvent.Set();
			if ( m_launcherThread != null )
			{
				Logging.Log.Debug( "Waiting for launcher thread to end." );
				m_launcherThread.Join();
				Logging.Log.Debug( "Launcher thread joined." );
			}

			SettingsLoader.Save( GetCurrentSettings() );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			// Tray icon housekeeping
			if ( this.WindowState == FormWindowState.Minimized )
			{
				// Minimize to tray
				m_notifyIcon.Visible = true;
				this.ShowInTaskbar = false;
			}
			else
			{
				// Remove from tray when not minimized
				this.ShowInTaskbar = true;
				m_notifyIcon.Visible = false;
				m_stateToRestoreTo = this.WindowState;
			}
			base.OnSizeChanged( e );
		}

		private void exitToolStripMenuItem_Click( object sender, EventArgs e )
		{
			this.Close();
		}

		private void aboutToolStripMenuItem_Click( object sender, EventArgs e )
		{
			using ( About aboutBox = new About() )
			{
				aboutBox.ShowDialog();
			}
		}

		private void ctlUsername_TextChanged( object sender, EventArgs e )
		{
			UpdateSaveAsVisibility();
			UpdateLaunchVisibility();
		}

		private void ctlPassword_TextChanged( object sender, EventArgs e )
		{
			UpdateSaveAsVisibility();
			UpdateLaunchVisibility();
		}

		/// <summary>
		/// Disables the "export to .bat" menu item if all required fields are not filled out or enables it
		/// if everything required for exporting is ok. NOT THREAD-SAFE.
		/// </summary>
		private void UpdateSaveAsVisibility()
		{
			if ( Username == string.Empty || Password == string.Empty )
			{
				exportToolStripMenuItem.Enabled = false;
			}
			else
			{
				exportToolStripMenuItem.Enabled = true;
			}
		}

		private void exportToolStripMenuItem_EnabledChanged( object sender, EventArgs e )
		{
			if ( exportToolStripMenuItem.Enabled )
			{
				exportToolStripMenuItem.ToolTipText = s_exportEnabledTooltip;
			}
			else
			{
				exportToolStripMenuItem.ToolTipText = s_exportDisabledTooltip;
			}
		}

		private void exportToolStripMenuItem_Click( object sender, EventArgs e )
		{
			bool userUnderstandsRisks = GetOkCancel(
				"This will save your username and password in a text file. Anyone who has access to your computer will be able to see your username and password if they open the file. Are you sure you want to continue?",
				"Security Warning!",
				false );

			if ( !userUnderstandsRisks )
			{
				return;
			}

			using ( SaveFileDialog fileDialog = new SaveFileDialog() )
			{
				fileDialog.AddExtension = true;
				fileDialog.DefaultExt = "bat";
				fileDialog.Filter = "Batch Scripts (*.bat)|*.bat";
				fileDialog.InitialDirectory = Application.StartupPath;
				DialogResult result = fileDialog.ShowDialog();

				if ( result == DialogResult.OK )
				{
					Logging.Log.InfoFormat( "Creating batch script {0}...", fileDialog.FileName );
					try
					{
						SettingsExporter.ExportToBat( GetCurrentSettings(), fileDialog.FileName );
						Logging.Log.InfoFormat( "Batch script saved to {0}.", fileDialog.FileName );
					}
					catch ( IOException ex )
					{
						DisplayError( ex.Message, "Error while saving" );
					}
				}
			}
		}

		private StartupSettings GetCurrentSettings()
		{
			StartupSettings settings = new StartupSettings();

			if ( m_parsedArgs.Settings.DfoDir != null )
			{
				settings.DfoDir = DfoDir;
			}

			foreach ( IUiSwitchableFile uiBoundFile in SwitchableFiles.Values )
			{
				SwitchableFile switchableFile = new SwitchableFile( uiBoundFile );
				if ( m_parsedArgs.Settings.SwitchableFiles[ switchableFile.Name ].CustomFile == null )
				{
					// Custom file was not specified on commmand-line and there is no UI, so the user wants
					// the default.
					switchableFile.CustomFile = null;
				}
				if ( m_parsedArgs.Settings.SwitchableFiles[ switchableFile.Name ].TempFile == null )
				{
					// ditto
					switchableFile.TempFile = null;
				}

				settings.SwitchableFiles[ switchableFile.Name ] = switchableFile;
				settings.SwitchFile[ switchableFile.Name ] = switchableFile.Switch;
			}

			settings.ClosePopup = ClosePopup;
			settings.LaunchWindowed = LaunchWindowed;
			settings.Password = Password;
			settings.RememberUsername = RememberMe;
			settings.Username = Username;
			settings.GameWindowWidth = GameWindowStartingWidth;
			settings.GameWindowHeight = GameWindowStartingHeight;

			return settings;
		}

		private void ctlWindowSizeSlider_ValueChanged( object sender, EventArgs e )
		{
			if ( !m_widthBeingEdited && !m_heightBeingEdited )
			{
				int newWidth = ctlWindowSizeSlider.Value;
				int widthFromTextbox;
				bool textboxHasInt = int.TryParse( ctlWindowWidth.Text, out widthFromTextbox );
				if ( !textboxHasInt || widthFromTextbox != newWidth )
				{
					ctlWindowWidth.Text = newWidth.ToString();
				}

				int newHeight = DfoLauncher.GetHeightFromWidth( ctlWindowSizeSlider.Value );
				int heightFromTextbox;
				textboxHasInt = int.TryParse( ctlWindowHeight.Text, out heightFromTextbox );
				if ( !textboxHasInt || heightFromTextbox != newHeight )
				{
					ctlWindowHeight.Text = newHeight.ToString();
				}
			}
		}

		private bool m_widthBeingEdited = false;
		private void ctlWindowWidth_TextChanged( object sender, EventArgs e )
		{
			m_widthBeingEdited = true;
			// Update the height textbox
			int newWidth;
			if ( int.TryParse( ctlWindowWidth.Text, out newWidth ) )
			{
				//GameWindowStartingWidth = newWidth;
				if ( !m_heightBeingEdited )
				{
					ctlWindowHeight.Text = DfoLauncher.GetHeightFromWidth( newWidth ).ToString();

					// Update the slider
					//newWidth = newWidth.Clamp( ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );
					ctlWindowSizeSlider.Value = newWidth.Clamp( ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );
				}
			}

			m_widthBeingEdited = false;
		}

		private void ctlWindowWidth_Leave( object sender, EventArgs e )
		{
			// Update the slider (and indirectly, the height textbox)
			int newWidth;
			if ( int.TryParse( ctlWindowWidth.Text, out newWidth ) )
			{
				newWidth = newWidth.Clamp( ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );

				ctlWindowSizeSlider.Value = newWidth;

				// Set the text to a canonical text
				ctlWindowWidth.Text = ctlWindowSizeSlider.Value.ToString();
			}
			else
			{
				// invalid input, reset text to the slider value
				ctlWindowWidth.Text = ctlWindowSizeSlider.Value.ToString();
			}
		}

		private bool m_heightBeingEdited = false;
		private void ctlWindowHeight_TextChanged( object sender, EventArgs e )
		{
			m_heightBeingEdited = true;
			// Update the width textbox
			int newHeight;
			if ( int.TryParse( ctlWindowHeight.Text, out newHeight ) )
			{
				//GameWindowStartingWidth = GetWidthFromHeight( newHeight );
				if ( !m_widthBeingEdited )
				{
					int width;
					bool widthSuccess = int.TryParse( ctlWindowWidth.Text, out width );
					if ( !widthSuccess || DfoLauncher.GetHeightFromWidth( width ) != newHeight )
					{
						ctlWindowWidth.Text = DfoLauncher.GetWidthFromHeight( newHeight ).ToString();

						// Update the slider
						int newWidth;
						if ( int.TryParse( ctlWindowWidth.Text, out newWidth ) )
						{
							newWidth = newWidth.Clamp( ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );
							ctlWindowSizeSlider.Value = newWidth;
						}
					}
				}
			}
			m_heightBeingEdited = false;
		}

		private void ctlWindowHeight_Leave( object sender, EventArgs e )
		{
			// Update the slider (and indirectly, the width textbox)
			int newHeight;
			if ( int.TryParse( ctlWindowHeight.Text, out newHeight ) )
			{
				int width;
				bool widthSuccess = int.TryParse( ctlWindowWidth.Text, out width );

				int newWidth = 0;
				if ( widthSuccess && DfoLauncher.GetHeightFromWidth( width ) == newHeight )
				{
					newWidth = width;
				}
				else
				{
					newWidth = DfoLauncher.GetWidthFromHeight( newHeight );
				}

				newWidth = newWidth.Clamp( ctlWindowSizeSlider.Minimum, ctlWindowSizeSlider.Maximum );

				ctlWindowSizeSlider.Value = newWidth;

				// Set the text to a canonical text for the width
				ctlWindowHeight.Text = DfoLauncher.GetHeightFromWidth( ctlWindowSizeSlider.Value ).ToString();
			}
			else
			{
				// invalid input, reset text to the slider value
				ctlWindowHeight.Text = DfoLauncher.GetHeightFromWidth( ctlWindowSizeSlider.Value ).ToString();
			}
		}

		private void ctlResizeButton_Click( object sender, EventArgs e )
		{
			Logging.Log.DebugFormat( "Resizing game window while running to {0}x{1}",
				GameWindowWidth, GameWindowHeight );

			try
			{
				if ( GameWindowWidth.HasValue && GameWindowHeight.HasValue )
				{
					m_launcher.ResizeDfoWindow( GameWindowWidth.Value, GameWindowHeight.Value );
					Logging.Log.DebugFormat( "Resized." );
				}
				else
				{
					Logging.Log.WarnFormat( "Width or height doesn't have a value. O_o" );
				}
			}
			catch ( Exception ex )
			{
				if ( ex is InvalidOperationException || ex is Win32Exception )
				{
					Logging.Log.WarnFormat( "Could not resize the game window: {0}", ex.Message );
					Logging.Log.Debug( "Exception details:", ex );
				}
				else
				{
					throw;
				}
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
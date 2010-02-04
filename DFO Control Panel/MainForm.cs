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

namespace Dfo.ControlPanel
{
	public partial class ctlMainForm : Form
	{
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
		private bool SwitchSoundpacks
		{
			get { return ctlSwitchSoundpacks.Checked; }
			set
			{
				if ( ctlSwitchSoundpacks.Enabled )
				{
					ctlSwitchSoundpacks.Checked = value;
				}
			}
		}
		private string Username { get { return ctlUsername.Text; } set { ctlUsername.Text = value; } }
		private string Password { get { return ctlPassword.Text; } set { ctlPassword.Text = value; } }

		private string m_dfoDir;
		private string DfoDir
		{
			get { return m_dfoDir; }
			set
			{
				ValidatePath( value, "DfoDir" );
				m_dfoDir = value;
			}
		}

		private string m_customSoundpackDir;
		private string CustomSoundpackDir
		{
			get { return m_customSoundpackDir; }
			set
			{
				ValidatePath( value, "CustomSoundpackDir" );
				m_customSoundpackDir = value;
			}
		}

		private string m_tempSoundpackDir;
		private string TempSoundpackDir
		{
			get { return m_tempSoundpackDir; }
			set
			{
				ValidatePath( value, "TempSoundpackDir" );
				m_tempSoundpackDir = value;
			}
		}

		private static void ValidatePath( string path, string propertyName )
		{
			if ( path == null )
			{
				throw new ArgumentNullException( propertyName );
			}
			char[] invalidChars = Path.GetInvalidPathChars();
			if ( path.IndexOfAny( invalidChars ) != -1 )
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
			m_launcher.Params.CustomSoundpackDir = CustomSoundpackDir;
			m_launcher.Params.DfoDir = DfoDir;
			m_launcher.Params.LaunchInWindowed = LaunchWindowed;
			m_launcher.Params.Password = Password;
			m_launcher.Params.SwitchSoundpacks = SwitchSoundpacks;
			m_launcher.Params.TempSoundpackDir = TempSoundpackDir;
			m_launcher.Params.Username = Username;
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

			exportToolStripMenuItem.ToolTipText = s_exportDisabledTooltip;

			m_parsedArgs = parsedArgs;

			m_launcher.WindowModeFailed += WindowModeFailedHandler;
			m_launcher.LaunchStateChanged += StateChangedHandler;
			m_launcher.SoundpackSwitchFailed += SoundpackFailedHandler;
			m_launcher.PopupKillFailed += PopupKillFailedHandler;

			ctlStatusLabel.Text = m_stateNoneText;

			Logging.Log.Debug( "Main window created." );
		}

		private void ctlMainForm_Load( object sender, EventArgs e )
		{
			Logging.Log.Debug( "Loading main window." );

			try
			{
				m_launcher.Params.AutoDetectDfoDir();
				DfoDir = m_launcher.Params.DfoDir;
				CustomSoundpackDir = m_launcher.Params.CustomSoundpackDir;
				TempSoundpackDir = m_launcher.Params.TempSoundpackDir;
			}
			catch ( IOException ex )
			{
				Logging.Log.ErrorFormat( "Could not autodetect the DFO directory. {0}", ex.Message );
			}

			m_savedSettings = SettingsLoader.Load();
			ApplySettingsAndArguments();

			FixSoundpacksIfNeeded();

			bool mainSoundpackDirExists = Directory.Exists( m_launcher.Params.SoundpackDir );
			bool customSoundpackDirExists = Directory.Exists( CustomSoundpackDir );
			bool tempSoundpackDirFree = !Directory.Exists( TempSoundpackDir );

			if ( !mainSoundpackDirExists || !customSoundpackDirExists || !tempSoundpackDirFree )
			{
				Logging.Log.InfoFormat(
					"Directories are not set up for soundpack switching, greying out that option. Main soundpack dir exists = {0}, custom soundpack dir exists = {1}, temp soundpack dir available = {2}.",
					mainSoundpackDirExists, customSoundpackDirExists, tempSoundpackDirFree );

				ctlSwitchSoundpacks.Checked = false;
				ctlSwitchSoundpacks.Enabled = false;
			}

			ctlUsername.Select();
			Logging.Log.Debug( "Main window loaded." );
		}

		/// <summary>
		/// Attempts to fix mixed-up soundpacks usually caused by a system crash while the game is running.
		/// This function uses m_launcher's DfoDir, CustomSoundpackDir, and TempSoundpackDir properties.
		/// </summary>
		private void FixSoundpacksIfNeeded()
		{
			Logging.Log.Info( "Checking for broken soundpacks..." );
			if ( m_launcher.SoundpacksBroken() )
			{
				Logging.Log.Info( "Broken soundpack directories detected, attempting to fix them..." );
				bool fixWorked = false;
				try
				{
					m_launcher.FixBrokenSoundpacks();
					fixWorked = true;
				}
				catch ( IOException ex )
				{
					// XXX: Should the program exit?
					DisplayError( string.Format(
						"Error while trying to fix broken soundpack directories. {0} I guess you'll have to fix them yourself.",
						ex.Message ),
						"Couldn't fix the soundpacks" );
				}

				if ( fixWorked )
				{
					DisplayInfo( "Your soundpack directories were detected to be mixed up (this is usually caused by a system crash). They have been fixed.",
						"Soundpacks fixed" );
				}
			}
			else
			{
				Logging.Log.Info( "Soundpacks are OK." );
			}
		}

		private void ApplySettingsAndArguments()
		{
			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.ClosePopup, m_savedSettings.ClosePopup, null,
				"Close popup", ( bool closePopup ) => ClosePopup = closePopup, m_launcher.Params.ClosePopup, false );

			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.LaunchWindowed, m_savedSettings.LaunchWindowed, null,
				"Launch windowed", ( bool windowed ) => LaunchWindowed = windowed, false, false );

			SettingsLoader.ApplySettingStruct( m_parsedArgs.Settings.SwitchSoundpacks, m_savedSettings.SwitchSoundpacks,
				null, "Switch soundpacks", ( bool switchSoundpacks ) => SwitchSoundpacks = switchSoundpacks,
				m_launcher.Params.SwitchSoundpacks, false );

			SettingsLoader.ApplySettingStruct( null, m_savedSettings.RememberUsername, null, "Remember username",
				( bool remember ) => RememberMe = remember, false, false );

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.Username, m_savedSettings.Username, null,
				"Username", ( string user ) => Username = user, "", true );

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.Password, m_savedSettings.Password, null,
				"Password", ( string pass ) => Password = pass, "", true );

			Func<string, string> validatePath = ( string dir ) =>
			{
				if ( LaunchParams.PathIsValid( dir ) )
				{
					return null;
				}
				else
				{
					return string.Format( "{0} is not a valid path.", dir );
				}
			};

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.DfoDir, m_savedSettings.DfoDir, validatePath,
				"DFO directory", ( string dfodir ) => DfoDir = dfodir, m_launcher.Params.DfoDir, false );

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.CustomSoundpackDir,
				m_savedSettings.CustomSoundpackDir, validatePath, "Custom soundpack directory",
				( string customSoundDir ) => CustomSoundpackDir = customSoundDir, m_launcher.Params.CustomSoundpackDir,
				false );

			SettingsLoader.ApplySettingClass( m_parsedArgs.Settings.TempSoundpackDir, m_savedSettings.TempSoundpackDir,
				validatePath, "Temp soundpack directory", ( string tempSoundDir ) => TempSoundpackDir = tempSoundDir,
				m_launcher.Params.TempSoundpackDir, false );
		}

		private void WindowModeFailedHandler( object sender, CancelErrorEventArgs e )
		{
			DisplayError( string.Format( "The window mode setting could not be applied. {0}", e.Error.Message ),
				"Windowed Setting Error" );
			e.Cancel = true;
		}

		private void SoundpackFailedHandler( object sender, Dfo.Controlling.ErrorEventArgs e )
		{
			DisplayError( string.Format( "Switching soundpacks failed. {0}", e.Error.Message ),
				"Soundpack Switch Error" );
		}

		private void PopupKillFailedHandler( object sender, Dfo.Controlling.ErrorEventArgs e )
		{
			Logging.Log.Warn( string.Format( "Could not kill the popup. {0}", e.Error.Message ) );
		}

		private void StateChangedHandler( object sender, EventArgs e )
		{
			LaunchState currentState = ( (DfoLauncher)sender ).State;
			Logging.Log.DebugFormat( "State change to {0}.", currentState );

			switch ( currentState )
			{
				case LaunchState.None:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = m_stateNoneText );
					m_stateBecameNoneEvent.Set();
					break;
				case LaunchState.Login:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Logging in..." );
					break;
				case LaunchState.Launching:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Launching..." );
					break;
				case LaunchState.GameInProgress:
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Game in progress" );
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

		private void ShowProgressBar()
		{
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Visible = true );
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Style = ProgressBarStyle.Marquee );
		}

		private void HideProgressBar()
		{
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Visible = false );
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Style = ProgressBarStyle.Blocks ); // Don't animate when it's not visible - dunno if this actually saves any cpu or not
		}

		private void ctlLaunch_Click( object sender, EventArgs e )
		{
			ctlLaunch.Enabled = false;

			SetLauncherParams();

			Logging.Log.DebugFormat( "Launching. Launch parameters:{0}{1}", Environment.NewLine, m_launcher.Params );

			m_launcherThreadCanceledEvent.Reset();

			m_launcherThread = new Thread( LaunchThreadStart );
			m_launcherThread.IsBackground = true;
			m_launcherThread.Name = "Launcher";
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
					Logging.Log.Debug( "State became None." );
				}
				else
				{
					Logging.Log.Debug( "Canceled." );
				}

				m_launcher.Reset();
				Logging.Log.Debug( "Launcher object reset." );
			}

			ctlLaunch.BeginInvoke( () => ctlLaunch.Enabled = true );

			lock ( m_syncHandle )
			{
				if ( m_closeWhenDone )
				{
					// This has to be an async invoke because the form closing handler waits for this thread to finish!
					this.BeginInvoke( () => this.Close() );
				}
			}

			Logging.Log.Debug( "End of thread." );
		}

		private void DisplayError( string errorMessage, string secondaryText )
		{
			Logging.Log.Error( errorMessage );
			MessageBox.Show( errorMessage, secondaryText, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}

		private void DisplayInfo( string message, string secondaryText )
		{
			Logging.Log.Info( message );
			MessageBox.Show( message, secondaryText, MessageBoxButtons.OK, MessageBoxIcon.Information );
		}

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
			if ( m_launcher.State == LaunchState.GameInProgress && m_launcher.Params.SwitchSoundpacks )
			{
				DisplayError( "You cannot close this program while the game is in progress if you chose to switch soundpacks. You must wait until the game finishes so the soundpacks can be switched back.",
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

			StartupSettings settingsToSave = new StartupSettings();
			settingsToSave.ClosePopup = ClosePopup;
			settingsToSave.LaunchWindowed = LaunchWindowed;
			settingsToSave.SwitchSoundpacks = SwitchSoundpacks;
			if ( RememberMe )
			{
				settingsToSave.RememberUsername = true;
				settingsToSave.Username = Username;
			}
			else
			{
				settingsToSave.RememberUsername = false;
			}

			SettingsLoader.Save( settingsToSave );
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
		}

		private void ctlPassword_TextChanged( object sender, EventArgs e )
		{
			UpdateSaveAsVisibility();
		}

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
						Logging.Log.InfoFormat("Batch script saved to {0}.", fileDialog.FileName);
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
			if ( m_parsedArgs.Settings.CustomSoundpackDir != null )
			{
				settings.CustomSoundpackDir = CustomSoundpackDir;
			}
			if ( m_parsedArgs.Settings.TempSoundpackDir != null )
			{
				settings.CustomSoundpackDir = TempSoundpackDir;
			}

			settings.ClosePopup = ClosePopup;
			settings.LaunchWindowed = LaunchWindowed;
			settings.Password = Password;
			settings.RememberUsername = RememberMe;
			settings.SwitchSoundpacks = SwitchSoundpacks;
			settings.Username = Username;

			return settings;
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
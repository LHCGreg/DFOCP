using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Dfo.Login;
using System.IO;

namespace Dfo.BrowserlessDfoGui
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

		// Prefer Control.BeginInvoke to Control.Invoke. Using Control.Invoke when the UI thread is waiting for the
		// current thread to finish results in a deadlock.

		public ctlMainForm()
		{
			Logging.Log.Debug( "Creating main window." );
			InitializeComponent();
			m_launcher.WindowModeFailed += WindowModeFailedHandler;
			m_launcher.LaunchStateChanged += StateChangedHandler;
			m_launcher.SoundpackSwitchFailed += SoundpackFailedHandler;

			ctlStatusLabel.Text = m_stateNoneText;

			bool mainSoundpackDirExists = Directory.Exists( m_launcher.Params.SoundpackDir );
			bool customSoundpackDirExists = Directory.Exists( m_launcher.Params.CustomSoundpackDir );
			bool tempSoundpackDirFree = !Directory.Exists( m_launcher.Params.TempSoundpackDir );

			if ( !mainSoundpackDirExists || !customSoundpackDirExists || !tempSoundpackDirFree )
			{
				Logging.Log.InfoFormat(
					"Directories are not set up for soundpack switching, greying out that option. Main soundpack dir exists = {0}, custom soundpack dir exists = {1}, temp soundpack dir available = {2}.",
					mainSoundpackDirExists, customSoundpackDirExists, tempSoundpackDirFree );

				ctlSwitchSoundpacks.Checked = false;
				ctlSwitchSoundpacks.Enabled = false;
			}
		}

		private void WindowModeFailedHandler( object sender, CancelErrorEventArgs e )
		{
			DisplayError( string.Format( "The window mode setting could not be applied. {0}", e.Error.Message ),
				"Windowed Setting Error" );
		}

		private void SoundpackFailedHandler( object sender, Dfo.Login.ErrorEventArgs e )
		{
			DisplayError( string.Format( "Switching soundpacks failed. {0}", e.Error.Message ),
				"Soundpack Switch Error" );
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
					ctlStatusStrip.BeginInvoke( () => ctlStatusLabel.Text = "Game in progress. Do not exit if you are switching soundpacks." );
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
		}

		private void HideProgressBar()
		{
			ctlStatusStrip.BeginInvoke( () => ctlProgressBar.Visible = false );
		}

		private void ctlLaunch_Click( object sender, EventArgs e )
		{
			ctlLaunch.Enabled = false;

			m_launcher.Params.ClosePopup = ctlClosePopup.Checked;
			m_launcher.Params.LaunchInWindowed = ctlLaunchWindowed.Checked;
			m_launcher.Params.Password = ctlPassword.Text;
			m_launcher.Params.SwitchSoundpacks = ctlSwitchSoundpacks.Checked;
			m_launcher.Params.Username = ctlUsername.Text;

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
					DisplayError( string.Format( "The game could not be started. {0}", ex.Message ), "Launch Error" );
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

		private void ctlMainForm_FormClosing( object sender, FormClosingEventArgs e )
		{
			Logging.Log.Debug( "Main window closing." );

			m_launcherThreadCanceledEvent.Set();
			if ( m_launcherThread != null )
			{
				Logging.Log.Debug( "Waiting for launcher thread to end." );
				m_launcherThread.Join();
				Logging.Log.Debug( "Launcher thread joined." );
			}
		}

		private void exitToolStripMenuItem_Click( object sender, EventArgs e )
		{
			this.Close();
		}

		private void aboutToolStripMenuItem_Click( object sender, EventArgs e )
		{
			using ( AboutBrowserlessDfo aboutBox = new AboutBrowserlessDfo() )
			{
				aboutBox.ShowDialog();
			}
		}
	}
}

/*
 Copyright 2009 Greg Najda

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
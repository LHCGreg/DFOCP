using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Dfo.Login;

namespace Dfo.BrowserlessDfoGui
{
	public partial class ctlMainForm : Form
	{
		private List<Control> controlsToEnableDisable;
		private DfoLauncher m_launcher = new DfoLauncher();

		public ctlMainForm()
		{
			InitializeComponent();
			controlsToEnableDisable = new List<Control> { ctlUsername, ctlPassword, ctlCloseOnSuccess ,ctlLaunch };
		}

		private void ctlLaunch_Click( object sender, EventArgs e )
		{
			if ( ctlLaunch.Enabled ) // Not sure if disabling automatically stops already pending click events
			{
				DisableControls(); // Prevent the user from spam-clicking the button or changing stuff while trying to launch
				Thread launchThread = new Thread( launchThreadStart ); // Use a separate thread so the UI rem
				UsernamePasswordPair userAndPass = new UsernamePasswordPair( ctlUsername.Text, ctlPassword.Text );
				launchThread.Name = "Launch thread";
				launchThread.IsBackground = true;
				launchThread.Start( userAndPass );
			}
		}

		private void DisableControls()
		{
			controlsToEnableDisable.ForEach( control => control.Enabled = false );
		}

		private void EnableControls()
		{
			controlsToEnableDisable.ForEach( control => control.Enabled = true );
		}

		private void launchSuccessful()
		{
			if ( ctlCloseOnSuccess.Checked )
			{
				Application.Exit();
			}
			else
			{
				EnableControls();
			}
		}

		private void launchFailed( DfoLaunchException ex )
		{
			MessageBox.Show( ex.Message, "Could not start DFO", MessageBoxButtons.OK, MessageBoxIcon.Error );
			EnableControls();
		}

		private void launchThreadStart( object param )
		{
			UsernamePasswordPair userAndPass = (UsernamePasswordPair)param;
			try
			{
				//DfoLogin.StartDfo( userAndPass.Username, userAndPass.Password );
				m_launcher.Params.Username = userAndPass.Username;
				m_launcher.Params.Password = userAndPass.Password;
				m_launcher.Launch();
				this.Invoke( (ThreadStart)( () => launchSuccessful() ) );
			}
			catch ( DfoLaunchException ex )
			{
				this.Invoke( (ThreadStart)( () => launchFailed( ex ) ) );
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
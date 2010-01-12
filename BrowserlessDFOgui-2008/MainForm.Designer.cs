namespace Dfo.BrowserlessDfoGui
{
	partial class ctlMainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				m_launcher.Dispose();
				m_launcherThreadCanceledEvent.Close();
				m_stateBecameNoneEvent.Close();

				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ctlUsername = new System.Windows.Forms.TextBox();
			this.ctlPassword = new System.Windows.Forms.TextBox();
			this.lblUsername = new System.Windows.Forms.Label();
			this.lblPassword = new System.Windows.Forms.Label();
			this.ctlLaunch = new System.Windows.Forms.Button();
			this.ctlClosePopup = new System.Windows.Forms.CheckBox();
			this.ctlLaunchWindowed = new System.Windows.Forms.CheckBox();
			this.ctlSwitchSoundpacks = new System.Windows.Forms.CheckBox();
			this.ctlRememberMe = new System.Windows.Forms.CheckBox();
			this.ctlLoginInfoBox = new System.Windows.Forms.GroupBox();
			this.ctlOptionsBox = new System.Windows.Forms.GroupBox();
			this.ctlStatusStrip = new System.Windows.Forms.StatusStrip();
			this.ctlStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.ctlProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.ctlMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ctlLoginInfoBox.SuspendLayout();
			this.ctlOptionsBox.SuspendLayout();
			this.ctlStatusStrip.SuspendLayout();
			this.ctlMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// ctlUsername
			// 
			this.ctlUsername.Location = new System.Drawing.Point( 94, 19 );
			this.ctlUsername.Name = "ctlUsername";
			this.ctlUsername.Size = new System.Drawing.Size( 198, 20 );
			this.ctlUsername.TabIndex = 0;
			// 
			// ctlPassword
			// 
			this.ctlPassword.Location = new System.Drawing.Point( 94, 45 );
			this.ctlPassword.Name = "ctlPassword";
			this.ctlPassword.Size = new System.Drawing.Size( 198, 20 );
			this.ctlPassword.TabIndex = 1;
			this.ctlPassword.UseSystemPasswordChar = true;
			// 
			// lblUsername
			// 
			this.lblUsername.AutoSize = true;
			this.lblUsername.Location = new System.Drawing.Point( 11, 22 );
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size( 55, 13 );
			this.lblUsername.TabIndex = 2;
			this.lblUsername.Text = "Username";
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point( 11, 48 );
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size( 53, 13 );
			this.lblPassword.TabIndex = 3;
			this.lblPassword.Text = "Password";
			// 
			// ctlLaunch
			// 
			this.ctlLaunch.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.ctlLaunch.Location = new System.Drawing.Point( 109, 188 );
			this.ctlLaunch.Name = "ctlLaunch";
			this.ctlLaunch.Size = new System.Drawing.Size( 240, 55 );
			this.ctlLaunch.TabIndex = 4;
			this.ctlLaunch.Text = "Start DFO";
			this.ctlLaunch.UseVisualStyleBackColor = true;
			this.ctlLaunch.Click += new System.EventHandler( this.ctlLaunch_Click );
			// 
			// ctlClosePopup
			// 
			this.ctlClosePopup.AutoSize = true;
			this.ctlClosePopup.Checked = true;
			this.ctlClosePopup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ctlClosePopup.Location = new System.Drawing.Point( 6, 19 );
			this.ctlClosePopup.Name = "ctlClosePopup";
			this.ctlClosePopup.Size = new System.Drawing.Size( 91, 17 );
			this.ctlClosePopup.TabIndex = 5;
			this.ctlClosePopup.Text = "Close popup?";
			this.ctlClosePopup.UseVisualStyleBackColor = true;
			// 
			// ctlLaunchWindowed
			// 
			this.ctlLaunchWindowed.AutoSize = true;
			this.ctlLaunchWindowed.Location = new System.Drawing.Point( 6, 42 );
			this.ctlLaunchWindowed.Name = "ctlLaunchWindowed";
			this.ctlLaunchWindowed.Size = new System.Drawing.Size( 105, 17 );
			this.ctlLaunchWindowed.TabIndex = 6;
			this.ctlLaunchWindowed.Text = "Start windowed?";
			this.ctlLaunchWindowed.UseVisualStyleBackColor = true;
			// 
			// ctlSwitchSoundpacks
			// 
			this.ctlSwitchSoundpacks.AutoSize = true;
			this.ctlSwitchSoundpacks.Location = new System.Drawing.Point( 6, 65 );
			this.ctlSwitchSoundpacks.Name = "ctlSwitchSoundpacks";
			this.ctlSwitchSoundpacks.Size = new System.Drawing.Size( 125, 17 );
			this.ctlSwitchSoundpacks.TabIndex = 7;
			this.ctlSwitchSoundpacks.Text = "Switch soundpacks?";
			this.ctlSwitchSoundpacks.UseVisualStyleBackColor = true;
			// 
			// ctlRememberMe
			// 
			this.ctlRememberMe.AutoSize = true;
			this.ctlRememberMe.Location = new System.Drawing.Point( 14, 71 );
			this.ctlRememberMe.Name = "ctlRememberMe";
			this.ctlRememberMe.Size = new System.Drawing.Size( 142, 17 );
			this.ctlRememberMe.TabIndex = 9;
			this.ctlRememberMe.Text = "Remember my username";
			this.ctlRememberMe.UseVisualStyleBackColor = true;
			// 
			// ctlLoginInfoBox
			// 
			this.ctlLoginInfoBox.Controls.Add( this.ctlUsername );
			this.ctlLoginInfoBox.Controls.Add( this.ctlRememberMe );
			this.ctlLoginInfoBox.Controls.Add( this.lblUsername );
			this.ctlLoginInfoBox.Controls.Add( this.ctlPassword );
			this.ctlLoginInfoBox.Controls.Add( this.lblPassword );
			this.ctlLoginInfoBox.Location = new System.Drawing.Point( 15, 45 );
			this.ctlLoginInfoBox.Name = "ctlLoginInfoBox";
			this.ctlLoginInfoBox.Size = new System.Drawing.Size( 320, 107 );
			this.ctlLoginInfoBox.TabIndex = 10;
			this.ctlLoginInfoBox.TabStop = false;
			this.ctlLoginInfoBox.Text = "Login Information";
			// 
			// ctlOptionsBox
			// 
			this.ctlOptionsBox.Controls.Add( this.ctlClosePopup );
			this.ctlOptionsBox.Controls.Add( this.ctlSwitchSoundpacks );
			this.ctlOptionsBox.Controls.Add( this.ctlLaunchWindowed );
			this.ctlOptionsBox.Location = new System.Drawing.Point( 341, 45 );
			this.ctlOptionsBox.Name = "ctlOptionsBox";
			this.ctlOptionsBox.Size = new System.Drawing.Size( 139, 107 );
			this.ctlOptionsBox.TabIndex = 11;
			this.ctlOptionsBox.TabStop = false;
			this.ctlOptionsBox.Text = "Options";
			// 
			// ctlStatusStrip
			// 
			this.ctlStatusStrip.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.ctlStatusLabel,
            this.ctlProgressBar} );
			this.ctlStatusStrip.Location = new System.Drawing.Point( 0, 287 );
			this.ctlStatusStrip.Name = "ctlStatusStrip";
			this.ctlStatusStrip.Size = new System.Drawing.Size( 497, 22 );
			this.ctlStatusStrip.SizingGrip = false;
			this.ctlStatusStrip.TabIndex = 12;
			// 
			// ctlStatusLabel
			// 
			this.ctlStatusLabel.Name = "ctlStatusLabel";
			this.ctlStatusLabel.Size = new System.Drawing.Size( 482, 17 );
			this.ctlStatusLabel.Spring = true;
			this.ctlStatusLabel.Text = "Ready";
			this.ctlStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ctlProgressBar
			// 
			this.ctlProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.ctlProgressBar.Name = "ctlProgressBar";
			this.ctlProgressBar.Size = new System.Drawing.Size( 100, 16 );
			this.ctlProgressBar.Visible = false;
			// 
			// ctlMenuStrip
			// 
			this.ctlMenuStrip.BackColor = System.Drawing.SystemColors.MenuBar;
			this.ctlMenuStrip.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem} );
			this.ctlMenuStrip.Location = new System.Drawing.Point( 0, 0 );
			this.ctlMenuStrip.Name = "ctlMenuStrip";
			this.ctlMenuStrip.Size = new System.Drawing.Size( 497, 24 );
			this.ctlMenuStrip.TabIndex = 13;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem} );
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size( 35, 20 );
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size( 152, 22 );
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler( this.exitToolStripMenuItem_Click );
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem} );
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size( 40, 20 );
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size( 152, 22 );
			this.aboutToolStripMenuItem.Text = "&About...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler( this.aboutToolStripMenuItem_Click );
			// 
			// ctlMainForm
			// 
			this.AcceptButton = this.ctlLaunch;
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 497, 309 );
			this.Controls.Add( this.ctlStatusStrip );
			this.Controls.Add( this.ctlMenuStrip );
			this.Controls.Add( this.ctlOptionsBox );
			this.Controls.Add( this.ctlLoginInfoBox );
			this.Controls.Add( this.ctlLaunch );
			this.MainMenuStrip = this.ctlMenuStrip;
			this.Name = "ctlMainForm";
			this.Text = "Browserless DFO Launcher";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.ctlMainForm_FormClosing );
			this.ctlLoginInfoBox.ResumeLayout( false );
			this.ctlLoginInfoBox.PerformLayout();
			this.ctlOptionsBox.ResumeLayout( false );
			this.ctlOptionsBox.PerformLayout();
			this.ctlStatusStrip.ResumeLayout( false );
			this.ctlStatusStrip.PerformLayout();
			this.ctlMenuStrip.ResumeLayout( false );
			this.ctlMenuStrip.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ctlUsername;
		private System.Windows.Forms.TextBox ctlPassword;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.Button ctlLaunch;
		private System.Windows.Forms.CheckBox ctlClosePopup;
		private System.Windows.Forms.CheckBox ctlLaunchWindowed;
		private System.Windows.Forms.CheckBox ctlSwitchSoundpacks;
		private System.Windows.Forms.CheckBox ctlRememberMe;
		private System.Windows.Forms.GroupBox ctlLoginInfoBox;
		private System.Windows.Forms.GroupBox ctlOptionsBox;
		private System.Windows.Forms.StatusStrip ctlStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel ctlStatusLabel;
		private System.Windows.Forms.ToolStripProgressBar ctlProgressBar;
		private System.Windows.Forms.MenuStrip ctlMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
	}
}


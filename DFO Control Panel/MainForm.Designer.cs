namespace Dfo.ControlPanel
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
			if ( disposing )
			{
				m_launcher.Dispose();
				m_launcherThreadCanceledEvent.Close();
				m_stateBecameNoneEvent.Close();

				if ( m_notifyIcon != null )
				{
					m_notifyIcon.Visible = false;
					m_notifyIcon.Dispose();
					m_notifyIcon = null;
				}

				if ( components != null )
				{
					components.Dispose();
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( ctlMainForm ) );
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
			this.ctlResizeButton = new System.Windows.Forms.Button();
			this.lblWindowSizeX = new System.Windows.Forms.Label();
			this.ctlSwitchAudioXml = new System.Windows.Forms.CheckBox();
			this.ctlWindowHeight = new System.Windows.Forms.TextBox();
			this.ctlWindowWidth = new System.Windows.Forms.TextBox();
			this.ctlWindowSizeSlider = new System.Windows.Forms.TrackBar();
			this.ctlStatusStrip = new System.Windows.Forms.StatusStrip();
			this.ctlStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.ctlProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.ctlMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ctlLoginInfoBox.SuspendLayout();
			this.ctlOptionsBox.SuspendLayout();
			( (System.ComponentModel.ISupportInitialize)( this.ctlWindowSizeSlider ) ).BeginInit();
			this.ctlStatusStrip.SuspendLayout();
			this.ctlMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// ctlUsername
			// 
			this.ctlUsername.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlUsername.Location = new System.Drawing.Point( 94, 30 );
			this.ctlUsername.Name = "ctlUsername";
			this.ctlUsername.Size = new System.Drawing.Size( 167, 20 );
			this.ctlUsername.TabIndex = 0;
			this.ctlUsername.TextChanged += new System.EventHandler( this.ctlUsername_TextChanged );
			// 
			// ctlPassword
			// 
			this.ctlPassword.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlPassword.Location = new System.Drawing.Point( 94, 56 );
			this.ctlPassword.Name = "ctlPassword";
			this.ctlPassword.Size = new System.Drawing.Size( 167, 20 );
			this.ctlPassword.TabIndex = 1;
			this.ctlPassword.UseSystemPasswordChar = true;
			this.ctlPassword.TextChanged += new System.EventHandler( this.ctlPassword_TextChanged );
			// 
			// lblUsername
			// 
			this.lblUsername.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.lblUsername.AutoSize = true;
			this.lblUsername.Location = new System.Drawing.Point( 11, 33 );
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size( 55, 13 );
			this.lblUsername.TabIndex = 2;
			this.lblUsername.Text = "Username";
			// 
			// lblPassword
			// 
			this.lblPassword.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point( 11, 59 );
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size( 53, 13 );
			this.lblPassword.TabIndex = 3;
			this.lblPassword.Text = "Password";
			// 
			// ctlLaunch
			// 
			this.ctlLaunch.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.ctlLaunch.Location = new System.Drawing.Point( 167, 216 );
			this.ctlLaunch.Name = "ctlLaunch";
			this.ctlLaunch.Size = new System.Drawing.Size( 240, 55 );
			this.ctlLaunch.TabIndex = 0;
			this.ctlLaunch.Text = "Start DFO";
			this.ctlLaunch.UseVisualStyleBackColor = true;
			this.ctlLaunch.Click += new System.EventHandler( this.ctlLaunch_Click );
			// 
			// ctlClosePopup
			// 
			this.ctlClosePopup.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlClosePopup.AutoSize = true;
			this.ctlClosePopup.Checked = true;
			this.ctlClosePopup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ctlClosePopup.Location = new System.Drawing.Point( 6, 19 );
			this.ctlClosePopup.Name = "ctlClosePopup";
			this.ctlClosePopup.Size = new System.Drawing.Size( 91, 17 );
			this.ctlClosePopup.TabIndex = 0;
			this.ctlClosePopup.Text = "Close popup?";
			this.ctlClosePopup.UseVisualStyleBackColor = true;
			// 
			// ctlLaunchWindowed
			// 
			this.ctlLaunchWindowed.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlLaunchWindowed.AutoSize = true;
			this.ctlLaunchWindowed.Location = new System.Drawing.Point( 6, 42 );
			this.ctlLaunchWindowed.Name = "ctlLaunchWindowed";
			this.ctlLaunchWindowed.Size = new System.Drawing.Size( 105, 17 );
			this.ctlLaunchWindowed.TabIndex = 1;
			this.ctlLaunchWindowed.Text = "Start windowed?";
			this.ctlLaunchWindowed.UseVisualStyleBackColor = true;
			// 
			// ctlSwitchSoundpacks
			// 
			this.ctlSwitchSoundpacks.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlSwitchSoundpacks.AutoSize = true;
			this.ctlSwitchSoundpacks.Location = new System.Drawing.Point( 6, 101 );
			this.ctlSwitchSoundpacks.Name = "ctlSwitchSoundpacks";
			this.ctlSwitchSoundpacks.Size = new System.Drawing.Size( 125, 17 );
			this.ctlSwitchSoundpacks.TabIndex = 6;
			this.ctlSwitchSoundpacks.Text = "Switch soundpacks?";
			this.ctlSwitchSoundpacks.UseVisualStyleBackColor = true;
			// 
			// ctlRememberMe
			// 
			this.ctlRememberMe.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlRememberMe.AutoSize = true;
			this.ctlRememberMe.Location = new System.Drawing.Point( 14, 82 );
			this.ctlRememberMe.Name = "ctlRememberMe";
			this.ctlRememberMe.Size = new System.Drawing.Size( 142, 17 );
			this.ctlRememberMe.TabIndex = 2;
			this.ctlRememberMe.Text = "Remember my username";
			this.ctlRememberMe.UseVisualStyleBackColor = true;
			// 
			// ctlLoginInfoBox
			// 
			this.ctlLoginInfoBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlLoginInfoBox.Controls.Add( this.ctlUsername );
			this.ctlLoginInfoBox.Controls.Add( this.ctlRememberMe );
			this.ctlLoginInfoBox.Controls.Add( this.lblUsername );
			this.ctlLoginInfoBox.Controls.Add( this.ctlPassword );
			this.ctlLoginInfoBox.Controls.Add( this.lblPassword );
			this.ctlLoginInfoBox.Location = new System.Drawing.Point( 15, 45 );
			this.ctlLoginInfoBox.Name = "ctlLoginInfoBox";
			this.ctlLoginInfoBox.Size = new System.Drawing.Size( 289, 118 );
			this.ctlLoginInfoBox.TabIndex = 10;
			this.ctlLoginInfoBox.TabStop = false;
			this.ctlLoginInfoBox.Text = "Login Information";
			// 
			// ctlOptionsBox
			// 
			this.ctlOptionsBox.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlOptionsBox.Controls.Add( this.ctlResizeButton );
			this.ctlOptionsBox.Controls.Add( this.lblWindowSizeX );
			this.ctlOptionsBox.Controls.Add( this.ctlSwitchAudioXml );
			this.ctlOptionsBox.Controls.Add( this.ctlClosePopup );
			this.ctlOptionsBox.Controls.Add( this.ctlWindowHeight );
			this.ctlOptionsBox.Controls.Add( this.ctlSwitchSoundpacks );
			this.ctlOptionsBox.Controls.Add( this.ctlLaunchWindowed );
			this.ctlOptionsBox.Controls.Add( this.ctlWindowWidth );
			this.ctlOptionsBox.Controls.Add( this.ctlWindowSizeSlider );
			this.ctlOptionsBox.Location = new System.Drawing.Point( 310, 45 );
			this.ctlOptionsBox.Name = "ctlOptionsBox";
			this.ctlOptionsBox.Size = new System.Drawing.Size( 269, 150 );
			this.ctlOptionsBox.TabIndex = 11;
			this.ctlOptionsBox.TabStop = false;
			this.ctlOptionsBox.Text = "Options";
			// 
			// ctlResizeButton
			// 
			this.ctlResizeButton.Location = new System.Drawing.Point( 208, 54 );
			this.ctlResizeButton.Name = "ctlResizeButton";
			this.ctlResizeButton.Size = new System.Drawing.Size( 53, 28 );
			this.ctlResizeButton.TabIndex = 5;
			this.ctlResizeButton.Text = "Resize";
			this.ctlResizeButton.UseVisualStyleBackColor = true;
			this.ctlResizeButton.Visible = false;
			this.ctlResizeButton.Click += new System.EventHandler( this.ctlResizeButton_Click );
			// 
			// lblWindowSizeX
			// 
			this.lblWindowSizeX.AutoSize = true;
			this.lblWindowSizeX.Location = new System.Drawing.Point( 148, 63 );
			this.lblWindowSizeX.Name = "lblWindowSizeX";
			this.lblWindowSizeX.Size = new System.Drawing.Size( 12, 13 );
			this.lblWindowSizeX.TabIndex = 22;
			this.lblWindowSizeX.Text = "x";
			// 
			// ctlSwitchAudioXml
			// 
			this.ctlSwitchAudioXml.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlSwitchAudioXml.AutoSize = true;
			this.ctlSwitchAudioXml.Location = new System.Drawing.Point( 6, 124 );
			this.ctlSwitchAudioXml.Name = "ctlSwitchAudioXml";
			this.ctlSwitchAudioXml.Size = new System.Drawing.Size( 111, 17 );
			this.ctlSwitchAudioXml.TabIndex = 7;
			this.ctlSwitchAudioXml.Text = "Switch audio.xml?";
			this.ctlSwitchAudioXml.UseVisualStyleBackColor = true;
			// 
			// ctlWindowHeight
			// 
			this.ctlWindowHeight.Location = new System.Drawing.Point( 163, 59 );
			this.ctlWindowHeight.Name = "ctlWindowHeight";
			this.ctlWindowHeight.Size = new System.Drawing.Size( 39, 20 );
			this.ctlWindowHeight.TabIndex = 4;
			this.ctlWindowHeight.Text = "480";
			this.ctlWindowHeight.TextChanged += new System.EventHandler( this.ctlWindowHeight_TextChanged );
			this.ctlWindowHeight.Leave += new System.EventHandler( this.ctlWindowHeight_Leave );
			// 
			// ctlWindowWidth
			// 
			this.ctlWindowWidth.Location = new System.Drawing.Point( 108, 59 );
			this.ctlWindowWidth.Name = "ctlWindowWidth";
			this.ctlWindowWidth.Size = new System.Drawing.Size( 39, 20 );
			this.ctlWindowWidth.TabIndex = 3;
			this.ctlWindowWidth.Text = "640";
			this.ctlWindowWidth.TextChanged += new System.EventHandler( this.ctlWindowWidth_TextChanged );
			this.ctlWindowWidth.Leave += new System.EventHandler( this.ctlWindowWidth_Leave );
			// 
			// ctlWindowSizeSlider
			// 
			this.ctlWindowSizeSlider.LargeChange = 100;
			this.ctlWindowSizeSlider.Location = new System.Drawing.Point( 21, 59 );
			this.ctlWindowSizeSlider.Maximum = 1280;
			this.ctlWindowSizeSlider.Minimum = 640;
			this.ctlWindowSizeSlider.Name = "ctlWindowSizeSlider";
			this.ctlWindowSizeSlider.Size = new System.Drawing.Size( 90, 45 );
			this.ctlWindowSizeSlider.SmallChange = 20;
			this.ctlWindowSizeSlider.TabIndex = 2;
			this.ctlWindowSizeSlider.TickFrequency = 0;
			this.ctlWindowSizeSlider.TickStyle = System.Windows.Forms.TickStyle.None;
			this.ctlWindowSizeSlider.Value = 640;
			this.ctlWindowSizeSlider.ValueChanged += new System.EventHandler( this.ctlWindowSizeSlider_ValueChanged );
			// 
			// ctlStatusStrip
			// 
			this.ctlStatusStrip.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.ctlStatusLabel,
            this.ctlProgressBar} );
			this.ctlStatusStrip.Location = new System.Drawing.Point( 0, 346 );
			this.ctlStatusStrip.Name = "ctlStatusStrip";
			this.ctlStatusStrip.Size = new System.Drawing.Size( 591, 22 );
			this.ctlStatusStrip.SizingGrip = false;
			this.ctlStatusStrip.TabIndex = 12;
			// 
			// ctlStatusLabel
			// 
			this.ctlStatusLabel.Name = "ctlStatusLabel";
			this.ctlStatusLabel.Size = new System.Drawing.Size( 576, 17 );
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
			this.ctlMenuStrip.Size = new System.Drawing.Size( 591, 24 );
			this.ctlMenuStrip.TabIndex = 13;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem} );
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size( 35, 20 );
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.Enabled = false;
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.ShortcutKeys = ( (System.Windows.Forms.Keys)( ( System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S ) ) );
			this.exportToolStripMenuItem.Size = new System.Drawing.Size( 197, 22 );
			this.exportToolStripMenuItem.Text = "Save As .bat...";
			this.exportToolStripMenuItem.ToolTipText = "abc";
			this.exportToolStripMenuItem.EnabledChanged += new System.EventHandler( this.exportToolStripMenuItem_EnabledChanged );
			this.exportToolStripMenuItem.Click += new System.EventHandler( this.exportToolStripMenuItem_Click );
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size( 194, 6 );
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size( 197, 22 );
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
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size( 126, 22 );
			this.aboutToolStripMenuItem.Text = "&About...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler( this.aboutToolStripMenuItem_Click );
			// 
			// ctlMainForm
			// 
			this.AcceptButton = this.ctlLaunch;
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 591, 368 );
			this.Controls.Add( this.ctlStatusStrip );
			this.Controls.Add( this.ctlMenuStrip );
			this.Controls.Add( this.ctlOptionsBox );
			this.Controls.Add( this.ctlLoginInfoBox );
			this.Controls.Add( this.ctlLaunch );
			this.Icon = ( (System.Drawing.Icon)( resources.GetObject( "$this.Icon" ) ) );
			this.MainMenuStrip = this.ctlMenuStrip;
			this.Name = "ctlMainForm";
			this.Text = "DFO Control Panel";
			this.Load += new System.EventHandler( this.ctlMainForm_Load );
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.ctlMainForm_FormClosing );
			this.ctlLoginInfoBox.ResumeLayout( false );
			this.ctlLoginInfoBox.PerformLayout();
			this.ctlOptionsBox.ResumeLayout( false );
			this.ctlOptionsBox.PerformLayout();
			( (System.ComponentModel.ISupportInitialize)( this.ctlWindowSizeSlider ) ).EndInit();
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
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.CheckBox ctlSwitchAudioXml;
		private System.Windows.Forms.TrackBar ctlWindowSizeSlider;
		private System.Windows.Forms.TextBox ctlWindowHeight;
		private System.Windows.Forms.TextBox ctlWindowWidth;
		private System.Windows.Forms.Label lblWindowSizeX;
		private System.Windows.Forms.Button ctlResizeButton;
	}
}


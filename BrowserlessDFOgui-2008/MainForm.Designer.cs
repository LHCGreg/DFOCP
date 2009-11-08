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
			this.ctlCloseOnSuccess = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// ctlUsername
			// 
			this.ctlUsername.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlUsername.Location = new System.Drawing.Point( 126, 25 );
			this.ctlUsername.Name = "ctlUsername";
			this.ctlUsername.Size = new System.Drawing.Size( 142, 20 );
			this.ctlUsername.TabIndex = 0;
			// 
			// ctlPassword
			// 
			this.ctlPassword.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlPassword.Location = new System.Drawing.Point( 126, 64 );
			this.ctlPassword.Name = "ctlPassword";
			this.ctlPassword.Size = new System.Drawing.Size( 142, 20 );
			this.ctlPassword.TabIndex = 1;
			this.ctlPassword.UseSystemPasswordChar = true;
			// 
			// lblUsername
			// 
			this.lblUsername.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.lblUsername.AutoSize = true;
			this.lblUsername.Location = new System.Drawing.Point( 12, 28 );
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size( 55, 13 );
			this.lblUsername.TabIndex = 2;
			this.lblUsername.Text = "Username";
			// 
			// lblPassword
			// 
			this.lblPassword.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point( 12, 67 );
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size( 53, 13 );
			this.lblPassword.TabIndex = 3;
			this.lblPassword.Text = "Password";
			// 
			// ctlLaunch
			// 
			this.ctlLaunch.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.ctlLaunch.Location = new System.Drawing.Point( 64, 162 );
			this.ctlLaunch.Name = "ctlLaunch";
			this.ctlLaunch.Size = new System.Drawing.Size( 154, 55 );
			this.ctlLaunch.TabIndex = 4;
			this.ctlLaunch.Text = "Start DFO";
			this.ctlLaunch.UseVisualStyleBackColor = true;
			this.ctlLaunch.Click += new System.EventHandler( this.ctlLaunch_Click );
			// 
			// ctlCloseOnSuccess
			// 
			this.ctlCloseOnSuccess.AutoSize = true;
			this.ctlCloseOnSuccess.Checked = true;
			this.ctlCloseOnSuccess.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ctlCloseOnSuccess.Location = new System.Drawing.Point( 15, 101 );
			this.ctlCloseOnSuccess.Name = "ctlCloseOnSuccess";
			this.ctlCloseOnSuccess.Size = new System.Drawing.Size( 155, 17 );
			this.ctlCloseOnSuccess.TabIndex = 2;
			this.ctlCloseOnSuccess.Text = "Close on successful launch";
			this.ctlCloseOnSuccess.UseVisualStyleBackColor = true;
			// 
			// ctlMainForm
			// 
			this.AcceptButton = this.ctlLaunch;
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 292, 266 );
			this.Controls.Add( this.ctlCloseOnSuccess );
			this.Controls.Add( this.ctlLaunch );
			this.Controls.Add( this.lblPassword );
			this.Controls.Add( this.lblUsername );
			this.Controls.Add( this.ctlPassword );
			this.Controls.Add( this.ctlUsername );
			this.Name = "ctlMainForm";
			this.Text = "Browserless DFO Launcher";
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ctlUsername;
		private System.Windows.Forms.TextBox ctlPassword;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.Button ctlLaunch;
		private System.Windows.Forms.CheckBox ctlCloseOnSuccess;
	}
}


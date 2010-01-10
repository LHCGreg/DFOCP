using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Dfo.Login
{
	public class LaunchParams
	{
		/// <summary>
		/// Gets or sets the username to log in as.
		/// Default: null
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the password to log in with.
		/// Default: null
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to timeout after when waiting for a response from the
		/// server when logging in.
		/// Default: 15000 (15 seconds).
		/// </summary>
		public int LoginTimeoutInMs { get; set; }

		private string m_dfoDir;
		/// <summary>
		/// Gets or sets the DFO root directory.
		/// Default: The directory containing currently executing program.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars()</c>.
		/// </exception>
		public string DfoDir
		{
			get
			{
				return m_dfoDir;
			}
			set
			{
				if ( value == null )
				{
					throw new ArgumentNullException( "DfoDir" );
				}
				char[] invalidChars = Path.GetInvalidPathChars();
				if ( value.IndexOfAny( invalidChars ) != -1 )
				{
					throw new ArgumentException( "DfoDir cannot contains characters that are invalid in a path." );
				}

				m_dfoDir = value;
			}
		}

		/// <summary>
		/// Gets or sets whether to switch soundpacks to use different sounds in the game.
		/// Default: false
		/// </summary>
		public bool SwitchSoundpacks { get; set; }

		/// <summary>
		/// Gets the path of the directory containing the normal soundpacks.
		/// This will be some subdirectory of DfoDir.
		/// </summary>
		internal string SoundpackDir
		{
			get
			{
				//if ( DfoDir != null )
				//{
				//    return Path.Combine( DfoDir, "SoundPacks" );
				//}
				//else
				//{
				//    return null;
				//}
				//return "SoundPacks";
				return Path.Combine( DfoDir, "SoundPacks" );
			}
		}

		/// <summary>
		/// Gets or sets the directory containing the soundpacks to switch to. If <c>SwitchSoundpacks</c> is
		/// false, this setting is not used.
		/// Default: (The directory containing currently executing program)/SoundPacksCustom
		/// </summary>
		public string CustomSoundpackDir { get; set; }

		/// <summary>
		/// Gets or sets the directory to put the original soundpacks in while the game is running. If
		/// <c>SwitchSoundpacks</c> is false, this setting is not used.
		/// Default: (The directory containing currently executing program)/SoundPacksOriginal
		/// </summary>
		public string TempSoundpackDir { get; set; }

		/// <summary>
		/// Gets or sets whether to kill the popup at the end of the game.
		/// Default: true
		/// </summary>
		public bool ClosePopup { get; set; }

		/// <summary>
		/// Gets or sets whether to launch in windowed mode. null means "don't care" and will use
		/// whatever the user is already configured for.
		/// Default: null
		/// </summary>
		public bool? LaunchInWindowed { get; set; }

		/// <summary>
		/// Gets or sets the window class name of the main DFO window.
		/// </summary>
		internal string DfoWindowClassName { get; set; }

		/// <summary>
		/// Gets the path of the DFO launcher program. This will be in DfoDir.
		/// </summary>
		internal string DfoLauncherExe
		{
			get
			{
				//if ( DfoDir != null )
				//{
				//    return Path.Combine( DfoDir, "DFOLauncher.exe" );
				//}
				//else
				//{
				//    return null;
				//}
				return Path.Combine( DfoDir, "DFOLauncher.exe" );
			}
		}
		
		/// <summary>
		/// Gets the path to the DFO executable. This will be in DfoDir.
		/// </summary>
		internal string DfoExe
		{
			get
			{
				//if ( DfoDir != null )
				//{
				//    return Path.Combine( DfoDir, "DFO.exe" );
				//}
				//else
				//{
				//    return null;
				//}
				return Path.Combine( DfoDir, "DFO.exe" );
			}
		}

		///// <summary>
		///// Gets or sets the number of milliseconds to wait when polling for something to happen, like the main
		///// DFO window to be closed.
		///// Default: 200 (200 milliseconds)
		///// </summary>
		//internal int PollingIntervalInMs { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the main game window to be created.
		/// Default: 100 (100 milliseconds)
		/// </summary>
		internal int GameWindowCreatedPollingIntervalInMs { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the main game window to be closed.
		/// Default: 1000 (1 second)
		/// </summary>
		internal int GameDonePollingIntervalInMs { get; set; }

		///// <summary>
		///// Gets or sets the return code of the launcher program that indicates success.
		///// Default: 0
		///// </summary>
		//internal int LauncherSuccessCode { get; set; }

		internal LaunchParams()
		{
			Username = null;
			Password = null;
			LoginTimeoutInMs = 15000;
			DfoDir = AppDomain.CurrentDomain.BaseDirectory;
			SwitchSoundpacks = false;
			CustomSoundpackDir = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "SoundPacksCustom" );
			TempSoundpackDir = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "SoundPacksOriginal" );
			ClosePopup = true;
			LaunchInWindowed = null;

			DfoWindowClassName = "DFO";
			//PollingIntervalInMs = 200;
			GameDonePollingIntervalInMs = 1000;
			GameWindowCreatedPollingIntervalInMs = 100;
			//LauncherSuccessCode = 0; // ?
		}

		internal LaunchParams Clone()
		{
			LaunchParams clone = new LaunchParams();

			clone.Username = this.Username;
			clone.Password = this.Password;
			clone.LoginTimeoutInMs = this.LoginTimeoutInMs;
			clone.DfoDir = this.DfoDir;
			clone.SwitchSoundpacks = this.SwitchSoundpacks;
			clone.CustomSoundpackDir = this.CustomSoundpackDir;
			clone.TempSoundpackDir = this.TempSoundpackDir;
			clone.ClosePopup = this.ClosePopup;
			clone.LaunchInWindowed = this.LaunchInWindowed;

			clone.DfoWindowClassName = this.DfoWindowClassName;
			//clone.PollingIntervalInMs = this.PollingIntervalInMs;
			clone.GameDonePollingIntervalInMs = this.GameDonePollingIntervalInMs;
			clone.GameWindowCreatedPollingIntervalInMs = this.GameWindowCreatedPollingIntervalInMs;
			//clone.LauncherSuccessCode = this.LauncherSuccessCode;

			return clone;
		}
	}
}

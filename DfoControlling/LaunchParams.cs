using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Dfo.Controlling
{
	/// <summary>
	/// Represents a game that Dfo.Controlling can launch.
	/// </summary>
	public enum Game
	{
		/// <summary>
		/// Dungeon Fighter Online
		/// </summary>
		DFO
	}

	/// <summary>
	/// Represents parameters controlling how the game is launched and controlled.
	/// </summary>
	public class LaunchParams
	{
		private Game m_gameToLaunch = Game.DFO;
		/// <summary>
		/// Gets or sets the game to launch.
		/// Default: DFO
		/// </summary>
		public Game GameToLaunch { get { return m_gameToLaunch; } set { m_gameToLaunch = value; } }

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

		private int m_loginTimeoutInMs;
		/// <summary>
		/// Gets or sets the number of milliseconds to timeout after when waiting for a response from the
		/// server when logging in.
		/// Default: 10000 (10 seconds)
		/// </summary>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a negative number.</exception>
		public int LoginTimeoutInMs
		{
			get { return m_loginTimeoutInMs; }
			set
			{
				if ( value < 0 )
				{
					throw new ArgumentException( "Login timeout cannot be negative." );
				}
				m_loginTimeoutInMs = value;
			}
		}

		private string m_gameDir;
		/// <summary>
		/// Gets or sets the game root directory.
		/// Default: The directory containing currently executing program.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars()</c>.</exception>
		public string GameDir
		{
			get { return m_gameDir; }
			set
			{
				ValidatePath( value, "GameDir" );
				m_gameDir = value;
			}
		}

		/// <summary>
		/// Gets the default game root directory.
		/// </summary>
		private string GameDirDefault { get { return AppDomain.CurrentDomain.BaseDirectory; } }

		private ICollection<FileSwitcher> m_filesToSwitch = new List<FileSwitcher>();
		/// <summary>
		/// Gets or sets the files that should be switched to avoid them being patched over.
		/// </summary>
		public ICollection<FileSwitcher> FilesToSwitch { get { return m_filesToSwitch; } set { m_filesToSwitch = value; } }

		/// <summary>
		/// Gets or sets whether to kill the popup at the end of the game.
		/// Default: true
		/// </summary>
		public bool ClosePopup { get; set; }

		private bool m_launchInWindowed = false;
		/// <summary>
		/// Gets or sets whether to launch in windowed mode.
		/// Default: false
		/// </summary>
		public bool LaunchInWindowed { get { return m_launchInWindowed; } set { m_launchInWindowed = value; } }

		private int? m_windowWidth = null;
		/// <summary>
		/// Gets or sets the width to set the game window to immediately after launch. If both WindowWidth and
		/// WindowHeight are null, the default window size is used. If only one is null, the null value is
		/// determined by keeping the same aspect ratio as the default window size. This property only has
		/// meaning when LaunchInWindowed is set to true.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">The value was attempted to be set to a
		/// non-positive number.</exception>
		public int? WindowWidth
		{
			get { return m_windowWidth; }
			set
			{
				if ( value <= 0 )
				{
					throw new ArgumentOutOfRangeException( "WindowWidth", "Window width must be a positive number." );
				}
				m_windowWidth = value;
			}
		}

		private int? m_windowHeight = null;
		/// <summary>
		/// Gets or sets the height to set the game window to immediately after launch. If both WindowWidth and
		/// WindowHeight are null, the default window size is used. If only one is null, the null value is
		/// determined by keeping the same aspect ratio as the default window size. This property only has
		/// meaning when LaunchInWindowed is set to true.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">The value was attempted to be set to a
		/// non-positive number.</exception>
		public int? WindowHeight
		{
			get { return m_windowHeight; }
			set
			{
				if ( value <= 0 )
				{
					throw new ArgumentOutOfRangeException( "WindowHeight", "Window height must be a positive number." );
				}
				m_windowHeight = value;
			}
		}

		/// <summary>
		/// Gets the path of the DFO launcher program.
		/// </summary>
		internal string DfoLauncherExe
		{
			get
			{
				return GetFullPath( "DFOLauncher.exe" );
			}
		}

		/// <summary>
		/// Gets the path to the DFO executable.
		/// </summary>
		internal string DfoExe
		{
			get
			{
				return GetFullPath( "DFO.exe" );
			}
		}

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the main game window to be created.
		/// Default: 100 (100 milliseconds)
		/// </summary>
		internal int GameWindowCreatedPollingIntervalInMs { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the main game window to be closed.
		/// Default: 250 (250 milliseconds)
		/// </summary>
		internal int GameDonePollingIntervalInMs { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the game process to end after the
		/// main game window has closed.
		/// Default: 100 (100 milliseconds)
		/// </summary>
		internal int GameDeadPollingIntervalInMs { get; set; }

		/// <summary>
		/// Creates a new <c>LaunchParams</c> object with default values.
		/// </summary>
		public LaunchParams()
		{
			Username = null;
			Password = null;
			LoginTimeoutInMs = 10000;
			GameDir = GameDirDefault;
			ClosePopup = true;
			GameDonePollingIntervalInMs = 250;
			GameWindowCreatedPollingIntervalInMs = 100;
			GameDeadPollingIntervalInMs = 100;
		}

		/// <summary>
		/// Tries to figure out where the game directory for the game currently selected is and sets
		/// <c>GameDir</c> to it. 
		/// </summary>
		/// <exception cref="System.IO.IOException">The game directory could not be detected.</exception>
		public void AutoDetectGameDir()
		{
			GameDir = DfoLauncher.AutoDetectGameDir( GameToLaunch );
		}

		/// <summary>
		/// Makes a copy of this object.
		/// </summary>
		/// <returns>A copy of this object with the same property values.</returns>
		public LaunchParams Clone()
		{
			LaunchParams clone = new LaunchParams();

			clone.GameToLaunch = this.GameToLaunch;
			clone.Username = this.Username;
			clone.Password = this.Password;
			clone.LoginTimeoutInMs = this.LoginTimeoutInMs;
			clone.GameDir = this.GameDir;

			clone.FilesToSwitch = new List<FileSwitcher>( this.FilesToSwitch.Count );
			foreach ( FileSwitcher fileToSwitch in this.FilesToSwitch )
			{
				clone.FilesToSwitch.Add( fileToSwitch.Clone() );
			}

			clone.ClosePopup = this.ClosePopup;
			clone.LaunchInWindowed = this.LaunchInWindowed;
			clone.WindowWidth = this.WindowWidth;
			clone.WindowHeight = this.WindowHeight;

			clone.GameDonePollingIntervalInMs = this.GameDonePollingIntervalInMs;
			clone.GameWindowCreatedPollingIntervalInMs = this.GameWindowCreatedPollingIntervalInMs;
			clone.GameDeadPollingIntervalInMs = this.GameDeadPollingIntervalInMs;

			return clone;
		}

		/// <summary>
		/// Creates a string representation of this object for debugging.
		/// </summary>
		/// <returns>A dump of all properties, including internal properties. <c>Username</c> and <c>Password</c>
		/// are shown only if Logging.SensitiveDataToShow allows.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine( string.Format( "Game: {0}", GameToLaunch ) );
			builder.AppendLine( string.Format( "Close popup: {0}", ClosePopup ) );
			builder.AppendLine( string.Format( "Game dir: {0}", GameDir ) );
			builder.AppendLine( string.Format( "DFO exe: {0}", DfoExe ) );
			builder.AppendLine( string.Format( "DFO launcher exe: {0}", DfoLauncherExe ) );
			builder.AppendLine( string.Format( "Game dead polling interval: {0}", GameDeadPollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Game done polling interval: {0}", GameDonePollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Game window created polling interval: {0}", GameWindowCreatedPollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Launch in windowed: {0}", LaunchInWindowed ) );
			builder.AppendLine( string.Format( "Window width: {0}", WindowWidth ) );
			builder.AppendLine( string.Format( "Window height: {0}", WindowHeight ) );
			builder.AppendLine( string.Format( "Login timeout: {0}", LoginTimeoutInMs ) );

			foreach ( FileSwitcher fileToSwitch in FilesToSwitch )
			{
				builder.AppendLine( string.Format( "Switching file {0} with {1} using {2} as a temporary.",
					fileToSwitch.NormalFile, fileToSwitch.CustomFile, fileToSwitch.TempFile ) );
			}

			builder.AppendLine( string.Format( "Username present: {0}", Username != null ) );
			builder.AppendLine( string.Format( "Username: {0}", Username.HideSensitiveData( SensitiveData.Usernames ) ) );
			builder.AppendLine( string.Format( "Password present: {0}", Password != null ) );
			builder.Append( string.Format( "Password: {0}", Password.HideSensitiveData( SensitiveData.Passwords ) ) );

			return builder.ToString();
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

		private string GetFullPath( string possiblyRelativePath )
		{
			if ( Path.IsPathRooted( possiblyRelativePath ) )
			{
				return possiblyRelativePath;
			}
			else
			{
				return Path.Combine( GameDir, possiblyRelativePath );
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
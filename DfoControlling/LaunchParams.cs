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

		/// <summary>
		/// Gets or sets whether to switch soundpacks to use different sounds in the game. Only supported for DFO.
		/// Default: false
		/// </summary>
		public bool SwitchSoundpacks { get; set; }

		/// <summary>
		/// Gets the path of the directory containing the normal soundpacks. This property only makes sense for DFO.
		/// </summary>
		public string SoundpackDir
		{
			get
			{
				return GetFullPath( "SoundPacks" );
			}
		}

		private string m_customSoundpackDir;
		/// <summary>
		/// Gets the absolute path to the directory containing the soundpacks to switch to. If
		/// <c>SwitchSoundpacks</c> is false, this setting is not used. If <c>CustomSoundpackDirRaw</c> is set
		/// to a relative path, it is relative to <c>GameDir</c> and the value of this property will change as
		/// <c>GameDir</c> does.
		/// This property only makes sense for DFO.
		/// Default: SoundPacksCustom
		/// </summary>
		public string CustomSoundpackDir
		{
			get { return GetFullPath( m_customSoundpackDir ); }
		}

		/// <summary>
		/// Gets or sets the directory containing the soundpacks to switch to. If <c>SwitchSoundpacks</c> is
		/// false, this setting is not used. If set to a relative path, <c>CustomSoundpackDir</c> will be relative
		/// to <c>GameDir</c>. Getting this directory will return a relative path if a relative was specified,
		/// unlike <c>CustomSoundpackDir</c> which will expand to an absolute path.
		/// This property only makes sense for DFO.
		/// Default: SoundPacksCustom
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars().</c></exception>
		public string CustomSoundpackDirRaw
		{
			get { return m_customSoundpackDir; }
			set
			{
				ValidatePath( value, "CustomSoundpackDirRaw" );
				m_customSoundpackDir = value;
			}
		}

		/// <summary>
		/// Gets the default custom soundpack directory. If it is a relative path, it is relative to GameDir.
		/// This property only makes sense for DFO.
		/// </summary>
		private string CustomSoundpackDirDefault { get { return "SoundPacksCustom"; } }

		private string m_tempSoundpackDir;
		/// <summary>
		/// Gets the absolute path to the directory to put the original soundpacks in while the game is running.
		/// If <c>SwitchSoundpacks</c> is false, this setting is not used. If <c>TempSoundpackDirRaw</c> is set
		/// to a relative path, it is relative to <c>GameDir</c> and the value of this property will change as
		/// <c>GameDir</c> does.
		/// This property only makes sense for DFO.
		/// Default: SoundPacksCustom
		/// </summary>
		public string TempSoundpackDir
		{
			get { return GetFullPath( m_tempSoundpackDir ); }
		}

		/// <summary>
		/// Gets or sets the directory to put the original soundpacks in while the game is running. If
		/// <c>SwitchSoundpacks</c> is false, this setting is not used. If set to a relative path,
		/// <c>TempSoundpackDir</c> will be relative to <c>GameDir</c>. Getting this directory will return
		/// a relative path if a relative was specified, unlike <c>TempSoundpackDir</c> which will expand to
		/// an absolute path.
		/// This property only makes sense for DFO.
		/// Default: SoundPacksOriginal
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars().</c></exception>
		public string TempSoundpackDirRaw
		{
			get { return m_tempSoundpackDir; }
			set
			{
				ValidatePath( value, "TempSoundpackDirRaw" );
				m_tempSoundpackDir = value;
			}
		}

		/// <summary>
		/// Gets the default temporary soundpack directory as a subdirectory of GameDir. This only makes sense
		/// for DFO.
		/// </summary>
		private string TempSoundpackDirDefault { get { return "SoundPacksOriginal"; } }

		/// <summary>
		/// Gets or sets whether to kill the popup at the end of the game. Only supported for DFO.
		/// Default: true
		/// </summary>
		public bool ClosePopup { get; set; }

		/// <summary>
		/// Gets or sets whether to launch in windowed mode. null means "don't care" and will use
		/// whatever the user is already configured for. Only supported for DFO.
		/// Default: null
		/// </summary>
		public bool? LaunchInWindowed { get; set; }

		/// <summary>
		/// Gets or sets the window class name of the main DFO window.
		/// </summary>
		internal string DfoWindowClassName { get; set; }

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
			SwitchSoundpacks = false;
			CustomSoundpackDirRaw = CustomSoundpackDirDefault;
			TempSoundpackDirRaw = TempSoundpackDirDefault;
			ClosePopup = true;
			LaunchInWindowed = null;
			DfoWindowClassName = "DFO";
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
			clone.SwitchSoundpacks = this.SwitchSoundpacks;
			clone.CustomSoundpackDirRaw = this.CustomSoundpackDirRaw;
			clone.TempSoundpackDirRaw = this.TempSoundpackDirRaw;
			clone.ClosePopup = this.ClosePopup;
			clone.LaunchInWindowed = this.LaunchInWindowed;

			clone.DfoWindowClassName = this.DfoWindowClassName;
			clone.GameDonePollingIntervalInMs = this.GameDonePollingIntervalInMs;
			clone.GameWindowCreatedPollingIntervalInMs = this.GameWindowCreatedPollingIntervalInMs;
			clone.GameDeadPollingIntervalInMs = this.GameDeadPollingIntervalInMs;

			return clone;
		}

		/// <summary>
		/// Creates a string representation of this object for debugging.
		/// </summary>
		/// <returns>A dump of all properties, including internal properties. <c>Username</c> and <c>Password</c>
		/// are not shown for security purposes, but it does say if they are set or not.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine( string.Format( "Game: {0}", GameToLaunch ) );
			builder.AppendLine( string.Format( "Close popup: {0}", ClosePopup ) );
			builder.AppendLine( string.Format( "Custom soundpack dir: {0}", CustomSoundpackDirRaw ) );
			builder.AppendLine( string.Format( "Game dir: {0}", GameDir ) );
			builder.AppendLine( string.Format( "DFO exe: {0}", DfoExe ) );
			builder.AppendLine( string.Format( "DFO launcher exe: {0}", DfoLauncherExe ) );
			builder.AppendLine( string.Format( "DFO window class name: {0}", DfoWindowClassName ) );
			builder.AppendLine( string.Format( "Game dead polling interval: {0}", GameDeadPollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Game done polling interval: {0}", GameDonePollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Game window created polling interval: {0}", GameWindowCreatedPollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Launch in windowed: {0}", LaunchInWindowed ) );
			builder.AppendLine( string.Format( "Login timeout: {0}", LoginTimeoutInMs ) );
			builder.AppendLine( string.Format( "Soundpack dir: {0}", SoundpackDir ) );
			builder.AppendLine( string.Format( "Switch soundpacks: {0}", SwitchSoundpacks ) );
			builder.AppendLine( string.Format( "Temp soundpack dir: {0}", TempSoundpackDirRaw ) );
			builder.AppendLine( string.Format( "Username present: {0}", Username != null ) );
			builder.Append( string.Format( "Password present: {0}", Password != null ) );
			// DO NOT include the username or password!

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

		/// <summary>
		/// Determines if a string is a valid path.
		/// </summary>
		/// <param name="path">A path.</param>
		/// <returns>True if <paramref name="path"/> is not null and has no invalid characters.</returns>
		public static bool PathIsValid( string path )
		{
			if ( path == null )
			{
				return false;
			}
			char[] invalidChars = Path.GetInvalidPathChars();
			if ( path.IndexOfAny( invalidChars ) != -1 )
			{
				return false;
			}
			else
			{
				return true;
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
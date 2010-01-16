using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace Dfo.Controlling
{
	/// <summary>
	/// Represents parameters controlling how the game is launched and controlled.
	/// </summary>
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

		private string m_dfoDir;
		/// <summary>
		/// Gets or sets the DFO root directory.
		/// Default: The directory containing currently executing program.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars()</c>.</exception>
		public string DfoDir
		{
			get { return m_dfoDir; }
			set
			{
				ValidatePath( value, "DfoDir" );
				m_dfoDir = value;
			}
		}

		/// <summary>
		/// Gets the default DFO root directory.
		/// </summary>
		public string DfoDirDefault { get { return AppDomain.CurrentDomain.BaseDirectory; } }

		/// <summary>
		/// Gets or sets whether to switch soundpacks to use different sounds in the game.
		/// Default: false
		/// </summary>
		public bool SwitchSoundpacks { get; set; }

		/// <summary>
		/// Gets the path of the directory containing the normal soundpacks.
		/// This will be some subdirectory of DfoDir.
		/// </summary>
		public string SoundpackDir
		{
			get
			{
				return Path.Combine( DfoDir, "SoundPacks" );
			}
		}

		private string m_customSoundpackDir;
		/// <summary>
		/// Gets or sets the directory containing the soundpacks to switch to. If <c>SwitchSoundpacks</c> is
		/// false, this setting is not used.
		/// Default: DfoDir/SoundPacksCustom
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars()</c></exception>.
		public string CustomSoundpackDir
		{
			get { return m_customSoundpackDir; }
			set
			{
				ValidatePath( value, "CustomSoundpackDir" );
				m_customSoundpackDir = value;
			}
		}

		/// <summary>
		/// Gets the default custom soundpack directory as a subdirectory of DfoDir.
		/// </summary>
		public string CustomSoundpackDirDefault { get { return Path.Combine( DfoDir, "SoundPacksCustom" ); } }

		private string m_tempSoundpackDir;
		/// <summary>
		/// Gets or sets the directory to put the original soundpacks in while the game is running. If
		/// <c>SwitchSoundpacks</c> is false, this setting is not used.
		/// Default: DfoDir/SoundPacksOriginal
		/// </summary>
		/// <exception cref="System.ArgumentNullException">The value was attempted to be set to null.</exception>
		/// <exception cref="System.ArgumentException">The value was attempted to be set to a path that contains
		/// characters in <c>System.IO.Path.GetInvalidPathChars()</c>.</exception>
		public string TempSoundpackDir
		{
			get { return m_tempSoundpackDir; }
			set
			{
				ValidatePath( value, "TempSoundpackDir" );
				m_tempSoundpackDir = value;
			}
		}

		/// <summary>
		/// Gets the default temporary soundpack directory as a subdirectory of DfoDir.
		/// </summary>
		public string TempSoundpackDirDefault { get { return Path.Combine( DfoDir, "SoundPacksOriginal" ); } }

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
				return Path.Combine( DfoDir, "DFO.exe" );
			}
		}

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the main game window to be created.
		/// Default: 100 (100 milliseconds)
		/// </summary>
		internal int GameWindowCreatedPollingIntervalInMs { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to wait when polling for the main game window to be closed.
		/// Default: 500 (500 milliseconds)
		/// </summary>
		internal int GameDonePollingIntervalInMs { get; set; }

		/// <summary>
		/// Creates a new <c>LaunchParams</c> object with default values.
		/// </summary>
		public LaunchParams()
		{
			Username = null;
			Password = null;
			LoginTimeoutInMs = 10000;
			DfoDir = DfoDirDefault;
			SwitchSoundpacks = false;
			CustomSoundpackDir = CustomSoundpackDirDefault;
			TempSoundpackDir = TempSoundpackDirDefault;
			ClosePopup = true;
			LaunchInWindowed = null;
			DfoWindowClassName = "DFO";
			GameDonePollingIntervalInMs = 500;
			GameWindowCreatedPollingIntervalInMs = 100;
		}

		/// <summary>
		/// Tries to figure out where the DFO directory is and sets DfoDir to it and changes CustomSoundpackDir
		/// and TempSoundpackDir as well.
		/// </summary>
		/// <exception cref="System.IO.IOException">The DFO directory could not be detected.</exception>
		public void AutoDetectDfoDir()
		{
			object dfoRoot = null;
			string keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Nexon\DFO";
			string valueName = "RootPath";

			try
			{
				dfoRoot = Registry.GetValue( keyName, valueName, null );
			}
			catch ( System.Security.SecurityException ex )
			{
				ThrowAutoDetectException( keyName, valueName, ex );
			}
			catch ( IOException ex )
			{
				ThrowAutoDetectException( keyName, valueName, ex );
			}

			if ( dfoRoot == null )
			{
				ThrowAutoDetectException( keyName, valueName, "The registry value does not exist" );
			}

			string dfoRootDir = dfoRoot.ToString();
			if ( PathIsValid( dfoRootDir ) )
			{
				DfoDir = dfoRootDir;
				CustomSoundpackDir = CustomSoundpackDirDefault;
				TempSoundpackDir = TempSoundpackDirDefault;
			}
			else
			{
				throw new IOException( string.Format( "Registry value {0} in {1} is not a valid path." ) );
			}
		}

		private void ThrowAutoDetectException( string keyname, string valueName, Exception ex )
		{
			throw new IOException( string.Format( "Could not read registry value {0} in {1}. {2}",
				valueName, keyname, ex.Message ), ex );
		}

		private void ThrowAutoDetectException( string keyname, string valueName, string message )
		{
			throw new IOException( string.Format( "Could not read registry value {0} in {1}. {2}",
				valueName, keyname, message ) );
		}

		/// <summary>
		/// Makes a copy of this object.
		/// </summary>
		/// <returns>A copy of this object with the same property values.</returns>
		public LaunchParams Clone()
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
			clone.GameDonePollingIntervalInMs = this.GameDonePollingIntervalInMs;
			clone.GameWindowCreatedPollingIntervalInMs = this.GameWindowCreatedPollingIntervalInMs;

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

			builder.AppendLine( string.Format( "Close popup: {0}", ClosePopup ) );
			builder.AppendLine( string.Format( "Custom soundpack dir: {0}", CustomSoundpackDir ) );
			builder.AppendLine( string.Format( "DFO dir: {0}", DfoDir ) );
			builder.AppendLine( string.Format( "DFO exe: {0}", DfoExe ) );
			builder.AppendLine( string.Format( "DFO launcher exe: {0}", DfoLauncherExe ) );
			builder.AppendLine( string.Format( "DFO window class name: {0}", DfoWindowClassName ) );
			builder.AppendLine( string.Format( "Game done polling interval: {0}", GameDonePollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Game window created polling interval: {0}", GameWindowCreatedPollingIntervalInMs ) );
			builder.AppendLine( string.Format( "Launch in windowed: {0}", LaunchInWindowed ) );
			builder.AppendLine( string.Format( "Login timeout: {0}", LoginTimeoutInMs ) );
			builder.AppendLine( string.Format( "Soundpack dir: {0}", SoundpackDir ) );
			builder.AppendLine( string.Format( "Switch soundpacks: {0}", SwitchSoundpacks ) );
			builder.AppendLine( string.Format( "Temp soundpack dir: {0}", TempSoundpackDir ) );
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
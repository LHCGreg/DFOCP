using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace Dfo.ControlPanel
{
	static class SettingsLoader
	{
		// 2.0
		// <DFOCP_Settings SettingsVersion="major.minor">
		// Username - string
		// RememberMe - bool
		// ClosePopup - bool
		// LaunchWindowed - bool
		// SwitchSoundpacks - bool
		// </DFOCP_Settings>

		// 3.0
		// <DFOCP_Settings SettingsVersion="major.minor">
		// Username - string
		// RememberMe - bool
		// ClosePopup - bool
		// LaunchWindowed - bool
		//
		// <switchable name="soundpack">
		//   switch - bool
		// </switchable>
		// ...
		// </DFOCP_Settings>

		private static readonly string s_rootName = "DFOCP_Settings";
		private static readonly string s_settingsVersion = "SettingsVersion";
		private static readonly string s_username = "Username";
		private static readonly string s_rememberMe = "RememberMe";
		private static readonly string s_closePopup = "ClosePopup";
		private static readonly string s_launchWindowed = "LaunchWindowed";
		private static readonly string s_switchSoundpacks = "SwitchSoundpacks";
		private static readonly string s_switchableElement = "Switchable";
		private static readonly string s_switchableNameAttr = "name";
		private static readonly string s_switchableSwitch = "Switch";

		// These are for settings format versioning, which is not the same as program versioning
		private static readonly string s_majorVersion = "3";
		private static readonly string s_minorVersion = "0";

		// bool = 0 or 1, anything else is an error
		public static StartupSettings Load()
		{
			StartupSettings settings = new StartupSettings();
			XElement root;
			try
			{
				Logging.Log.InfoFormat( "Loading settings from {0}.", Paths.SettingsPath );
				root = XElement.Load( Paths.SettingsPath );
			}
			catch ( Exception ex )
			{
				// I'm kinda guessing with these exceptions...XElement.Load() does not document what exceptions it can throw -_-
				if ( ex is IOException
				  || ex is UnauthorizedAccessException
				  || ex is NotSupportedException
				  || ex is System.Security.SecurityException
				  || ex is XmlException )
				{
					Logging.Log.WarnFormat( "Could not load settings file {0}: {1}", Paths.SettingsPath, ex.Message );
					return new StartupSettings();
				}
				else
				{
					throw;
				}
			}

			XElement settingsRoot = null;
			var settingsRootCollection = root.DescendantsAndSelf( s_rootName );
			foreach ( var element in settingsRootCollection )
			{
				settingsRoot = element;
				break;
			}
			if ( settingsRoot == null )
			{
				Logging.Log.ErrorFormat( "{0} element not found.", s_rootName );
				return new StartupSettings();
			}

			string settingsVersion = GetAttribute( settingsRoot, s_settingsVersion );
			string majorVersion = s_majorVersion;
			string minorVersion = s_minorVersion;
			if ( settingsVersion == null )
			{
				Logging.Log.ErrorFormat( "Settings version not found. Assuming {0}.{1}",
					majorVersion, minorVersion );
			}
			else
			{
				string[] versionSplit = settingsVersion.Split( '.' );
				if ( versionSplit.Length < 2 )
				{
					Logging.Log.ErrorFormat( "Settings version '{0}' is badly formatted. Assuming {1}.{2}.",
						settingsVersion, majorVersion, minorVersion );
				}
				else
				{
					majorVersion = versionSplit[ 0 ];
					minorVersion = versionSplit[ 1 ];

					if ( majorVersion != s_majorVersion && majorVersion != "2")
					{
						Logging.Log.InfoFormat(
						"Major version of settings file format is {0}, which is different from {1}. Ignoring file and using default settings.",
						majorVersion, s_majorVersion );
						return new StartupSettings();
					}
				}
			}

			settings.Username = GetString( settingsRoot, s_username );
			settings.RememberUsername = GetBool( settingsRoot, s_rememberMe );
			settings.ClosePopup = GetBool( settingsRoot, s_closePopup );
			settings.LaunchWindowed = GetBool( settingsRoot, s_launchWindowed );

			if ( majorVersion == "2" )
			{
				settings.SwitchFile[ SwitchableFile.Soundpacks ] = GetBool( settingsRoot, s_switchSoundpacks );
			}
			else
			{
				var switchableCollection = settingsRoot.Elements( s_switchableElement );
				foreach ( XElement switchable in switchableCollection )
				{
					string switchableName = GetAttribute( switchable, s_switchableNameAttr );
					if ( switchableName == null )
					{
						continue;
					}
					bool? whetherToSwitch = GetBool( switchable, s_switchableSwitch );

					settings.SwitchFile[ switchableName ] = whetherToSwitch;
				}
			}

			Logging.Log.Info( "Settings loaded." );

			return settings;
		}

		private static bool? GetBool( XElement settingsRoot, string elementName )
		{
			XElement element = settingsRoot.Element( elementName );
			if ( element == null )
			{
				return null;
			}
			else
			{
				if ( element.Value == "1" )
				{
					return true;
				}
				else if ( element.Value == "0" )
				{
					return false;
				}
				else
				{
					Logging.Log.ErrorFormat( "Element '{0}' has value '{1}', expected boolean.", elementName, element.Value );
					return null;
				}
			}
		}

		private static string GetString( XElement settingsRoot, string elementName )
		{
			XElement element = settingsRoot.Element( elementName );
			if ( element == null )
			{
				return null;
			}
			else
			{
				return element.Value;
			}
		}

		private static string GetAttribute( XElement element, string attributeName )
		{
			XAttribute attribute = element.Attribute( attributeName );
			if ( attribute != null )
			{
				return attribute.Value;
			}
			else
			{
				return null;
			}
		}

		public static void Save( StartupSettings settings )
		{
			Logging.Log.InfoFormat( "Saving settings to {0}.", Paths.SettingsPath );

			XElement root = new XElement( s_rootName );
			string settingsVersion = string.Format( "{0}.{1}", s_majorVersion, s_minorVersion );
			root.SetAttributeValue( s_settingsVersion, settingsVersion );
			if ( settings.RememberUsername == true )
			{
				AddElement( root, s_rememberMe, GetBoolString( settings.RememberUsername ) );
				AddElement( root, s_username, settings.Username );
			}
			AddElement( root, s_closePopup, GetBoolString( settings.ClosePopup ) );
			AddElement( root, s_launchWindowed, GetBoolString( settings.LaunchWindowed ) );
			foreach ( string switchableName in settings.SwitchFile.Keys )
			{
				XElement switchableElement = new XElement( s_switchableElement );
				AddElement( switchableElement, s_switchableSwitch, GetBoolString( settings.SwitchFile[ switchableName ].Value ) );
				switchableElement.SetAttributeValue( s_switchableNameAttr, switchableName );
				root.Add( switchableElement );
			}

			try
			{
				root.Save( Paths.SettingsPath );
				Logging.Log.InfoFormat( "Settings saved." );
			}
			catch ( Exception ex )
			{
				// I'm kinda guessing with these exceptions...XElement.Save() does not document what exceptions it can throw -_-
				if ( ex is IOException
				  || ex is UnauthorizedAccessException
				  || ex is NotSupportedException
				  || ex is System.Security.SecurityException )
				{
					Logging.Log.ErrorFormat( "Could not save settings file {0}: {1}", Paths.SettingsPath, ex.Message );
				}
				else
				{
					throw;
				}
			}
		}

		private static void AddElement( XElement parent, string elementName, string elementValue )
		{
			if ( elementValue != null )
			{
				parent.Add( new XElement( elementName, elementValue ) );
			}
		}

		private static string GetBoolString( bool? b )
		{
			if ( b.HasValue )
			{
				if ( b.Value )
				{
					return "1";
				}
				else
				{
					return "0";
				}
			}
			else
			{
				return null;
			}
		}

		public static void ApplySetting<TSetting>( bool cmdHas, TSetting fromCmd, bool settingsHas, TSetting fromSettings, Func<TSetting, string> validate, string settingName, Action<TSetting> apply, TSetting defaultValue, bool suppressValueDisplay )
		{
			Logging.Log.DebugFormat( "Getting value for setting '{0}'.", settingName );
			TSetting settingValue = default( TSetting );
			bool valueFound = false;

			if ( !valueFound && cmdHas )
			{
				Logging.Log.DebugFormat( "Command-line value found." );

				string error = null;
				if ( validate != null )
				{
					error = validate( fromCmd );
				}

				string valueString = suppressValueDisplay ? "(hidden)" : fromCmd.ToString();
				if ( error == null )
				{
					Logging.Log.DebugFormat( "{0} = {1} (from command-line)", settingName, valueString );
					settingValue = fromCmd;
					valueFound = true;
				}
				else
				{
					Logging.Log.ErrorFormat( "Error in command-line value for {0} = {1}. {2} Trying settings file.", settingName, valueString, error );
				}
			}

			if ( !valueFound && settingsHas )
			{
				Logging.Log.DebugFormat( "Settings file value found." );

				string error = null;
				if ( validate != null )
				{
					error = validate( fromSettings );
				}

				string valueString = suppressValueDisplay ? "(hidden)" : fromSettings.ToString();
				if ( error == null )
				{
					Logging.Log.DebugFormat( "{0} = {1} (from settings file)", settingName, valueString );
					settingValue = fromSettings;
					valueFound = true;
				}
				else
				{
					Logging.Log.ErrorFormat( "Error in settings file value for {0} = {1}. {2} Using default.", settingName, valueString, error );
				}
			}

			if ( !valueFound )
			{
				string valueString = suppressValueDisplay ? "(hidden)" : string.Format( "{0}", defaultValue ); // defaultValue can be null, so can't use defaultValue.ToString()
				settingValue = defaultValue;
				valueFound = true;
				Logging.Log.DebugFormat( "{0} = {1} (default)", settingName, valueString );
			}

			apply( settingValue );
		}

		public static void ApplySettingStruct<TSetting>( TSetting? fromCmd, TSetting? fromSettings, Func<TSetting, string> validate, string settingName, Action<TSetting> apply, TSetting defaultValue, bool suppressValueDisplay ) where TSetting : struct
		{
			ApplySetting<TSetting>( fromCmd.HasValue, fromCmd.HasValue ? fromCmd.Value : default( TSetting ),
				fromSettings.HasValue, fromSettings.HasValue ? fromSettings.Value : default( TSetting ),
				validate, settingName, apply, defaultValue, suppressValueDisplay );
		}

		public static void ApplySettingClass<TSetting>( TSetting fromCmd, TSetting fromSettings, Func<TSetting, string> validate, string settingName, Action<TSetting> apply, TSetting defaultValue, bool suppressValueDisplay ) where TSetting : class
		{
			ApplySetting<TSetting>( fromCmd != null, fromCmd, fromSettings != null, fromSettings, validate,
				settingName, apply, defaultValue, suppressValueDisplay );
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
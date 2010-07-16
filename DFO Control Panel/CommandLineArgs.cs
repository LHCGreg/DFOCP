using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	class CommandLineArgs
	{
		public bool ShowHelp { get; private set; }
		public bool ShowVersion { get; private set; }
		public bool Gui { get; private set; }

		private StartupSettings m_settings = new StartupSettings();
		public StartupSettings Settings { get { return m_settings; } }

		private List<string> m_messages = new List<string>();
		public IEnumerable<string> Messages { get { return m_messages; } }

		public OptionSet GetOptionSet()
		{
			OptionSet optionSet = new OptionSet()
			{
				{ "?|h|help", "Show this message and exit.", argExistence => ShowHelp = (argExistence != null) },
				{ "v|version", "Show version information and exit.", argExistence => ShowVersion = (argExistence != null) },
				{ "gui", "Start the GUI with whatever parameters have been supplied.", argExistence => Gui = (argExistence != null) },
				{ "cli", "Explicitly says you want the command-line version to be used.", argExistence => Gui = !(argExistence != null) },
				{ "u|username=", "Username to use when logging in.", argValue => Settings.Username = argValue },
				{ "p|pw|password=", "Password to use when logging in.", argValue => Settings.Password = argValue },
				{ "closepopup", "Close the popup when the game is done. This is the default.", argExistence => Settings.ClosePopup = (argExistence != null) },
				{ "noclosepopup", "Don't close the popup when the game is done.", argExistence => Settings.ClosePopup = !(argExistence != null) },
				{ "window|windowed", "Launch the game in windowed mode.", argExistence => Settings.LaunchWindowed = (argExistence != null) },
				{ "full", "Don't launch the game in windowed mode. This is the default.", argExistence => Settings.LaunchWindowed = !(argExistence != null) },
				{ "dfodir=", "Directory where DFO is. Defaults to the autodetected DFO directory.",
					argValue => { ThrowIfPathNotValid(argValue, "dfodir"); Settings.DfoDir = argValue; } },
				{ "<>", argValue => // Default handler - Report unrecognized arguments
					{
						string message = string.Format("Unrecognized command-line argument: {0}", argValue);

						// This little hack is necessary because we don't have a console at this time
						// - we don't know if we're doing the GUI or the CLI yet
						m_messages.Add(message);
					}
				}
			};

			// Add the arguments for all switchable files
			foreach ( SwitchableFile switchableFile in Settings.SwitchableFiles.Values )
			{
				// Cannot use switchableFile.Name in the lambdas because the switchableFile
				// variable has only one instance - so the switch and noswitch arguments
				// for all switchables would actually be bound to the last switchable.
				SwitchableFile switchableFileInstance = switchableFile;
				string switchableName = switchableFile.Name;

				optionSet.Add( string.Format( "{0}", switchableFile.WhetherToSwitchArg ),
					string.Format( "Switch {0}", switchableFile.NormalFile ),
					argExistence => Settings.SwitchFile[ switchableName ] = ( argExistence != null ) );

				optionSet.Add( string.Format( "no{0}", switchableFile.WhetherToSwitchArg ),
					string.Format( "Don't switch {0}", switchableFile.NormalFile ),
					argExistence => Settings.SwitchFile[ switchableName ] = !( argExistence != null ) );

				optionSet.Add( string.Format( "{0}=", switchableFile.CustomFileArg ),
					string.Format( "File or directory to switch {0} with. If a relative path is given, it is relative to dfodir. Defaults to {1}.",
					switchableFile.NormalFile, switchableFile.DefaultCustomFile ),
					 argValue =>
					 {
						 ThrowIfPathNotValid( argValue, switchableFileInstance.CustomFileArg );
						 switchableFileInstance.CustomFile = argValue;
					 } );

				optionSet.Add( string.Format( "{0}=", switchableFile.TempFileArg ),
					string.Format( "File or directory to move {0} to while the game is running if switching it. If a relative path is given, it is relative to dfodir. Defaults to {1}.",
					switchableFile.NormalFile, switchableFile.DefaultTempFile ),
					argValue =>
					{
						ThrowIfPathNotValid( argValue, switchableFileInstance.TempFileArg );
						switchableFileInstance.TempFile = argValue;
					} );
			}

			return optionSet;
		}

		private void ThrowIfPathNotValid( string path, string optionName )
		{
			if ( !Utilities.PathIsValid( path ) )
			{
				throw new OptionException( string.Format( "{0} is not a valid path.", path ), optionName );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="Ndesk.Options.OptionException">Badly-formatted arguments?</exception>
		public CommandLineArgs( string[] args )
		{
			if ( args.Length == 0 )
			{
				Gui = true;
			}
			else
			{
				Gui = false;
			}

			ShowHelp = false;
			ShowVersion = false;

			OptionSet optionSet = GetOptionSet();

			optionSet.Parse( args );
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine( string.Format( "Show help = {0}", ShowHelp ) );
			builder.AppendLine( string.Format( "Show version = {0}", ShowVersion ) );
			builder.AppendLine( string.Format( "Use GUI = {0}", Gui ) );
			builder.AppendLine( string.Format( "Username specified = {0}", Settings.Username != null ) ); // Don't show username for security reasons
			builder.AppendLine( string.Format( "Password specified = {0}", Settings.Password != null ) ); // Don't show password for security reasons
			builder.AppendLine( string.Format( "Close popup = {0}", Settings.ClosePopup ) );
			builder.AppendLine( string.Format( "Launch windowed = {0}", Settings.LaunchWindowed ) );
			builder.AppendLine( string.Format( "DFO dir = {0}", Settings.DfoDir ) );

			foreach ( ISwitchableFile switchableFile in Settings.SwitchableFiles.Values )
			{
				builder.AppendLine( string.Format( "Switch {0} = {1}",
					switchableFile.NormalFile, Settings.SwitchFile[switchableFile.Name] ) );
				builder.AppendLine( string.Format( "Custom {0} file = {1}",
					switchableFile.NormalFile, switchableFile.CustomFile ) );
				builder.AppendLine( string.Format( "Temp {0} file = {1}",
					switchableFile.NormalFile, switchableFile.TempFile ) );
			}

			return builder.ToString();
		}

		public static void DisplayHelp( TextWriter writer )
		{
			CommandLineArgs emptyArgs = new CommandLineArgs( new string[] { } ); // This is a bit of a hack, but I can't think of a better way to do it
			writer.WriteLine( "Usage: {0} [OPTIONS]", GetProgramName() );
			writer.WriteLine();
			writer.WriteLine( "Parameters:" );
			emptyArgs.GetOptionSet().WriteOptionDescriptions( writer );
		}

		public static string GetProgramName()
		{
			string[] argsWithProgramName = System.Environment.GetCommandLineArgs();
			string programName;
			if ( argsWithProgramName[ 0 ].Equals( string.Empty ) )
			{
				// "If the file name is not available, the first element is equal to String.Empty."
				// Doesn't say why that would happen, but ok...
				programName = ( new System.Reflection.AssemblyName( System.Reflection.Assembly.GetExecutingAssembly().FullName ).Name ) + ".exe";
			}
			else
			{
				programName = Path.GetFileName( argsWithProgramName[ 0 ] );
			}

			return programName;
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
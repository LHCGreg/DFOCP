using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;

namespace Dfo.BrowserlessDfoGui
{
	class CommandLineArgs
	{
		public bool ShowHelp { get; private set; }
		public bool ShowVersion { get; private set; }
		public bool Gui { get; private set; }

		//public string Username { get; private set; }
		//public string Password { get; private set; }
		//public bool? ClosePopup { get; private set; }
		//public bool? LaunchWindowed { get; private set; }
		//public bool? SwitchSoundpacks { get; private set; }
		//public string DfoDir { get; private set; }
		//public string CustomSoundpackDir { get; private set; }
		//public string TempSoundpackDir { get; private set; }
		private StartupSettings m_settings = new StartupSettings();
		public StartupSettings Settings { get { return m_settings; } }

		public OptionSet GetOptionSet()
		{
			OptionSet optionSet = new OptionSet()
			{
				{ "h|help", "Show this message and exit.", argExistence => ShowHelp = (argExistence != null) },
				{ "v|version", "Show version information and exit.", argExistence => ShowVersion = (argExistence != null) },
				{ "gui", "Start the GUI with whatever parameters have been supplied.", argExistence => Gui = (argExistence != null) },
				{ "cli", "Force the command-line version to be used.", argExistence => Gui = !(argExistence != null) },
				{ "u|username=", "Username to use when logging in.", argValue => Settings.Username = argValue },
				{ "pw|password=", "Password to use when logging in.", argValue => Settings.Password = argValue },
				{ "closepopup", "Close the popup when the game is done. This is the default.", argExistence => Settings.ClosePopup = (argExistence != null) },
				{ "noclosepopup", "Don't close the popup when the game is done.", argExistence => Settings.ClosePopup = !(argExistence != null) },
				{ "windowed", "Launch the game in windowed mode.", argExistence => Settings.LaunchWindowed = (argExistence != null) },
				{ "full", "Don't launch the game in windowed mode. This is the default.", argExistence => Settings.LaunchWindowed = !(argExistence != null) },
				{ "soundswitch", "Switch soundpacks.", argExistence => Settings.SwitchSoundpacks = (argExistence != null) },
				{ "nosoundswitch", "Don't switch soundpacks. This is the default.", argExistence => Settings.SwitchSoundpacks = !(argExistence != null) },
				{ "dfodir=", "Directory where DFO is. Defaults to the directory this program is in.", argValue => Settings.DfoDir = argValue },
				{ "customsounddir=", "Directory where custom soundpacks are if switching soundpacks. Defaults to dfodir/SoundPacksCustom.", argValue => Settings.CustomSoundpackDir = argValue },
				{ "tempsounddir=", "Directory to rename the normal soundpack directory while the game is running if switching soundpacks. Defaults to dfodir/SoundPacksOriginal.", argValue => Settings.TempSoundpackDir = argValue },

			};

			return optionSet;
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
			//ClosePopup = true;
			//LaunchWindowed = false;
			//SwitchSoundpacks = false;
			//DfoDir = AppDomain.CurrentDomain.BaseDirectory;

			OptionSet optionSet = GetOptionSet();

			optionSet.Parse( args );

			//if ( CustomSoundpackDir == null )
			//{
			//    CustomSoundpackDir = Path.Combine( DfoDir, "SoundPacksCustom" );
			//}

			//if ( TempSoundpackDir == null )
			//{
			//    TempSoundpackDir = Path.Combine( DfoDir, "SoundPacksOriginal" );
			//}
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
			builder.AppendLine( string.Format( "Switch soundpacks = {0}", Settings.SwitchSoundpacks ) );
			builder.AppendLine( string.Format( "DFO dir = {0}", Settings.DfoDir ) );
			builder.AppendLine( string.Format( "Custom soundpack dir = {0}", Settings.CustomSoundpackDir ) );
			builder.Append( string.Format("Temp soundpack dir = {0}", Settings.TempSoundpackDir ));

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

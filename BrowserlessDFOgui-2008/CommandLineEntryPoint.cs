using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Dfo.Login;

namespace Dfo.BrowserlessDfoGui
{
	static partial class Program
	{
		static void CommandLineEntryPoint( CommandLineArgs parsedArgs )
		{
			if ( parsedArgs.ShowVersion )
			{
				Console.WriteLine( "{0} version {1}", VersionInfo.AssemblyTitle, VersionInfo.AssemblyVersion );
				Console.WriteLine( VersionInfo.AssemblyCopyright );
				Console.WriteLine( VersionInfo.LicenseStatement );
			}
			if ( parsedArgs.ShowHelp )
			{
				CommandLineArgs.DisplayHelp( Console.Out );
			}

			if ( parsedArgs.ShowVersion || parsedArgs.ShowHelp )
			{
				Environment.Exit( 0 );
			}

			bool invalidArgs = false;

			if ( parsedArgs.Username == null )
			{
				Logging.Log.FatalFormat( "You must supply a username." );
				invalidArgs = true;
			}

			if ( parsedArgs.Password == null )
			{
				Logging.Log.FatalFormat( "You must supply a password." );
				invalidArgs = true;
			}

			if ( invalidArgs )
			{
				Console.WriteLine( "Try {0} --help for usage information.", CommandLineArgs.GetProgramName() );
				Environment.Exit( 1 );
			}

			//string username = args[ 0 ];
			//string password = args[ 1 ];
			//try
			//{
			//    DfoLogin.StartDfo( username, password );
			//}
			//catch ( DfoLaunchException ex )
			//{
			//    Console.WriteLine( ex.Message );
			//    System.Environment.Exit( 2 );
			//}
		}
	}
}
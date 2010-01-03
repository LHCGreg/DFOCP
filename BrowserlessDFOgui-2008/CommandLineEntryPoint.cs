using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Dfo.Login;

namespace Dfo.BrowserlessDfoGui
{
	static partial class Program
	{
		static void CommandLineEntryPoint( string[] args )
		{
			if ( args.Length < 2 )
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
				Console.WriteLine( "Usage: {0} YourUserName YourPassword", programName );
				System.Environment.Exit( 1 );
			}

			string username = args[ 0 ];
			string password = args[ 1 ];
			try
			{
				DfoLogin.StartDfo( username, password );
			}
			catch ( DfoLaunchException ex )
			{
				Console.WriteLine( ex.Message );
				System.Environment.Exit( 2 );
			}
		}
	}
}
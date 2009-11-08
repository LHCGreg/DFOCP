using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Dfo.Login;

namespace BrowserlessDFOcmd
{
	class Program
	{
		/// <summary>
		/// Usage: BrowserlessDFOcmd.exe YourUserName YourPassword
		/// </summary>
		/// <param name="args"></param>
		static void Main( string[] args )
		{
			if ( args.Length < 2 )
			{
				string[] argsWithProgramName = System.Environment.GetCommandLineArgs();
				string programName;
				if ( argsWithProgramName[ 0 ].Equals( string.Empty ) )
				{
					// "If the file name is not available, the first element is equal to String.Empty."
					// Doesn't say why that would happen, but ok...
					programName = (new System.Reflection.AssemblyName(System.Reflection.Assembly.GetExecutingAssembly().FullName ).Name ) + ".exe";
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

/*
 Copyright 2009 Greg Najda

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
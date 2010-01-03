using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Dfo.BrowserlessDfoGui
{
	static partial class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( string[] args )
		{
			if ( args.Length == 0 ) // If no arguments, do the GUI. Otherwise, do the command-line version.
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault( false );
				Application.Run( new ctlMainForm() );
			}
			else
			{
				bool attachedToParentsConsole = AttachConsole( ATTACH_PARENT_PROCESS ); // Attach to parent's console for output
				if ( !attachedToParentsConsole )
				{
					AllocConsole(); // If couldn't attach to parent's console (maybe it doesn't have one), create a new console
				}

				CommandLineEntryPoint( args );
			}
		}

		[DllImport( "kernel32.dll" )]
		private static extern bool AllocConsole();

		[DllImport( "kernel32.dll" )]
		static extern bool AttachConsole( int dwProcessId );
		private const int ATTACH_PARENT_PROCESS = -1;

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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using NDesk.Options;

namespace Dfo.ControlPanel
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( string[] args )
		{
			Thread.CurrentThread.Name = "Main";
			Logging.SetUpLogging();

			AppDomain.CurrentDomain.UnhandledException += ( object sender, UnhandledExceptionEventArgs e ) =>
				{
					if ( e.ExceptionObject is Exception )
					{
						Logging.Log.Fatal( "FATAL UNCAUGHT EXCEPTION.", (Exception)e.ExceptionObject );
					}
					else
					{
						Logging.Log.Fatal( "FATAL UNCAUGHT EXCEPTION." );
					}
				};

			Logging.Log.InfoFormat( "{0} version {1} started.", VersionInfo.AssemblyTitle, VersionInfo.AssemblyVersion );
			Logging.Log.Debug( "Parsing command-line arguments." );

			CommandLineArgs parsedArgs = null;
			try
			{
				parsedArgs = new CommandLineArgs( args );
			}
			catch ( OptionException ex )
			{
				EnsureConsoleExists();
				Logging.Log.Fatal( ex.Message );
				Logging.Log.FatalFormat( "Try {0} --help for more information.", CommandLineArgs.GetProgramName() );

				Environment.Exit( 1 );
			}

			Logging.Log.DebugFormat( "Command line parsed. Argument dump:{0}{1}", Environment.NewLine, parsedArgs );

			if ( parsedArgs.Gui )
			{
				Logging.Log.Info( "Starting GUI." );
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault( false );
				Application.Run( new ctlMainForm( parsedArgs ) );
			}
			else
			{
				EnsureConsoleExists();
				Logging.Log.Info( "Starting command-line launcher." );

				using ( CommandLineEntryPoint cmd = new CommandLineEntryPoint( parsedArgs ) )
				{
					Environment.ExitCode = cmd.Run();
				}
			}
		}

		[DllImport( "kernel32.dll", SetLastError = true )]
		private static extern bool AllocConsole();

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern IntPtr GetConsoleWindow();

		[DllImport( "kernel32.dll", SetLastError = true )]
		static extern bool AttachConsole( int dwProcessId );
		private const int ATTACH_PARENT_PROCESS = -1;

		/// <summary>
		/// Use the function to make a console exists if you need to output to the screen. If there is no
		/// console yet, it tries to attach to the parent process's console. If it cannot, it creates a new
		/// console.
		/// </summary>
		private static void EnsureConsoleExists()
		{
			IntPtr currentConsoleWinHandle = GetConsoleWindow();
			if ( currentConsoleWinHandle == IntPtr.Zero )
			{
				bool attachedToParentsConsole = AttachConsole( ATTACH_PARENT_PROCESS ); // Attach to parent's console for output
				if ( !attachedToParentsConsole )
				{
					AllocConsole(); // If couldn't attach to parent's console (maybe it doesn't have one), create a new console
				}
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
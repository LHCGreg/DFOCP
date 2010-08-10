using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net2common;

namespace Dfo.ControlPanel
{
	static class Logging
	{
		public static Common.Logging.ILog Log { get; set; }
		private static TimeSpan MaxLogAge { get { return new TimeSpan( 7, 0, 0, 0 ); } } // 7 days

		public static void SetUpLogging()
		{
			log4net.ILog log4netlog = log4net.LogManager.GetLogger( "Main logger" );
			Log = new Log4NetCommonLogger( log4netlog );

			SetUpFileLogger();
			WriteLogPrologue();
			RemoveOldLogFiles();
			SetUpConsoleLogger();
			// Order is important - don't log anything to console until we're sure a console exists
			// or that it will never exist, because logging to console before it exists makes later
			// writes to the console not work for some reason.
		}

		private static void WriteLogPrologue()
		{
			Logging.Log.InfoFormat( "{0} version {1} started.", VersionInfo.AssemblyTitle, VersionInfo.AssemblyVersion );
			Logging.Log.DebugFormat( "CLR Version: {0}", Environment.Version );
			Logging.Log.DebugFormat( "Operating System: {0}", Environment.OSVersion );
			Logging.Log.DebugFormat( "Number of processors: {0}", Environment.ProcessorCount );
		}

		private static void SetUpConsoleLogger()
		{
			string consolePattern = "%message%newline";
			log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout( consolePattern );
			layout.Header = "";
			layout.Footer = "";
			layout.ActivateOptions();

			log4net.Appender.ConsoleAppender consoleAppender = new log4net.Appender.ConsoleAppender();
			//consoleAppender.ErrorHandler = new LoggingErrorHandler();
			consoleAppender.Name = "Console appender";
			consoleAppender.Threshold = log4net.Core.Level.Warn;
			consoleAppender.Layout = layout;
			consoleAppender.Target = "Console.Out";

			consoleAppender.ActivateOptions();

			// A slight bit of hackery in order to programatically set more than one appender (which isn't supported by BasicConfigurator)
			log4net.Repository.Hierarchy.Logger root = ( (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository() ).Root;
			root.AddAppender( consoleAppender );
			root.Repository.Configured = true;
		}

		private static void SetUpFileLogger()
		{
			string filePattern = "%date %-15.15thread %-5level: %message%newline";
			log4net.Layout.ILayout layout;
			log4net.Layout.PatternLayout normalTextLayout = new log4net.Layout.PatternLayout( filePattern );
			normalTextLayout.ActivateOptions();
			layout = normalTextLayout;

			log4net.Appender.FileAppender fileAppender = new log4net.Appender.FileAppender();
			fileAppender.AppendToFile = false;
			fileAppender.Encoding = Encoding.UTF8;
			fileAppender.File = Paths.LogPath;
			fileAppender.ImmediateFlush = true;
			fileAppender.Layout = layout;
			fileAppender.LockingModel = new log4net.Appender.FileAppender.ExclusiveLock();
			fileAppender.Name = "File appender";
			fileAppender.Threshold = log4net.Core.Level.Debug;

			fileAppender.ActivateOptions();

			// A slight bit of hackery in order to programatically set more than one appender (which isn't supported by BasicConfigurator)
			log4net.Repository.Hierarchy.Logger root = ( (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository() ).Root;
			root.AddAppender( fileAppender );
			root.Repository.Configured = true;
		}

		/// <summary>
		/// Deletes all log files with last write times mores than MaxLogAge ago.
		/// </summary>
		private static void RemoveOldLogFiles()
		{
			Logging.Log.DebugFormat( "Checking for log files older than {0}...", MaxLogAge );

			DateTime nowUtc = DateTime.Now.ToUniversalTime();

			string[] logFilePaths;
			try
			{
				logFilePaths = Directory.GetFiles( Paths.LogDir, "*.log", SearchOption.TopDirectoryOnly );
			}
			catch ( Exception ex )
			{
				if ( ex is IOException || ex is UnauthorizedAccessException || ex is DirectoryNotFoundException )
				{
					Logging.Log.WarnFormat( "Could not get a list of files in {0}: {1}", Paths.LogDir, ex.Message );
					return;
				}
				else
				{
					throw;
				}
			}

			foreach ( string logFilePath in logFilePaths )
			{
				DateTime lastWriteTimeUtc;
				try
				{
					lastWriteTimeUtc = File.GetLastWriteTimeUtc( logFilePath );
				}
				catch ( UnauthorizedAccessException ex )
				{
					Logging.Log.WarnFormat( "Could not get timestamp for {0}: {1}", logFilePath, ex.Message );
					continue;
				}

				TimeSpan fileAge = nowUtc - lastWriteTimeUtc;
				if ( fileAge > MaxLogAge )
				{
					try
					{
						File.Delete( logFilePath );
						Logging.Log.DebugFormat( "Deleted old log file {0}", logFilePath );
					}
					catch ( Exception ex )
					{
						if ( ex is DirectoryNotFoundException || ex is IOException || ex is UnauthorizedAccessException )
						{
							Logging.Log.WarnFormat( "Could not delete old log file {0}: {1}", logFilePath, ex.Message );
							continue;
						}
						else
						{
							throw;
						}
					}
				}
			}

			Logging.Log.Debug( "Done checking for old log files." );
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
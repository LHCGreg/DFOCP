using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dfo.BrowserlessDfoGui
{
	static class Logging
	{
		public static log4net.ILog Log { get; set; }

		public static void SetUpLogging()
		{
			Log = log4net.LogManager.GetLogger( "Main logger" );
			SetUpConsoleLogger();
			SetUpFileLogger();
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

			fileAppender.ErrorHandler = new LoggingErrorHandler();

			fileAppender.ActivateOptions();

			// A slight bit of hackery in order to programatically set more than one appender (which isn't supported by BasicConfigurator)
			log4net.Repository.Hierarchy.Logger root = ( (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository() ).Root;
			root.AddAppender( fileAppender );
			root.Repository.Configured = true;
		}
	}

	class LoggingErrorHandler : log4net.Core.IErrorHandler
	{
		public void Error( string message )
		{
			System.Diagnostics.Debug.WriteLine( message );
		}

		public void Error( string message, Exception e )
		{
			System.Diagnostics.Debug.WriteLine( message );
			System.Diagnostics.Debug.WriteLine( e );
		}

		public void Error( string message, Exception e, log4net.Core.ErrorCode errorCode )
		{
			System.Diagnostics.Debug.WriteLine( message );
			System.Diagnostics.Debug.WriteLine( e );
			System.Diagnostics.Debug.WriteLine( errorCode );
		}
	}
}

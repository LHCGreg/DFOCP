using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;

namespace Dfo.Controlling
{
	public static class Logging
	{
		// Protect access to Log. It is very possible for calling code to try to set Log while
		// a DfoLauncher monitor thread is trying to get the log to log something.
		//
		// Because calling code might also want to modify properties of the logger, the underlying
		// logger must be thread-safe.
		private static object logLock = new object();
		private static ILog log = new NoOpLogger();
		
		/// <summary>
		/// Gets or sets the logger that the Dfo.Controlling library should use. Logging can ease
		/// troubleshooting. Setting this property to null is shorthand for setting it to a
		/// Common.Logging.Simple.NoOpLogger. If a logger is never set, a Common.Logging.Simple.NoOpLogger
		/// is used. THE UNDERLYING LOGGER MUST BE THREADSAFE!
		/// </summary>
		public static ILog Log
		{
			get
			{
				lock ( logLock )
				{
					return log;
				}
			}
			set
			{
				if ( value == null )
				{
					lock ( logLock )
					{
						log = new NoOpLogger();
					}
				}
				else
				{
					lock ( logLock )
					{
						log = value;
					}
				}
			}
		}
	}
}

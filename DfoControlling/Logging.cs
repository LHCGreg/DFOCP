using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Common.Logging.Simple;

namespace Dfo.Controlling
{
	/// <summary>
	/// Specifies a set of kinds of sensitive data.
	/// </summary>
	[Flags]
	public enum SensitiveData
	{
		/// <summary>
		/// No sensitive data.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// Cookies used to authenticate to the game server. For a short period of time they allow anyone
		/// to authenticate to the game as a certain user. It is unknown how much, if any, information about
		/// username and password is directly encoded in the cookie.
		/// </summary>
		LoginCookies = 0x0001,

		/// <summary>
		/// Usernames.
		/// </summary>
		Usernames = 0x0002,

		/// <summary>
		/// Passwords.
		/// </summary>
		Passwords = 0x0004,

		/// <summary>
		/// All sensitive data.
		/// </summary>
		All = LoginCookies | Usernames | Passwords
	}

	public static class Logging
	{
		// Protect access to Log. It is very possible for calling code to try to set Log while
		// a DfoLauncher monitor thread is trying to get the log to log something.
		//
		// Because calling code might also want to modify properties of the logger, the underlying
		// logger must be thread-safe.
		private static object s_logLock = new object();
		private static ILog s_log = new NoOpLogger();
		private static SensitiveData s_sensitiveDataToLog = SensitiveData.None;

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
				lock ( s_logLock )
				{
					return s_log;
				}
			}
			set
			{
				if ( value == null )
				{
					lock ( s_logLock )
					{
						s_log = new NoOpLogger();
					}
				}
				else
				{
					lock ( s_logLock )
					{
						s_log = value;
					}
				}
			}
		}

		/// <summary>
		/// Specifies what sensitive data (such as usernames and passwords) should be logged.
		/// The default is no sensitive data at all.
		/// </summary>
		public static SensitiveData SensitiveDataToLog
		{
			get
			{
				lock ( s_logLock )
				{
					return s_sensitiveDataToLog;
				}
			}
			set
			{
				lock ( s_logLock )
				{
					s_sensitiveDataToLog = value;
				}
			}
		}
	}

	public static class DfoControllingLogHelpers
	{
		public static string HideSensitiveData( this string dataString, SensitiveData kindOfData, SensitiveData kindsToLog )
		{
			return ( ( kindsToLog & kindOfData ) == kindOfData ) ? dataString : "(hidden)";
		}

		internal static string HideSensitiveData( this string dataString, SensitiveData kindOfData )
		{
			return dataString.HideSensitiveData( kindOfData, Logging.SensitiveDataToLog );
		}
	}
}

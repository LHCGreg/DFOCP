using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Dfo.Controlling
{
	/// <summary>
	/// A class containing methods for authenticating to the DFO server.
	/// </summary>
	public static class DfoLogin
	{
		private static string DefaultLoginUrl { get { return "http://passport.nexon.net/Login.aspx?nexonTheme=DungeonFighter"; } }
		private static string DefaultGeolocationUrl { get { return "http://dungeonfighter.nexon.net/modules/geoloc.aspx"; } }
		private static string DefaultIni { get { return "http://download2.nexon.net/Game/DFO/ngm/DFOLauncher/version.ini"; } }
		private static string DefaultUserAgent { get { return "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 3.5.30729)"; } }

		/// <summary>
		/// The default timeout in milliseonds when waiting for a server response.
		/// </summary>
		public static int DefaultTimeoutInMillis { get { return 10000; } } // 10 seconds
		private static string DefaultUsernameField { get { return "txtId"; } }
		private static string DefaultPasswordField { get { return "txtPassword"; } }
		private static string DefaultButtonField { get { return "btnLogin"; } }
		private static string FallbackViewstate { get { return "/wEPDwUKMTY2MTY3MjU1M2Rk"; } }

		/// <summary>
		/// Returns one or more command-line arguments you can pass to DFOLauncher.exe to start the game
		/// using the given username and password.
		/// Because a connection to the web site is needed for authentication, this function blocks.
		/// </summary>
		/// 
		/// <param name="username">Username to log in as</param>
		/// <param name="password">Password to log in with</param>
		/// <param name="timeoutInMs">Timeout in milliseconds to wait for a server response.</param>
		/// 
		/// <exception cref="System.ArgumentNullException">username or password is null</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is negative.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the DFO
		/// DFO website.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns>One or more arguments you can pass to DFOLauncher.exe to start the game as the given user.
		/// This string can be directly used with Process.StartInfo.Arguments.</returns>
		public static string GetDfoArg( string username, string password, int timeoutInMs )
		{
			// Unlike most sensible programs, DFO does not use CommandLineToArgvW/CommandLineToArgvA
			// to parse its command-line into separate arguments. Instead it seems to simply separate
			// the arguments by a space and double quotes are normal characters.
			Logging.Log.Debug( "Getting argument string to pass to DFO." );
			IList<string> dfoArgs = GetDfoArgs( username, password, timeoutInMs );
			StringBuilder argString = new StringBuilder();
			for ( int argIndex = 0; argIndex < dfoArgs.Count; argIndex++ )
			{
				argString.Append( dfoArgs[ argIndex ] );
				if ( argIndex != dfoArgs.Count - 1 )
				{
					argString.Append( ' ' );
				}
			}

			Logging.Log.DebugFormat( "Argument string is {0}",
				argString.ToString().HideSensitiveData( SensitiveData.LoginCookies ) );

			return argString.ToString();
		}

		/// <summary>
		/// Returns one or more command-line arguments you can pass to DFOLauncher.exe to start the game
		/// using the given username and password.
		/// Because a connection to the web site is needed for authentication, this function blocks.
		/// </summary>
		/// 
		/// <param name="username">Username to log in as</param>
		/// <param name="password">Password to log in with</param>
		/// <param name="timeoutInMs">Timeout in milliseconds to wait for a server response.</param>
		/// 
		/// <exception cref="System.ArgumentNullException">username or password is null</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is negative.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the DFO
		/// DFO website.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred or there was an error while
		/// doing a web request.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns>One or more arguments you can pass to DFOLauncher.exe to start the game as the given user.</returns>
		public static IList<string> GetDfoArgs( string username, string password, int timeoutInMs )
		{
			Logging.Log.DebugFormat( "Getting arguments to pass to DFO." );
			List<string> args = new List<string>();

			string authArg = GetDfoAuthArg( username, password, DefaultLoginUrl, timeoutInMs, DefaultIni );
			args.Add( authArg );

			try
			{
				string geolocationArg = GetGeolocationArg( DefaultGeolocationUrl, timeoutInMs );
				args.Add( geolocationArg );
			}
			catch ( WebException ex )
			{
				// Couldn't get geolocation arg, maybe DFO changed and doesn't use it anymore
				// Don't break the app in that case.
				Logging.Log.ErrorFormat( "Couldn't get the geolocation argument: {0} Proceeding without geolocation information.", ex.Message );
				Logging.Log.Debug( "Exception details: {0}", ex );
			}
			catch ( GeolocationFormatException ex )
			{
				Logging.Log.ErrorFormat( "Couldn't get the geolocation argument because the data from the server was bad. Proceeding without geolocation information." );
				Logging.Log.DebugFormat( "Exception details: {0}", ex );
				Logging.Log.DebugFormat( "Geolocation page contents: {0}", ex.GeolocationData );
			}

			if ( Logging.Log.IsDebugEnabled )
			{
				StringBuilder argString = new StringBuilder();
				for ( int argIndex = 0; argIndex < args.Count; argIndex++ )
				{
					argString.Append( args[ argIndex ] );
					if ( argIndex != args.Count - 1 )
					{
						argString.Append( ' ' );
					}
				}

				Logging.Log.DebugFormat( "Arguments to pass to DFO are: {0}", argString.ToString() );
			}

			return args;
		}

		/// <summary>
		/// Returns a command-line argument you can pass to DFOLauncher.exe to start the game using the given
		/// username and password. Because a connection to the web site is needed for authentication,
		/// this function blocks.
		/// 
		/// The argument's format is: {authentication_token}?{ini} where {authentication_token} is the value of
		/// the cookie called NPP that the server gives you upon logging in and {ini} is the URL of an ini
		/// used for ???, eg http://download2.nexon.net/Game/DFO/ngm/DFOLauncher/version.ini. 
		/// </summary>
		/// 
		/// <param name="username">Username to log in as</param>
		/// <param name="password">Password to log in with</param>
		/// <param name="loginUrl">The DFO login URL</param>
		/// <param name="timeoutInMs">Timeout in milliseconds. If this time expires while waiting for a
		/// response from the server, a System.Net.WebException will be thrown.</param>
		/// <param name="ini">Name of the ini URL to use in forming the command-line argument, see summary.</param>
		/// 
		/// <exception cref="System.UriFormatException">The URI specified is not a valid URI.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to
		/// connect to the requested URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.ArgumentNullException">loginUrl, username, password, or ini is null.</exception>
		/// <exception cref="System.NotSupportedException">The request scheme specified in loginUrl is not http.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is negative.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns></returns>
		private static string GetDfoAuthArg( string username, string password, string loginUrl, int timeoutInMs, string ini )
		{
			ini.ThrowIfNull( "ini" );
			return GetAuthToken( username, password, loginUrl, timeoutInMs ) + "?" + ini;
		}

		/// <summary>
		/// Returns an authentication token string for the given username using the given password. Because
		/// a connection to the web site is needed for authentication, this function blocks.
		/// </summary>
		/// 
		/// <param name="username">Username to log in as</param>
		/// <param name="password">Password to log in with</param>
		/// <param name="loginUrl">The DFO login URL</param>
		/// <param name="timeoutInMs">Timeout in milliseconds. If this time expires while waiting for a
		/// response from the server, a System.Net.WebException will be thrown.</param>
		/// 
		/// <exception cref="System.UriFormatException">The URI specified is not a valid URI.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to
		/// connect to the requested URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.ArgumentNullException">loginUrl, username, or password is null</exception>
		/// <exception cref="System.NotSupportedException">The request scheme specified in loginUrl is not http.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is negative.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns>An authentication token string for the given username</returns>
		private static string GetAuthToken( string username, string password, string loginUrl, int timeoutInMs )
		{
			Logging.Log.DebugFormat( "Getting DFO authentication token using username '{0}', password '{1}', URL {2}, timeout of {3} ms.",
				username.HideSensitiveData( SensitiveData.Usernames ),
				password.HideSensitiveData( SensitiveData.Passwords ),
				loginUrl, timeoutInMs );

			username.ThrowIfNull( "username" );
			password.ThrowIfNull( "password" );

			// We need to make a POST request to the login URL containing the same parameters it would have
			// if you were sitting in front of your web browser.
			WebRequest loginPostRequestParentClass = WebRequest.Create( loginUrl );

			HttpWebRequest loginPostRequest = loginPostRequestParentClass as HttpWebRequest;
			if ( loginPostRequest == null )
			{
				throw new NotSupportedException( "The DFO login URL should be http" );
			}

			loginPostRequest.Method = "POST";
			loginPostRequest.ContentType = "application/x-www-form-urlencoded";
			loginPostRequest.UserAgent = DefaultUserAgent; // Probably not needed but we may as well act like a browser
			loginPostRequest.Timeout = timeoutInMs;
			loginPostRequest.ReadWriteTimeout = timeoutInMs; // Dunno if this is relevant
			loginPostRequest.KeepAlive = false; // We don't need to bother keeping the connection alive
			loginPostRequest.CookieContainer = new CookieContainer(); // Necessary for the HttpWebResponse object to contain cookies set in the response. Otherwise we'd have to manually parse the set-cookie headers

			StringBuilder postData = new StringBuilder(); // Used for building up the login form data

			string viewstate; // ASP.NET doesn't like it when you don't give it a viewstate
			try
			{
				viewstate = GetViewstate( loginUrl, timeoutInMs ); // Getting a fresh viewstate may be more likely to succeed. Using a hardcoded viewstate would probably work, but the time taken for an extra connection is not a concern here.
			}
			catch ( DfoAuthenticationException )
			{
				Logging.Log.WarnFormat( "Could not get a viewstate from {0}. Trying a fallback viewstate '{1}'.",
					loginUrl, FallbackViewstate );
				viewstate = FallbackViewstate;
			}

			postData.Append( "__VIEWSTATE=" ).Append( viewstate ).Append( "&" );
			postData.Append( DefaultUsernameField ).Append( "=" ).Append( HttpUtility.UrlEncode( username ) ).Append( "&" );
			postData.Append( DefaultPasswordField ).Append( "=" ).Append( HttpUtility.UrlEncode( password ) ).Append( "&" );
			postData.Append( DefaultButtonField ).Append( "=" ).Append( "&" );
			postData.Append( "__EVENTTARGET=&__EVENTARGUMENT=" );

			Logging.Log.DebugFormat( "POST data is '{0}'",
				postData.ToString().HideSensitiveData( SensitiveData.Usernames | SensitiveData.Passwords ) );

			// Not sure if MemoryStreams and StreamWriters need to be Disposed() of, but they do implement IDisposable even if it's only because of a parent class
			using ( MemoryStream postBytes = new MemoryStream() ) // Now we need to convert the post data string to raw bytes, because we need to set the content-length to the number of bytes, not number of chars.
			using ( StreamWriter postBytesWriter = new StreamWriter( postBytes, Encoding.UTF8 ) ) // I *think* UTF-8 is the right encoding to use here
			{
				postBytesWriter.Write( postData.ToString() );
				postBytesWriter.Flush();

				loginPostRequest.ContentLength = postBytes.Length;
				using ( Stream postDataStream = loginPostRequest.GetRequestStream() )
				{
					postDataStream.Write( postBytes.ToArray(), 0, (int)postBytes.Length );
					postDataStream.Flush();
				}
			}

			Logging.Log.DebugFormat( "Request written." );

			using ( WebResponse responseParentClass = loginPostRequest.GetResponse() )
			{
				Logging.Log.DebugFormat( "Got response." );
				HttpWebResponse response = responseParentClass as HttpWebResponse;
				if ( response == null )
				{
					throw new DfoAuthenticationException( "Response was not an http response" );
				}

				Cookie authTokenCookie = response.Cookies[ "NPP" ];
				if ( authTokenCookie == null )
				{
					throw new DfoAuthenticationException( "NPP cookie not present. Either username/password was not correct or the DFO website has made a change that breaks this program. If you are certain the username/password provided is correct by logging in using the website, report this as a bug." );
				}

				string authToken = authTokenCookie.Value;
				Logging.Log.DebugFormat( "Got authentication token: '{0}'",
					authToken.HideSensitiveData( SensitiveData.LoginCookies ) );
				return authToken;
			}
		}

		/// <summary>
		/// Gets a valid __VIEWSTATE to use when sending form values.
		/// </summary>
		/// <param name="loginUrl"></param>
		/// <param name="timeoutInMillis"></param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">The URL scheme is not http.</exception>
		/// <exception cref="System.UriFormatException">The URI specified is not a valid URI.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to
		/// connect to the requested URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="url"/> is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is
		/// less than or equal to zero and not System.Threading.Timeout.Infinite.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred or there was an error while
		/// doing the web request.</exception>
		/// <exception cref="Dfo.Controlling.DfoAuthenticationException">Could not get the viewstate possibly due to a change
		/// in the web site.</exception>
		private static string GetViewstate( string loginUrl, int timeoutInMillis )
		{
			// Here we're sending a GET request to the login URL for the sole purpose of getting the value
			// of the hidden form field called __VIEWSTATE
			Logging.Log.DebugFormat( "Getting viewstate from {0}", loginUrl );
			string html = GetWebPage( loginUrl, timeoutInMillis );

			string viewstateRegexString = "<input type=\"hidden\" name=\"__VIEWSTATE\" id=\"__VIEWSTATE\" value=\"(?<viewstate>.*?)\"";
			Regex viewstateRegex = new Regex( viewstateRegexString, RegexOptions.IgnoreCase );

			Match match = viewstateRegex.Match( html );
			if ( !match.Success )
			{
				throw new DfoAuthenticationException( "Could not extract the viewstate. This is an error in the program caused by the DFO website changing. Please report this as a bug." );
			}
			else
			{
				string viewstate = match.Groups[ "viewstate" ].Value;
				Logging.Log.DebugFormat( "Got viewstate: {0}", viewstate );
				return viewstate;
			}
		}

		/// <summary>
		/// Gets the web page at the given URL.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="timeoutInMs"></param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">The URL scheme is not http.</exception>
		/// <exception cref="System.UriFormatException">The URI specified is not a valid URI.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to
		/// connect to the requested URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="url"/> is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is
		/// less than or equal to zero and not System.Threading.Timeout.Infinite.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred or there was an error while
		/// doing the web request (including 404 not found).</exception>
		private static string GetWebPage( string url, int timeoutInMs )
		{
			Logging.Log.DebugFormat( "Getting web page at {0}", url );

			WebRequest getRequestParentClass = WebRequest.Create( url );
			HttpWebRequest getRequest = getRequestParentClass as HttpWebRequest;
			if ( getRequest == null )
			{
				throw new NotSupportedException( "The URL should be http." );
			}

			getRequest.Method = "GET";
			getRequest.UserAgent = DefaultUserAgent;
			getRequest.Timeout = timeoutInMs;
			getRequest.ReadWriteTimeout = timeoutInMs;
			getRequest.KeepAlive = false;

			string html;

			using ( WebResponse responseParentClass = getRequest.GetResponse() )
			{
				Logging.Log.DebugFormat( "Got response." );

				HttpWebResponse response = responseParentClass as HttpWebResponse;
				if ( response == null )
				{
					throw new DfoAuthenticationException( "Response was not an http response." );
				}

				using ( Stream responseBodyStream = response.GetResponseStream() )
				using ( StreamReader responseBodyReader = new StreamReader( responseBodyStream, Encoding.UTF8 ) )
				{
					// XXX: Shouldn't be hardcoding UTF-8...but how to get proper encoding?
					html = responseBodyReader.ReadToEnd();
				}
			}
			Logging.Log.Trace( html );
			return html;
		}

		/// <summary>
		/// Gets the geolocation argument that should be passed to DFO.
		/// </summary>
		/// <param name="geolocationUrl"></param>
		/// <param name="timeoutInMs"></param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">The URL scheme is not http.</exception>
		/// <exception cref="System.UriFormatException">The URI specified is not a valid URI.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to
		/// connect to the requested URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="geolocationUrl"/> is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is
		/// less than or equal to zero and not System.Threading.Timeout.Infinite.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred or there was an error while
		/// doing the web request.</exception>
		/// <exception cref="Dfo.Controlling.GeolocationFormatException">Could not get the geolocation arg because
		/// of getting bad data from the other site.</exception>
		private static string GetGeolocationArg( string geolocationUrl, int timeoutInMs )
		{
			Logging.Log.DebugFormat( "Getting geolocation argument to pass to DFO from {0}", geolocationUrl );

			string json = GetWebPage( geolocationUrl, timeoutInMs );

			Dictionary<string, string> geoKeyValuePairs;
			try
			{
				geoKeyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
			}
			catch ( Exception ex )
			{
				if ( ex is JsonReaderException || ex is JsonSerializationException )
				{
					throw new GeolocationFormatException( json, "Badly formatted geolocation page." );
				}
				else
				{
					throw;
				}
			}

			if ( geoKeyValuePairs.ContainsKey( "code" ) )
			{
				string geolocationArg = geoKeyValuePairs[ "code" ];
				Logging.Log.DebugFormat( "Geolocation arg is '{0}'", geolocationArg );
				return geoKeyValuePairs[ "code" ];
			}
			else
			{
				throw new GeolocationFormatException( json, "'code' not present in geolocation page." );
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
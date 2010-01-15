using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Dfo.Controlling
{
	public static class DfoLogin
	{
		public static string DefaultLoginUrl { get { return "http://passport.nexon.net/Login.aspx?nexonTheme=DungeonFighter"; } }
		public static string DefaultIni { get { return "http://download2.nexon.net/Game/DFO/ngm/DFOLauncher/version.ini"; } }
		public static string DefaultUserAgent { get { return "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.3) Gecko/20090824 Firefox/3.5.3 (.NET CLR 3.5.30729)"; } }
		public static int DefaultTimeoutInMillis { get { return 20000; } } // 20 seconds
		public static string DefaultUsernameField { get { return "txtId"; } }
		public static string DefaultPasswordField { get { return "txtPassword"; } }
		public static string DefaultButtonField { get { return "btnLogin"; } }
		public static string FallbackViewstate { get { return "/wEPDwUKMTY2MTY3MjU1M2Rk"; } }

		/// <summary>
		/// Returns a command-line argument you can pass to DFOLauncher.exe to start the game using the given
		/// username and password and using defaults for advanced parameters like connection timeout.
		/// Because a connection to the web site is needed for authentication, this function blocks.
		/// </summary>
		/// 
		/// <param name="username">Username to log in as</param>
		/// <param name="password">Password to log in with</param>
		/// 
		/// <exception cref="System.ArgumentNullException">username or password is null</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is negative.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the DFO
		/// URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="DfoLogin.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns>A command-line argument you can pass to DFOLauncher.exe to start the game as the given user</returns>
		public static string GetDfoArg( string username, string password, int timeoutInMs )
		{
			return GetDfoArg( username, password, DefaultLoginUrl, timeoutInMs, DefaultIni );
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
		/// <exception cref="DfoLogin.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns></returns>
		public static string GetDfoArg( string username, string password, string loginUrl, int timeoutInMs, string ini )
		{
			if ( ini == null )
			{
				throw new ArgumentNullException( "ini" );
			}
			return GetAuthToken( username, password, loginUrl, timeoutInMs ) + "?" + ini;
		}

		/// <summary>
		/// Returns an authentication token string for the given username using the given password, using
		/// defaults for advanced parameters like connection timeout. Because a connection to the web site
		/// is needed for authentication, this function blocks.
		/// </summary>
		/// 
		/// <param name="username">Username to log in as.</param>
		/// <param name="password">Password to log in with.</param>
		/// <param name="timeoutInMs">Timeout in milliseconds. If this time expires while waiting for a
		/// response from the server, a System.Net.WebException will be thrown.</param>
		/// 
		/// <exception cref="System.Security.SecurityException">The caller does not have permission to
		/// connect to the DFO URI or a URI that the request is redirected to.</exception>
		/// <exception cref="System.ArgumentNullException">username or password is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="timeoutInMs"/> is negative.</exception>
		/// <exception cref="System.Net.WebException">A timeout occurred.</exception>
		/// <exception cref="DfoLogin.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns>An authentication token string for the given username</returns>
		public static string GetAuthToken( string username, string password, int timeoutInMs )
		{
			return GetAuthToken( username, password, DefaultLoginUrl, timeoutInMs );
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
		/// <exception cref="DfoLogin.DfoAuthenticationException">Either the username/password is incorrect
		/// or a change was made to the way the authentication token is given to the browser, in which case
		/// this function will not work.</exception>
		/// 
		/// <returns>An authentication token string for the given username</returns>
		public static string GetAuthToken( string username, string password, string loginUrl, int timeoutInMs )
		{
			if ( username == null )
			{
				throw new ArgumentNullException( "username" );
			}
			if ( password == null )
			{
				throw new ArgumentNullException( "password" );
			}

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
				viewstate = FallbackViewstate; // Couldn't parse the viewstate, try an already obtained viewstate.
			}

			postData.Append( "__VIEWSTATE=" ).Append( viewstate ).Append( "&" );
			postData.Append( DefaultUsernameField ).Append( "=" ).Append( HttpUtility.UrlEncode( username ) ).Append( "&" );
			postData.Append( DefaultPasswordField ).Append( "=" ).Append( HttpUtility.UrlEncode( password ) ).Append( "&" );
			postData.Append( DefaultButtonField ).Append( "=" ).Append( "&" );
			postData.Append( "__EVENTTARGET=&__EVENTARGUMENT=" );

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

			using ( WebResponse responseParentClass = loginPostRequest.GetResponse() )
			{
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
				return authToken;
			}
		}

		/// <summary>
		/// Gets a valid __VIEWSTATE to use when sending form values.
		/// </summary>
		/// <param name="loginUrl"></param>
		/// <param name="timeoutInMillis"></param>
		/// <returns></returns>
		private static string GetViewstate( string loginUrl, int timeoutInMillis )
		{
			// Here we're sending a GET request to the login URL for the sole purpose of getting the value
			// of the hidden form field called __VIEWSTATE
			WebRequest loginGetRequestParentClass = WebRequest.Create( loginUrl );
			HttpWebRequest loginGetRequest = loginGetRequestParentClass as HttpWebRequest;
			if ( loginGetRequest == null )
			{
				throw new NotSupportedException( "The DFO login URL should be http" );
			}

			loginGetRequest.Method = "GET";
			loginGetRequest.UserAgent = DefaultUserAgent;
			loginGetRequest.Timeout = timeoutInMillis;
			loginGetRequest.ReadWriteTimeout = timeoutInMillis;
			loginGetRequest.KeepAlive = false;

			string html;

			using ( WebResponse responseParentClass = loginGetRequest.GetResponse() )
			{
				HttpWebResponse response = responseParentClass as HttpWebResponse;
				if ( response == null )
				{
					throw new DfoAuthenticationException( "Response was not an http response" );
				}

				using ( Stream responseBodyStream = response.GetResponseStream() )
				using ( StreamReader responseBodyReader = new StreamReader( responseBodyStream, Encoding.UTF8 ) )
				{
					html = responseBodyReader.ReadToEnd();
				}
			}

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
				return viewstate;
			}
		}

		/// <summary>
		/// Starts DFO using the given username and password. The executable should be in the DFO directory.
		/// Because a connection to the web site is needed for authentication, this function blocks.
		/// </summary>
		/// 
		/// <param name="username">Username to log in as</param>
		/// <param name="password">Password to log in with</param>
		/// 
		/// <exception cref="DfoLogin.DfoLaunchException">There was an error while attempting to authenticate
		/// or start DFO. The Message property contains an already user-friendly error message.</exception>
		/// <exception cref="System.ArgumentNullException">username or password is null.</exception>
		public static void StartDfo( string username, string password )
		{
			string dfoLauncherPath = ""; // assignment to shut the compiler up. I know that if Win32Exception is thrown, this has been set.
			try
			{
				string dfoArg = GetDfoArg( username, password, DfoLogin.DefaultTimeoutInMillis );
				Process dfo = new Process();
				dfoLauncherPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "DFOLauncher.exe" );
				dfo.StartInfo.FileName = dfoLauncherPath;
				dfo.StartInfo.Arguments = dfoArg;
				dfo.StartInfo.UseShellExecute = true;
				dfo.StartInfo.ErrorDialog = true; // pop up that very helpful message box when DFOLaucher.exe can't be run
				dfo.Start();
				dfo.Dispose(); // This doesn't kill the process. What it does is release the OS handle to the process since we don't need it.
			}
			catch ( WebException ex )
			{
				throw new DfoLaunchException( string.Format(
					"There was a problem connecting. Check your Internet connection. Details: {0}", ex.Message ), ex );
			}
			catch ( DfoAuthenticationException ex )
			{
				throw new DfoLaunchException( string.Format(
					"Error while authenticating: {0}", ex.Message ), ex );
			}
			catch ( Win32Exception ex )
			{
				throw new DfoLaunchException( string.Format(
					"Error while starting DFO using {0}: {1} (Did you forget to put this program in the DFO directory?)",
					dfoLauncherPath, ex.Message ), ex );
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
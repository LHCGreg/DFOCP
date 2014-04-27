using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dfo.Controlling
{
	public static class Utilities
	{
		public static void ThrowIfNull( this object obj, string argName )
		{
			if ( obj == null )
			{
				throw new ArgumentNullException( argName );
			}
		}

		/// <summary>
		/// Resolves a path to an absolute path if is a relative path.
		/// </summary>
		/// <param name="path">Path to resolve.</param>
		/// <param name="relativeRoot">Path that <paramref name="path"/> is relative to if it is relative.</param>
		/// <returns>The resolved path if it was relative or the same path if it was absolute.</returns>
		/// <exception cref="System.ArgumentException">One of the paths contains invalid characters.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="path"/> or
		/// <paramref name="relativeRoot"/> is null.</exception>
		public static string ResolvePossiblyRelativePath( string path, string relativeRoot )
		{
			path.ThrowIfNull( "path" );
			relativeRoot.ThrowIfNull( "relativeRoot" );

			if ( Path.IsPathRooted( path ) )
			{
				return path;
			}
			else
			{
				return Path.Combine( relativeRoot, path );
			}
		}

		/// <summary>
		/// Returns true if a file or directory exists at the given path, false otherwise.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool FileOrDirectoryExists( string path )
		{
			return File.Exists( path ) || Directory.Exists( path );
		}

		/// <summary>
		/// Determines if a string is a valid path.
		/// </summary>
		/// <param name="path">A path.</param>
		/// <returns>True if <paramref name="path"/> is not null and has no invalid characters.</returns>
		public static bool PathIsValid( string path )
		{
			if ( path == null )
			{
				return false;
			}
			char[] invalidChars = Path.GetInvalidPathChars();
			if ( path.IndexOfAny( invalidChars ) != -1 )
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		// Taken from a comment on
		// http://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.arguments%28VS.90%29.aspx
		/// <summary>
		/// Translates a number of string command-line arguments into a single string that can be passed to
		/// Process.StartInfo.Arguments. Windows only. Note that Windows programs are responsible for picking
		/// out the arguments from one argument string. If you specify "-o output.txt     ", that's exactly
		/// what the program sees. It's up to the program to break that into the args "-o" and "output.txt".
		/// Most sensible programs will use the Windows function CommandLineToArgvW/CommandLineToArgvA.
		/// 
		/// DFO is not a sensible program.
		/// 
		/// The rules that CommandLineToArgvW uses involving quoting and backslashes are quite odd.
		/// A backslash has no special meaning unless it is part of a series of backslashes before a
		/// double quote. In that case, 2 backslashes translates to 1 backslash and a backslash followed by
		/// a double quote translates into a literal double quote (instead of a quote enclosing an argument).
		/// 
		/// The string returned by this method quotes the arguments in a form suitable for
		/// CommandLineToArgvW.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string ArgvToCommandLine( IEnumerable<string> args )
		{
			StringBuilder sb = new StringBuilder();
			foreach ( string s in args )
			{
				sb.Append( '"' );
				// Escape double quotes (") and backslashes (\).
				int searchIndex = 0;
				while ( true )
				{
					// Put this test first to support zero length strings.
					if ( searchIndex >= s.Length )
					{
						break;
					}
					int quoteIndex = s.IndexOf( '"', searchIndex );
					if ( quoteIndex < 0 )
					{
						break;
					}
					sb.Append( s, searchIndex, quoteIndex - searchIndex );
					EscapeBackslashes( sb, s, quoteIndex - 1 );
					sb.Append( '\\' );
					sb.Append( '"' );
					searchIndex = quoteIndex + 1;
				}
				sb.Append( s, searchIndex, s.Length - searchIndex );
				EscapeBackslashes( sb, s, s.Length - 1 );
				sb.Append( @""" " );
			}
			return sb.ToString( 0, Math.Max( 0, sb.Length - 1 ) );
		}
		private static void EscapeBackslashes( StringBuilder sb, string s, int lastSearchIndex )
		{
			// Backslashes must be escaped if and only if they precede a double quote.
			for ( int i = lastSearchIndex; i >= 0; i-- )
			{
				if ( s[ i ] != '\\' )
				{
					break;
				}
				sb.Append( '\\' );
			}
		}
	}
}

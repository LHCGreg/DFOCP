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
	}
}

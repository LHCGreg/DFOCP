using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dfo.Controlling
{
	/// <summary>
	/// A class for temporarily switching two files or directories.
	/// </summary>
	public class FileSwitcher
	{
		/// <summary>
		/// The absolute path of the file to switch out.
		/// </summary>
		public string NormalFile { get; set; }
		
		/// <summary>
		/// The absolute path of the file to switch in.
		/// </summary>
		public string CustomFile { get; set; }
		
		/// <summary>
		/// The absolute path of the file of use as a temporary location for the file being switched out.
		/// </summary>
		public string TempFile { get; set; }

		public FileSwitcher()
		{
			;
		}

		/// <summary>
		/// Moves FileToSwitch to TempFile and FileToSwitchWith to FileToSwitch. If the second move fails,
		/// the first move is attempted to be undone.
		/// </summary>
		/// <returns>A SwitchedFile object you can use to switch back by calling Dispose() on it.</returns>
		/// <exception cref="System.ArgumentNullExcepton">One of the properties is null.</exception>
		/// <exception cref="System.IO.IOException">The file could not be switched.</exception>
		public SwitchedFile Switch()
		{
			NormalFile.ThrowIfNull( "FileToSwitch" );
			CustomFile.ThrowIfNull( "FileToSwitchWith" );
			TempFile.ThrowIfNull( "TempFile" );

			bool firstMoveSuccessful = false;
			try
			{
				File.Move( NormalFile, TempFile );
				firstMoveSuccessful = true;

				File.Move( CustomFile, NormalFile );

				return new SwitchedFile( NormalFile, CustomFile, TempFile );
			}
			catch ( Exception ex )
			{
				// If the first move was successful, we can probably move it back
				bool undoSuccess = false;
				Exception undoError = null;
				if ( firstMoveSuccessful )
				{
					try
					{
						File.Move( TempFile, NormalFile );
						undoSuccess = true;
					}
					catch ( Exception ex2 )
					{
						undoError = ex2;
					}
				}

				if ( !firstMoveSuccessful )
				{
					throw new IOException( string.Format(
						"Could not move {0} to {1}. {2}",
						NormalFile, TempFile, ex.Message ), ex );
				}
				else
				{
					string undoMessage;
					if ( undoSuccess )
					{
						undoMessage = string.Format(
						"{0} moved back to {1}.",
						TempFile, NormalFile );
					}
					else
					{
						undoMessage = string.Format(
						"{0} could not be moved back to {1}. {2} Files are in an inconsistent state!",
						TempFile, NormalFile, undoError.Message );
					}

					throw new IOException( string.Format(
						"Could not move {0} to {1}. {2} {3}",
						CustomFile, NormalFile, ex.Message, undoMessage ), ex );
				}
			}
		}

		/// <summary>
		/// Checks if the files of a switchable file (normal and custom) are not where they should be
		/// (perhaps because of a crash). This only checks if it is something that can be fixed by
		/// <c>FixBrokenSwitchableFiles()</c>. For example, a completely missing soundpack directory with no
		/// temp soundpack directory is not considered to be "broken". Note that if the files are currently
		/// switched, this method will report them as "broken" even if they will be switched back when the game
		/// ends.
		/// </summary>
		/// <returns>True if the files are in an abnormal state that can be fixed by
		/// <c>FixBrokenSwitchableFiles()</c>.</returns>
		public bool FilesBroken()
		{
			NormalFile.ThrowIfNull( "FileToSwitch" );
			CustomFile.ThrowIfNull( "FileToSwitchWith" );
			TempFile.ThrowIfNull( "TempFile" );

			if ( Utilities.FileOrDirectoryExists( TempFile ) && Utilities.FileOrDirectoryExists( NormalFile ) )
			{
				return true;
			}
			else if ( Utilities.FileOrDirectoryExists( TempFile ) && Utilities.FileOrDirectoryExists( CustomFile ) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Fixes switchable file mixups usually caused by a system or DFOCP crash. Does nothing if no problems
		/// are detected. Note if the game is running with switched files, this method will try to "fix" them
		/// and fail.
		/// </summary>
		/// <exception cref="System.IO.IOException">Something went wrong while trying to fix the mixup.</exception>
		public void FixBrokenFiles()
		{
			NormalFile.ThrowIfNull( "FileToSwitch" );
			CustomFile.ThrowIfNull( "FileToSwitchWith" );
			TempFile.ThrowIfNull( "TempFile" );

			if ( !FilesBroken() )
			{
				return;
			}

			if ( Utilities.FileOrDirectoryExists( TempFile ) && Utilities.FileOrDirectoryExists( NormalFile ) )
			{
				// Rename FileToSwitch to FileToSwitchWith
				try
				{
					File.Move( NormalFile, CustomFile );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format( "Could not move {0} to {1}. {2}",
						NormalFile, CustomFile, ex.Message ) );
				}

				// Rename TempFile to FileToSwitch
				try
				{
					File.Move( TempFile, NormalFile );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format( "Could not move {0} to {1}. {2}",
						TempFile, NormalFile, ex.Message ) );
				}
			}
			else if ( Utilities.FileOrDirectoryExists( TempFile ) && Utilities.FileOrDirectoryExists( CustomFile ) )
			{
				// Rename TempFile to FileToSwitch
				try
				{
					File.Move( TempFile, NormalFile );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format( "Could not move {0} to {1}. {2}",
						TempFile, NormalFile, ex.Message ) );
				}
			}
			else
			{
				return;
			}
		}
	}
}

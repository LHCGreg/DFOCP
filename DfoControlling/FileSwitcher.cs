using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dfo.Controlling
{
	/// <summary>
	/// Represents a kind of file system entity (regular file, directory).
	/// </summary>
	public enum FileType
	{
		/// <summary>
		/// A normal file.
		/// </summary>
		RegularFile,

		/// <summary>
		/// A directory.
		/// </summary>
		Directory,
	}

	public static class FileTypeExtensions
	{
		/// <summary>
		/// Gets either File.Move or Directory.Move depending on fileType.
		/// </summary>
		/// <param name="fileType"></param>
		/// <returns></returns>
		public static Action<string, string> GetMoveFunction( this FileType fileType )
		{
			switch ( fileType )
			{
				case FileType.RegularFile:
					return File.Move;
				case FileType.Directory:
					return Directory.Move;
				default:
					throw new Exception( "Oops, missed a file type." );
			}
		}

		/// <summary>
		/// Gets either File.Exists or Directory.Exists depending on fileType.
		/// </summary>
		/// <param name="fileType"></param>
		/// <returns></returns>
		public static Func<string, bool> GetExistsFunction( this FileType fileType )
		{
			switch ( fileType )
			{
				case FileType.RegularFile:
					return File.Exists;
				case FileType.Directory:
					return Directory.Exists;
				default:
					throw new Exception( "Oops, missed a file type." );
			}
		}
	}

	/// <summary>
	/// A class for temporarily switching two files or directories.
	/// </summary>
	public class FileSwitcher
	{
		/// <summary>
		/// Gets or sets the absolute path of the file to switch out.
		/// </summary>
		public string NormalFile { get; set; }

		/// <summary>
		/// Gets or sets the absolute path of the file to switch in.
		/// </summary>
		public string CustomFile { get; set; }

		/// <summary>
		/// Gets or sets the absolute path of the file of use as a temporary location for the file being switched out.
		/// </summary>
		public string TempFile { get; set; }

		private FileType m_fileType = FileType.RegularFile;
		/// <summary>
		/// Gets or sets what kind of file system entity the file is supposed to be (file, directory).
		/// </summary>
		public FileType FileType { get { return m_fileType; } set { m_fileType = value; } }

		/// <summary>
		/// Creates a FileSwitcher object. You must set values for NormalFile, CustomFile, TempFile, and
		/// FileType.
		/// </summary>
		public FileSwitcher()
		{
			;
		}

		/// <summary>
		/// Creates a copy of this object.
		/// </summary>
		/// <returns></returns>
		public FileSwitcher Clone()
		{
			FileSwitcher clone = new FileSwitcher();
			clone.NormalFile = this.NormalFile;
			clone.CustomFile = this.CustomFile;
			clone.TempFile = this.TempFile;
			clone.FileType = this.FileType;

			return clone;
		}

		/// <summary>
		/// Moves FileToSwitch to TempFile and FileToSwitchWith to FileToSwitch. If the second move fails,
		/// the first move is attempted to be undone.
		/// </summary>
		/// <returns>A SwitchedFile object you can use to switch back by calling SwitchBack() on it.</returns>
		/// <exception cref="System.ArgumentNullExcepton">One of the properties is null.</exception>
		/// <exception cref="System.IO.IOException">The file could not be switched.</exception>
		public SwitchedFile Switch()
		{
			NormalFile.ThrowIfNull( "FileToSwitch" );
			CustomFile.ThrowIfNull( "FileToSwitchWith" );
			TempFile.ThrowIfNull( "TempFile" );

			Logging.Log.DebugFormat( "Switching {0} with {1} using {2} as a temporary.",
				NormalFile, CustomFile, TempFile );

			Action<string, string> move = FileType.GetMoveFunction();

			bool firstMoveSuccessful = false;
			try
			{
				move( NormalFile, TempFile );
				firstMoveSuccessful = true;
				Logging.Log.DebugFormat( "Moved {0} to {1}.", NormalFile, TempFile );

				move( CustomFile, NormalFile );
				Logging.Log.DebugFormat( "Moved {0} to {1}.", CustomFile, NormalFile );
				Logging.Log.DebugFormat( "Switch successful." );

				return new SwitchedFile( NormalFile, CustomFile, TempFile, FileType );
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
						move( TempFile, NormalFile );
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

		// The soundpack directories go through the following states:

		// normal soundpacks: soundpackDir
		// custom soundpacks: customSoundpackDir

		// Rename soundpackDir to tempSoundpackDir

		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: customSoundpackDir

		// Rename customSoundpackDir to soundpackDir

		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: soundpackDir

		// Game runs...
		// Game stops...

		// Rename soundpackDir to customSoundpackDir

		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: customSoundpackDir

		// Rename tempSoundpackDir to soundpackDir

		// normal soundpacks: soundpackDir
		// custom soundpacks: customSoundpackDir

		// That makes the abnormal states:
		//
		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: customSoundpackDir
		//
		// (this is the common (unfortunately) case of a crash while the game is running
		// normal soundpacks: tempSoundpackDir
		// custom soundpacks: soundpackDir
		
		/// <summary>
		/// Checks if the files of a switchable file (normal and custom) are not where they should be
		/// (perhaps because of a crash). This only checks if it is something that can be fixed by
		/// <c>FixBrokenFiles()</c>. For example, a completely missing soundpack directory with no
		/// temp soundpack directory is not considered to be "broken". Note that if the files are currently
		/// switched, this method will report them as "broken" even if they will be switched back when the game
		/// ends.
		/// </summary>
		/// <returns>True if the files are in an abnormal state that can be fixed by
		/// <c>FixBrokenFiles()</c>.</returns>
		public bool FilesBroken()
		{
			Logging.Log.DebugFormat( "Checking if switchable ('{0}', '{1}', '{2}') is broken.",
				NormalFile, CustomFile, TempFile );

			if ( NormalFile == null || CustomFile == null || TempFile == null )
			{
				Logging.Log.DebugFormat( "A file is not, so not \"broken\"." );
				return false;
			}

			Func<string, bool> exists = FileType.GetExistsFunction();

			if ( exists( TempFile ) && exists( NormalFile ) )
			{
				Logging.Log.DebugFormat( "Normal and temp both exist, switchable is broken." );
				return true;
			}
			else if ( exists( TempFile ) && exists( CustomFile ) )
			{
				Logging.Log.DebugFormat( "Custom and temp both exist, switchable is broken." );
				return true;
			}
			else
			{
				Logging.Log.DebugFormat( "Not broken." );
				return false;
			}
		}

		/// <summary>
		/// Fixes switchable file mixups usually caused by a system or DFOCP crash. Does nothing if no problems
		/// are detected. Note if the game is running with switched files, this method will try to "fix" them
		/// and fail.
		/// </summary>
		/// <exception cref="System.IO.IOException">Something went wrong while trying to fix the mixup.</exception>
		/// <exception cref="System.ArgumentNullException">One of the properties is null.</exception>
		public void FixBrokenFiles()
		{
			bool wereBroken;
			FixBrokenFiles( out wereBroken );
		}
		
		/// <summary>
		/// Fixes switchable file mixups usually caused by a system or DFOCP crash. Does nothing if no problems
		/// are detected. Note if the game is running with switched files, this method will try to "fix" them
		/// and fail.
		/// </summary>
		/// <param name="wereBroken">Set to true if the files were broken and were fixed. Set to false if
		/// the files were not broken.</param>
		/// <exception cref="System.IO.IOException">Something went wrong while trying to fix the mixup.</exception>
		/// <exception cref="System.ArgumentNullException">One of the properties is null.</exception>
		public void FixBrokenFiles(out bool wereBroken)
		{
			wereBroken = false;

			NormalFile.ThrowIfNull( "FileToSwitch" );
			CustomFile.ThrowIfNull( "FileToSwitchWith" );
			TempFile.ThrowIfNull( "TempFile" );

			Logging.Log.DebugFormat( "Repairing switchable ('{0}', '{1}', '{2}').",
				NormalFile, CustomFile, TempFile );

			Action<string, string> move = FileType.GetMoveFunction();
			Func<string, bool> exists = FileType.GetExistsFunction();

			if ( exists( TempFile ) && exists( NormalFile ) )
			{
				wereBroken = true;
				Logging.Log.DebugFormat( "Normal and temp exist." );
				
				// Rename FileToSwitch to FileToSwitchWith
				try
				{
					move( NormalFile, CustomFile );
					Logging.Log.DebugFormat( "Moved file at normal location {0} to custom location {1}",
						NormalFile, CustomFile );
				}
				catch ( Exception ex ) // XXX: Catch specific exceptions
				{
					throw new IOException( string.Format( "Could not move {0} to {1}. {2}",
						NormalFile, CustomFile, ex.Message ) );
				}

				// Rename TempFile to FileToSwitch
				try
				{
					move( TempFile, NormalFile );
					Logging.Log.DebugFormat( "Moved file at temp location {0} to normal location {1}",
						TempFile, NormalFile );
					Logging.Log.DebugFormat( "Repair complete." );
				}
				catch ( Exception ex ) // XXX: Catch specific exceptions
				{
					throw new IOException( string.Format( "Could not move {0} to {1}. {2}",
						TempFile, NormalFile, ex.Message ) );
				}
			}
			else if ( exists( TempFile ) && exists( CustomFile ) )
			{
				wereBroken = true;
				Logging.Log.DebugFormat( "Custom and temp exist." );

				// Rename TempFile to FileToSwitch
				try
				{
					move( TempFile, NormalFile );
					Logging.Log.DebugFormat( "Moved file at temp location {0} to normal location {1}",
						TempFile, NormalFile );
				}
				catch ( Exception ex )
				{
					throw new IOException( string.Format( "Could not move {0} to {1}. {2}",
						TempFile, NormalFile, ex.Message ) );
				}
			}
			else
			{
				Logging.Log.DebugFormat( "Not broken, nothing to repair." );
				return;
			}
		}
	}
}

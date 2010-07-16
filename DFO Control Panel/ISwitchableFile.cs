using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	/// <summary>
	/// Carries information about a switchable file.
	/// </summary>
	interface ISwitchableFile
	{
		/// <summary>
		/// The Name is used to uniquely identify a switchable file. It is used in saved settings and
		/// for the UI to know which switchable file objects to bind UI controls to.
		/// </summary>
		string Name { get; }
		
		/// <summary>
		/// Name of the command-line argument to use for this file. The negative command-line argument is
		/// "no{0}" where {0} is this property.
		/// </summary>
		string WhetherToSwitchArg { get; }
		
		/// <summary>
		/// Name of the command-line argument for specifying explicitly the custom file to switch with.
		/// </summary>
		string CustomFileArg { get; }
		
		/// <summary>
		/// Name of the command-line argument for specifying explicitly the temporary file to use when switching.
		/// </summary>
		string TempFileArg { get; }
		
		/// <summary>
		/// The default custom file to use if it is not explicitly specified.
		/// </summary>
		string DefaultCustomFile { get; }
		
		/// <summary>
		/// The default temp file to use if it is not explicitly specified.
		/// </summary>
		string DefaultTempFile { get; }
		
		/// <summary>
		/// Gets whether or not code using this switchable file should switch the file when launching the game.
		/// </summary>
		bool Switch { get; }
		
		/// <summary>
		/// Gets the (possibly relative) path of the file normally used by the game.
		/// </summary>
		string NormalFile { get; }
		
		/// <summary>
		/// Gets the (possibly relative) path of the custom file the user might want to switch in.
		/// </summary>
		string CustomFile { get; }
		
		/// <summary>
		/// Gets the (possibly relative) path of the file to use as a temporary when switching.
		/// </summary>
		string TempFile { get; }
		
		/// <summary>
		/// If NormalFile, CustomFile, or TempFile are relative, they are relative to this.
		/// </summary>
		string RelativeRoot { get; }
		
		/// <summary>
		/// Type of the file to switch (file, directory).
		/// </summary>
		FileType FileType { get; }
	}

	static class ISwitchableFileExtensions
	{
		/// <summary>
		/// Resolves NormalFile to an absolute path if it is a relative path using RelativeRoot.
		/// If it is already an absolute path, NormalFile is returned.
		/// </summary>
		/// <param name="switchableFile"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">NormalFile or RelativeRoot contain invalid characters.</exception>
		/// <exception cref="System.ArgumentNullException">NormalFile or RelativeRoot is null.</exception>
		public static string ResolveNormalFile( this ISwitchableFile switchableFile )
		{
			return Utilities.ResolvePossiblyRelativePath( switchableFile.NormalFile, switchableFile.RelativeRoot );
		}

		/// <summary>
		/// Resolves CustomFile to an absolute path if it is a relative path using RelativeRoot.
		/// If it is already an absolute path, CustomFile is returned.
		/// </summary>
		/// <param name="switchableFile"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">CustomFile or RelativeRoot contain invalid characters.</exception>
		/// <exception cref="System.ArgumentNullException">CustomFile or RelativeRoot is null.</exception>
		public static string ResolveCustomFile( this ISwitchableFile switchableFile )
		{
			return Utilities.ResolvePossiblyRelativePath( switchableFile.CustomFile, switchableFile.RelativeRoot );
		}

		/// <summary>
		/// Resolves TempFile to an absolute path if it is a relative path using RelativeRoot.
		/// If it is already an absolute path, TempFile is returned.
		/// </summary>
		/// <param name="switchableFile"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">TempFile or RelativeRoot contain invalid characters.</exception>
		/// <exception cref="System.ArgumentNullException">TempFile or RelativeRoot is null.</exception>
		public static string ResolveTempFile( this ISwitchableFile switchableFile )
		{
			return Utilities.ResolvePossiblyRelativePath( switchableFile.TempFile, switchableFile.RelativeRoot );
		}

		/// <summary>
		/// Fixes switchable files that are in an inconsistent state. If they are not in an inconsistent state,
		/// does nothing.
		/// </summary>
		/// <param name="switchableFile"></param>
		/// <exception cref="System.IO.IOException">There was an error while fixing the files.</exception>
		public static void FixBrokenFilesIfNeeded( this ISwitchableFile switchableFile )
		{
			bool wasBroken;
			FixBrokenFilesIfNeeded( switchableFile, out wasBroken );
		}

		/// <summary>
		/// Fixes switchable files that are in an inconsistent state. If they are not in an inconsistent state,
		/// does nothing.
		/// </summary>
		/// <param name="switchableFile"></param>
		/// <param name="wasBroken">Set to true if the files were in an inconsistent state.</param>
		/// <exception cref="System.IO.IOException">There was an error while fixing the files.</exception>
		/// <exception cref="System.ArgumentException">NormalFile, CustomFile, or TempFile contain invalid
		/// characters.</exception>
		/// <exception cref="System.ArgumentNullException">NormalFile, CustomFile, or TempFile are null.</exception>
		public static void FixBrokenFilesIfNeeded( this ISwitchableFile switchableFile, out bool wasBroken )
		{
			wasBroken = false;
			Logging.Log.InfoFormat( "Checking integrity of switchable file {0}.", switchableFile.NormalFile );

			FileSwitcher files = switchableFile.AsFileSwitcher();

			if ( files.FilesBroken() )
			{
				wasBroken = true;
				Logging.Log.Info( "Mixed up files detected, attempting to fix them..." );

				files.FixBrokenFiles();
				Logging.Log.Info( "Fixed." );
			}
			else
			{
				Logging.Log.InfoFormat( "{0} is ok.", switchableFile.NormalFile );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="switchableFile"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">NormalFile, CustomFile, TempFile, or RelativeRoot contain
		/// invalid characters.</exception>
		/// <exception cref="System.ArgumentNullException">NormalFile, CustomFile, TempFile, or RelativeRoot
		/// are null.</exception>
		public static FileSwitcher AsFileSwitcher( this ISwitchableFile switchableFile )
		{
			FileSwitcher fileSwitcher = new FileSwitcher();
			fileSwitcher.NormalFile = switchableFile.ResolveNormalFile();
			fileSwitcher.CustomFile = switchableFile.ResolveCustomFile();
			fileSwitcher.TempFile = switchableFile.ResolveTempFile();
			fileSwitcher.FileType = switchableFile.FileType;
			return fileSwitcher;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	interface ISwitchableFile
	{
		string Name { get; }
		string WhetherToSwitchArg { get; }
		string CustomFileArg { get; }
		string TempFileArg { get; }
		string SettingName { get; }
		string DefaultCustomFile { get; }
		string DefaultTempFile { get; }
		bool Switch { get; set; }
		string NormalFile { get; set; }
		string CustomFile { get; set; }
		string TempFile { get; set; }
		string RelativeRoot { get; set; }
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
		/// Sets CustomFile and TempFile to DefaultCustomFile and DefaultTempFile if they are null.
		/// </summary>
		/// <param name="switchableFile"></param>
		public static void ApplyDefaults( this ISwitchableFile switchableFile )
		{
			if ( switchableFile.CustomFile == null )
			{
				switchableFile.CustomFile = switchableFile.DefaultCustomFile;
			}
			if ( switchableFile.TempFile == null )
			{
				switchableFile.TempFile = switchableFile.DefaultTempFile;
			}
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
			return fileSwitcher;
		}
	}
}

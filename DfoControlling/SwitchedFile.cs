using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dfo.Controlling
{
	public class SwitchedFile : IDisposable
	{
		private bool m_disposed = false;
		
		public string SwitchedPath { get; private set; }
		public string SwitchedOriginalPath { get; private set; }
		public string TempPath { get; private set; }
		public FileType FileType { get; private set; }

		internal SwitchedFile( string switchedPath, string switchedOriginalPath, string tempPath, FileType fileType )
		{
			switchedPath.ThrowIfNull( "switchedPath" );
			switchedOriginalPath.ThrowIfNull( "switchedOriginalPath" );
			tempPath.ThrowIfNull( "tempPath" );

			SwitchedPath = switchedPath;
			SwitchedOriginalPath = switchedOriginalPath;
			TempPath = tempPath;
			FileType = fileType;
		}

		/// <summary>
		/// Switches the files back. Use this instead of Dispose() to be notified of failure with an exception.
		/// </summary>
		/// <exception cref="System.IO.IOException">The files could not be switched back. One or neither of
		/// the file moves may have been done.</exception>
		public void SwitchBack()
		{
			if ( m_disposed )
			{
				return; // Already switched back (or tried and failed on the second move), so nothing to do here
			}

			Action<string, string> move = FileType.GetMoveFunction();
			
			bool firstMoveSuccessful = false;
			try
			{
				move( SwitchedPath, SwitchedOriginalPath );
				firstMoveSuccessful = true;
				m_disposed = true;

				move( TempPath, SwitchedPath );
			}
			catch ( Exception ex )
			{
				if ( !firstMoveSuccessful )
				{
					throw new IOException( string.Format(
						"Could not move {0} back to {1}. {2} Files are in an inconsistent state!",
						SwitchedPath, SwitchedOriginalPath, ex.Message ), ex );
				}
				else
				{
					throw new IOException( string.Format(
						"Could not move {0} back to {1}. {2} Files are in an inconsistent state!",
						TempPath, SwitchedPath, ex.Message ), ex );
				}
			}
		}

		public void Dispose()
		{
			try
			{
				SwitchBack();
			}
			catch ( IOException )
			{
				// Caller had a chance to call SwitchBack instead and get the exception.
				// Don't throw exceptions from Dispose().
				;
			}
		}
	}
}

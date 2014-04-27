using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.ControlPanel
{
	interface IUiSwitchableFile : ISwitchableFile
	{
		/// <summary>
		/// Whether or not the user wants to switch the files regardless of whether they currently *can* be
		/// switched.
		/// </summary>
		bool SwitchIfFilesOk { get; set; }

		new string CustomFile { get; set; } // add a setter

		new string TempFile { get; set; } // add a setter

		new string RelativeRoot { get; set; } // add a setter

		/// <summary>
		/// Refresh any relevant UI controls, for example by checking if NormalFile and CustomFile exist.
		/// </summary>
		void Refresh();
	}
}

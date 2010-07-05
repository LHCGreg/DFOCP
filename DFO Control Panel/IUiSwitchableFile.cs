using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.ControlPanel
{
	interface IUiSwitchableFile : ISwitchableFile
	{
		bool SwitchIfFilesOk { get; set; }
		/// <summary>
		/// Refresh any relevant UI controls, for example by checking if NormalFile and CustomFile exist.
		/// </summary>
		void Refresh();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.ControlPanel
{
	/// <summary>
	/// Used if a switchable file does not have a real UI binding.
	/// </summary>
	class PlainUiSwitchableFile : SwitchableFile, IUiSwitchableFile
	{
		public bool SwitchIfFilesOk { get; set; }
		
		public PlainUiSwitchableFile( ISwitchableFile other )
			: base( other )
		{
			SwitchIfFilesOk = other.Switch;
		}

		public void Refresh()
		{
			;
		}
	}
}

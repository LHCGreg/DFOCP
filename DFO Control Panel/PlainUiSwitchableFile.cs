using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.ControlPanel
{
	class PlainUiSwitchableFile : SwitchableFile, IUiSwitchableFile
	{
		public PlainUiSwitchableFile( ISwitchableFile other )
			: base( other )
		{
			SwitchIfFilesOk = other.Switch;
		}

		public bool SwitchIfFilesOk { get; set; }

		public void Refresh()
		{
			;
		}
	}
}

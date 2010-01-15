using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dfo.Login;

namespace Dfo.BrowserlessDfoGui
{
	class StartupSettings
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public bool? RememberUsername { get; set; }
		public bool? ClosePopup { get; set; }
		public bool? LaunchWindowed { get; set; }
		public bool? SwitchSoundpacks { get; set; }
		public string DfoDir { get; set; }
		public string CustomSoundpackDir { get; set; }
		public string TempSoundpackDir { get; set; }

		public StartupSettings()
		{
			;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dfo.ControlPanel
{
	static class Paths
	{
		public static string DataDir { get { return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), "DFO Control Panel" ); } }
		public static string LogPath { get { return Path.Combine( DataDir, "DFO Control Panel.log" ); } }
		public static string SettingsPath { get { return Path.Combine( DataDir, "settings.xml" ); } }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Dfo.ControlPanel
{
	static class Paths
	{
		public static string DataDir { get { return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), "DFO Control Panel" ); } }
		public static string LogDir { get { return DataDir; } }

		private static string m_logPath = null;
		public static string LogPath
		{
			get
			{
				if ( m_logPath == null )
				{
					// Store the log path when it's first requested. We can't regenerate it each time
					// because it contains the date and time.
					DateTime now = DateTime.Now;
					string timeString = string.Format( "{0}-{1}-{2} {3}_{4}_{5}",
						now.Month, now.Day, now.Year, now.Hour, now.Minute, now.Second );
					int processId;
					using ( Process thisProcess = Process.GetCurrentProcess() )
					{
						processId = thisProcess.Id;
					}

					string filename = string.Format( "DFOCP {0} [{1}].log", timeString, processId );
					m_logPath = Path.Combine( LogDir, filename );
				}

				return m_logPath;
			}
		}

		public static string SettingsPath { get { return Path.Combine( DataDir, "settings.xml" ); } }
	}
}

/*
 Copyright 2010 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
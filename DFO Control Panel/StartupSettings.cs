using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	/// <summary>
	/// Null values indicate that a default should be used.
	/// </summary>
	class StartupSettings
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public bool? RememberUsername { get; set; }
		public bool? ClosePopup { get; set; }
		public bool? LaunchWindowed { get; set; }
		public string DfoDir { get; set; }
		public IDictionary<string, SwitchableFile> SwitchableFiles { get; set; }
		public IDictionary<string, bool?> SwitchFile { get; set; } // options in StartupSettings must be nullable to indicate preference for the default
		
		public StartupSettings()
		{
			SwitchableFiles = new Dictionary<string, SwitchableFile>();
			SwitchFile = new Dictionary<string, bool?>();
			ICollection<SwitchableFile> switchableFiles = SwitchableFile.GetSwitchableFiles();
			foreach ( SwitchableFile switchableFile in switchableFiles )
			{
				SwitchableFiles.Add( switchableFile.Name, switchableFile );
				SwitchFile.Add( switchableFile.Name, null );
			}
		}
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
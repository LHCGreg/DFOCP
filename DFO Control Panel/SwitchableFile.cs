using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.ControlPanel
{
	class SwitchableFile : ISwitchableFile
	{
		public string Name { get; set; }
		public string WhetherToSwitcharg { get; set; }
		public string CustomFileArg { get; set; }
		public string TempFileArg { get; set; }
		public string SettingName { get; set; }
		public string DefaultCustomFile { get; set; }
		public string DefaultTempFile { get; set; }

		private bool m_switch = false;
		public bool Switch { get { return m_switch; } set { m_switch = value; } }

		public string NormalFile { get; set; }
		public string CustomFile { get; set; }
		public string TempFile { get; set; }

		public string RelativeRoot { get { return Environment.CurrentDirectory; } }

		public SwitchableFile()
		{
			;
		}

		public static ICollection<SwitchableFile> GetSwitchableFiles()
		{
			List<SwitchableFile> switchableFiles = new List<SwitchableFile>();
			
			SwitchableFile soundpack = new SwitchableFile();
			soundpack.CustomFileArg = "customsounddir";
			soundpack.DefaultCustomFile = "SoundPacksCustom";
			soundpack.DefaultTempFile = "SoundPacksOriginal";
			soundpack.Name = Soundpacks;
			soundpack.NormalFile = "SoundPacks";
			soundpack.SettingName = "soundpack";
			soundpack.TempFileArg = "tempsounddir";
			soundpack.WhetherToSwitcharg = "soundswitch";

			switchableFiles.Add( soundpack );

			soundpack = new SwitchableFile();
			soundpack.CustomFileArg = "customaudioxml";
			soundpack.DefaultCustomFile = "customaudio.xml";
			soundpack.DefaultTempFile = "originalaudio.xml";
			soundpack.Name = AudioXml;
			soundpack.NormalFile = "audio.xml";
			soundpack.SettingName = "audio.xml";
			soundpack.TempFileArg = "tempaudioxml";
			soundpack.WhetherToSwitcharg = "audioxmlswitch";

			switchableFiles.Add( soundpack );

			return switchableFiles;
		}

		public static string Soundpacks { get { return "Soundpacks"; } }
		public static string AudioXml { get { return "audio.xml"; } }
	}
}

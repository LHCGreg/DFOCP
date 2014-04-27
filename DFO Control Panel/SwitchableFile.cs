using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	class SwitchableFile : ISwitchableFile
	{
		public string Name { get; set; }
		public string WhetherToSwitchArg { get; set; }
		public string CustomFileArg { get; set; }
		public string TempFileArg { get; set; }
		public string DefaultCustomFile { get; set; }
		public string DefaultTempFile { get; set; }
		public FileType FileType { get; set; }

		private bool m_switch = false;
		public bool Switch { get { return m_switch; } set { m_switch = value; } }

		public string NormalFile { get; set; }
		public string CustomFile { get; set; }
		public string TempFile { get; set; }

		private string m_relativeRoot = Environment.CurrentDirectory;
		public string RelativeRoot { get { return m_relativeRoot; } set { m_relativeRoot = value; } }

		public SwitchableFile()
		{
			;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="other"></param>
		public SwitchableFile( ISwitchableFile other )
		{
			this.Name = other.Name;
			this.FileType = other.FileType;
			this.WhetherToSwitchArg = other.WhetherToSwitchArg;
			this.CustomFileArg = other.CustomFileArg;
			this.TempFileArg = other.TempFileArg;
			this.DefaultCustomFile = other.DefaultCustomFile;
			this.DefaultTempFile = other.DefaultTempFile;
			this.Switch = other.Switch;
			this.NormalFile = other.NormalFile;
			this.CustomFile = other.CustomFile;
			this.TempFile = other.TempFile;
			this.RelativeRoot = other.RelativeRoot;
		}

		/// <summary>
		/// Sets CustomFile and TempFile to DefaultCustomFile and DefaultTempFile if they are null.
		/// </summary>
		public void ApplyDefaults()
		{
			if ( CustomFile == null )
			{
				CustomFile = DefaultCustomFile;
			}
			if ( TempFile == null )
			{
				TempFile = DefaultTempFile;
			}
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
			soundpack.TempFileArg = "tempsounddir";
			soundpack.WhetherToSwitchArg = "soundswitch";
			soundpack.FileType = FileType.Directory;

			switchableFiles.Add( soundpack );

			SwitchableFile audioXml = new SwitchableFile();
			audioXml = new SwitchableFile();
			audioXml.CustomFileArg = "customaudioxml";
			audioXml.DefaultCustomFile = "customaudio.xml";
			audioXml.DefaultTempFile = "originalaudio.xml";
			audioXml.Name = AudioXml;
			audioXml.NormalFile = "audio.xml";
			audioXml.TempFileArg = "tempaudioxml";
			audioXml.WhetherToSwitchArg = "audioxmlswitch";
			audioXml.FileType = FileType.RegularFile;

			switchableFiles.Add( audioXml );

			return switchableFiles;
		}

		/// <summary>
		/// Gets the Name (not the name of the NormalFile) of the soundpack switchable.
		/// </summary>
		public static string Soundpacks { get { return "Soundpacks"; } }
		
		/// <summary>
		/// Gets the Name (not the name of the NormalFile) of the audio.xml switchable.
		/// </summary>
		public static string AudioXml { get { return "audio.xml"; } }
	}
}

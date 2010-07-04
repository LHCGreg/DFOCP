using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	/// <summary>
	/// Binds switchable file properties to a checkbox.
	/// </summary>
	class CheckboxSwitchableFile : IUiSwitchableFile
	{
		private CheckBox m_checkbox;
		
		public string Name { get; private set; }
		public string WhetherToSwitchArg { get; private set; }
		public string CustomFileArg { get; private set; }
		public string TempFileArg { get; private set; }
		public string SettingName { get; private set; }
		public string DefaultCustomFile { get; private set; }
		public string DefaultTempFile { get; private set; }

		public bool Switch
		{
			get
			{
				return m_checkbox.Enabled && m_checkbox.Checked;
			}
			set
			{
				if ( m_checkbox.Enabled )
				{
					m_checkbox.Checked = true;
				}
			}
		}

		public string NormalFile { get; set; }
		public string CustomFile { get; set; }
		public string TempFile { get; set; }

		public string RelativeRoot { get; set; }

		/// <summary>
		/// Binds switchable file properties to a checkbox using the properties of some other ISwitchableFile.
		/// </summary>
		/// <param name="checkbox">The checkbox to bind to.</param>
		/// <param name="other"></param>
		public CheckboxSwitchableFile( CheckBox checkbox, ISwitchableFile other )
		{
			checkbox.ThrowIfNull( "checkbox" );
			m_checkbox = checkbox;

			Name = other.Name;
			WhetherToSwitchArg = other.WhetherToSwitchArg;
			CustomFileArg = other.CustomFileArg;
			TempFileArg = other.TempFileArg;
			SettingName = other.SettingName;
			DefaultCustomFile = other.DefaultCustomFile;
			DefaultTempFile = other.DefaultTempFile;
			NormalFile = other.NormalFile;
			CustomFile = other.CustomFile;
			TempFile = other.TempFile;
			RelativeRoot = other.RelativeRoot;

			Switch = other.Switch;

			Refresh();
		}

		/// <summary>
		/// Refreshes the Enabled property of the checkbox based on the existence of NormalFile and CustomFile
		/// and the availablility of TempFile. This method must be called on the UI thread of the checkbox.
		/// </summary>
		public void Refresh()
		{
			try
			{
				if ( NormalFile != null && Utilities.FileOrDirectoryExists( this.ResolveNormalFile() ) &&
				   CustomFile != null && Utilities.FileOrDirectoryExists( this.ResolveCustomFile() ) &&
				   TempFile != null && !Utilities.FileOrDirectoryExists( this.ResolveTempFile() ) )
				{
					m_checkbox.Enabled = true;
				}
				else
				{
					m_checkbox.Enabled = false;
					m_checkbox.Checked = false;
				}
			}
			catch ( ArgumentException )
			{
				m_checkbox.Enabled = false;
				m_checkbox.Checked = false;
			}
		}
	}
}

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

		private bool m_switchIfFilesOk;
		public bool SwitchIfFilesOk
		{
			get { return m_switchIfFilesOk; }
			set { m_switchIfFilesOk = value; Switch = value; }
		}

		// XXX: Doing a get right after a set may not give you the same value back if the checkbox isn't enabled.
		// That's kind of unintutive behavior.
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
					m_checkbox.Checked = value;
				}
			}
		}

		private bool m_autoRefresh = true;
		public bool AutoRefresh { get { return m_autoRefresh; } set { m_autoRefresh = value; } }

		private string m_normalFile;
		public string NormalFile
		{
			get { return m_normalFile; }
			set { m_normalFile = value; if ( AutoRefresh ) Refresh(); }
		}

		private string m_customFile;
		public string CustomFile
		{
			get { return m_customFile; }
			set { m_customFile = value; if ( AutoRefresh ) Refresh(); }
		}

		private string m_tempFile;
		public string TempFile
		{
			get { return m_tempFile; }
			set { m_tempFile = value; if ( AutoRefresh ) Refresh(); }
		}

		private string m_relativeRoot;
		public string RelativeRoot
		{
			get { return m_relativeRoot; }
			set { m_relativeRoot = value; if ( AutoRefresh ) Refresh(); }
		}

		/// <summary>
		/// Binds switchable file properties to a checkbox using the properties of some other ISwitchableFile.
		/// </summary>
		/// <param name="checkbox">The checkbox to bind to.</param>
		/// <param name="other"></param>
		public CheckboxSwitchableFile( CheckBox checkbox, ISwitchableFile other )
		{
			checkbox.ThrowIfNull( "checkbox" );
			m_checkbox = checkbox;

			AutoRefresh = false;

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
			SwitchIfFilesOk = other.Switch;

			AutoRefresh = true;

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
					if ( SwitchIfFilesOk )
					{
						m_checkbox.Checked = true;
					}
				}
				else
				{
					if ( m_checkbox.Enabled )
					{
						m_switchIfFilesOk = m_checkbox.Checked;
					}
					m_checkbox.Enabled = false;
					m_checkbox.Checked = false;
				}
			}
			catch ( ArgumentException )
			{
				if ( m_checkbox.Enabled )
				{
					m_switchIfFilesOk = m_checkbox.Checked;
				}
				m_checkbox.Enabled = false;
				m_checkbox.Checked = false;
			}
		}
	}
}

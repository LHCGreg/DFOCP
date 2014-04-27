using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dfo.Controlling;

namespace Dfo.ControlPanel
{
	/// <summary>
	/// Binds switchable file properties to a checkbox. The Switch property is bound to the Checked and Enabled
	/// properties of the checkbox and the checkbox will automatically check the switchability
	/// when CustomFile, TempFile, and RelativeRoot are changed and set the checkbox's Enabled property
	/// accordingly.
	/// </summary>
	class CheckboxSwitchableFile : IUiSwitchableFile
	{
		// This class remembers what the user wants so that the checkbox can be checked or unchecked when
		// re-enabling the checkbox after discovering that the files are switchable in reality.
		// When the checkbox is enabled, this class will set the Checked state to SwitchIfFilesOk.
		// When the checkbox is disabled, this class will also uncheck the checkbox.
		private CheckBox m_checkbox;

		public string Name { get; private set; }
		public string WhetherToSwitchArg { get; private set; }
		public string CustomFileArg { get; private set; }
		public string TempFileArg { get; private set; }
		public string DefaultCustomFile { get; private set; }
		public string DefaultTempFile { get; private set; }
		public FileType FileType { get; private set; }

		private bool m_switchIfFilesOk;
		public bool SwitchIfFilesOk
		{
			get { return m_switchIfFilesOk; }
			set { m_switchIfFilesOk = value; if ( m_checkbox.Enabled ) m_checkbox.Checked = value; }
		}

		public bool Switch
		{
			get
			{
				return m_checkbox.Enabled && m_checkbox.Checked;
			}
		}

		private bool m_autoRefresh = true;
		/// <summary>
		/// The checkbox Enabled property is refreshed after setting any of the file properties or the
		/// relative root if this property is true. You may wish to set it to false when doing several
		/// changes at once to those properties.
		/// </summary>
		public bool AutoRefresh { get { return m_autoRefresh; } set { m_autoRefresh = value; } }

		private string m_normalFile;
		public string NormalFile
		{
			get { return m_normalFile; }
			private set { m_normalFile = value; if ( AutoRefresh ) Refresh(); }
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
			other.ThrowIfNull( "other" );

			m_checkbox = checkbox;

			m_checkbox.CheckedChanged += ( object sender, EventArgs e ) =>
			{
				if ( m_checkbox.Enabled )
				{
					m_switchIfFilesOk = m_checkbox.Checked;
				}
			};

			m_checkbox.EnabledChanged += ( object sender, EventArgs e ) =>
			{
				if ( m_checkbox.Enabled )
				{
					m_checkbox.Checked = SwitchIfFilesOk;
				}
				else
				{
					m_checkbox.Checked = false;
				}
			};

			AutoRefresh = false;

			FileType = other.FileType;

			Name = other.Name;
			WhetherToSwitchArg = other.WhetherToSwitchArg;
			CustomFileArg = other.CustomFileArg;
			TempFileArg = other.TempFileArg;
			DefaultCustomFile = other.DefaultCustomFile;
			DefaultTempFile = other.DefaultTempFile;
			NormalFile = other.NormalFile;
			CustomFile = other.CustomFile;
			TempFile = other.TempFile;
			RelativeRoot = other.RelativeRoot;

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
			Logging.Log.DebugFormat( "Refreshing checkbox for switchable {0} with ('{1}', '{2}', '{3}')",
				Name, NormalFile, CustomFile, TempFile );
			Func<string, bool> exists = FileType.GetExistsFunction();
			try
			{
				if ( NormalFile != null && CustomFile != null && TempFile != null &&
					exists( this.ResolveNormalFile() ) && exists( this.ResolveCustomFile() )
					&& !exists( this.ResolveTempFile() ) )
				{
					Logging.Log.DebugFormat( "Enabling checkbox." );
					m_checkbox.Enabled = true;
				}
				else
				{
					Logging.Log.DebugFormat( "Disabling checkbox." );
					m_checkbox.Enabled = false;
				}
			}
			catch ( ArgumentException )
			{
				Logging.Log.DebugFormat( "Disabling checkbox." );
				m_checkbox.Enabled = false;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Dfo.ControlPanel
{
	public static class VersionInfo
	{
		public const string Revision = "$revision$";

		public static string AssemblyTitle
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyTitleAttribute ), false );
				if ( attributes.Length > 0 )
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[ 0 ];
					if ( titleAttribute.Title != "" )
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().CodeBase );
			}
		}

		public static string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public static string AssemblyDescription
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyDescriptionAttribute ), false );
				if ( attributes.Length == 0 )
				{
					return "";
				}
				return ( (AssemblyDescriptionAttribute)attributes[ 0 ] ).Description;
			}
		}

		public static string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyProductAttribute ), false );
				if ( attributes.Length == 0 )
				{
					return "";
				}
				return ( (AssemblyProductAttribute)attributes[ 0 ] ).Product;
			}
		}

		public static string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );
				if ( attributes.Length == 0 )
				{
					return "";
				}
				return ( (AssemblyCopyrightAttribute)attributes[ 0 ] ).Copyright;
			}
		}

		public static string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCompanyAttribute ), false );
				if ( attributes.Length == 0 )
				{
					return "";
				}
				return ( (AssemblyCompanyAttribute)attributes[ 0 ] ).Company;
			}
		}

		public static string LicenseStatement
		{
			get { return "The source code for this program is available under the Apache 2.0 License. Many ideas were taken from Tomato's DFOAssist script. Thanks Tomato!"; }
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Dfo.BrowserlessDfoGui
{
	static class ExtensionMethods
	{
		public static IAsyncResult BeginInvoke( this Control control, ThreadStart func )
		{
			return control.BeginInvoke( func );
		}
	}
}

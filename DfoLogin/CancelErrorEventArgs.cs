using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.Login
{
	public class CancelErrorEventArgs : ErrorEventArgs
	{
		public bool Cancel { get; set; }

		public CancelErrorEventArgs( Exception error )
			: base( error )
		{
			Cancel = false;
		}

		public CancelErrorEventArgs( Exception error, bool cancel )
			: base( error )
		{
			Cancel = cancel;
		}
	}
}

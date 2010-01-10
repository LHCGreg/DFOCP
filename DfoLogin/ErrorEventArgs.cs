using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.Login
{
	public class ErrorEventArgs : EventArgs
	{
		public Exception Error { get; private set; }

		public ErrorEventArgs( Exception error )
		{
			Error = error;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace log4net2common
{
	public class Log4NetCommonLogger : Common.Logging.Log4Net.Log4NetLogger
	{
		public Log4NetCommonLogger( log4net.Core.ILoggerWrapper log )
			: base( log )
		{
			; // Now WHY couldn't the Log4NetLogger constructor be public instead of protected internal? -_-
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Dfo.ControlPanel
{
	static class ExtensionMethods
	{
		// Because casting a lambda expression to a delegate type like ThreadStart every time is annoying.
		public static IAsyncResult BeginInvoke( this Control control, ThreadStart func )
		{
			return control.BeginInvoke( func );
		}

		public static void Invoke( this Control control, ThreadStart func )
		{
			control.Invoke( func );
		}

		public static void InvokeIfRequiredAsync( this Control control, ThreadStart func )
		{
			if ( control.InvokeRequired )
			{
				control.BeginInvoke( func );
			}
			else
			{
				func();
			}
		}

		public static void InvokeIfRequiredSync( this Control control, ThreadStart func )
		{
			if ( control.InvokeRequired )
			{
				control.Invoke( func );
			}
			else
			{
				func();
			}
		}

		public static int Clamp( this int value, int min, int max )
		{
			if ( value < min )
			{
				return min;
			}
			else if ( value > max )
			{
				return max;
			}
			else
			{
				return value;
			}
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
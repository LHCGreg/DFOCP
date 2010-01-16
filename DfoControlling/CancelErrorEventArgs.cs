using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.Controlling
{
	/// <summary>
	/// Notifies an event listener of an error and allows the listener to cancel an operation.
	/// </summary>
	public class CancelErrorEventArgs : ErrorEventArgs
	{
		/// <summary>
		/// Gets or sets whether to cancel the operation.
		/// </summary>
		public bool Cancel { get; set; }

		/// <summary>
		/// Constructs a new <c>CancelErrorEventArgs</c> instance.
		/// </summary>
		/// <param name="error">The exception that caused the error.</param>
		public CancelErrorEventArgs( Exception error )
			: base( error )
		{
			Cancel = false;
		}

		/// <summary>
		/// Constructs a new <c>CancelErrorEventArgs</c> instance.
		/// </summary>
		/// <param name="error">The exception that caused the error.</param>
		/// <param name="cancel">The default value of the <c>Cancel</c> property.</param>
		public CancelErrorEventArgs( Exception error, bool cancel )
			: base( error )
		{
			Cancel = cancel;
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
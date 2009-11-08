using System;
using System.Collections.Generic;
using System.Text;

namespace Dfo.BrowserlessDfoGui
{
	public class UsernamePasswordPair
	{
		private readonly string m_username;
		public string Username { get { return m_username; } }
		private readonly string m_password;
		public string Password { get { return m_password; } }

		public UsernamePasswordPair( string username, string password )
		{
			m_username = username;
			m_password = password;
		}
	}
}

/*
 Copyright 2009 Greg Najda

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
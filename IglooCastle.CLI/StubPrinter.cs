using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
	class StubPrinter : IPrinter
	{
		public string Print(object element, bool typeLinks = true)
		{
			throw new NotImplementedException();
		}

		public string Syntax(object element, bool typeLinks = true)
		{
			throw new NotImplementedException();
		}

		public string Signature(object element, bool typeLinks = true)
		{
			throw new NotImplementedException();
		}

		public string Format(object element, string format)
		{
			return element.ToString();
		}
	}
}

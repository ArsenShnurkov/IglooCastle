using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	internal sealed class NamespacePrinter : PrinterBase<NamespaceElement, string>
	{
		public NamespacePrinter(Documentation documentation) : base(documentation)
		{
		}

		public override string Print(NamespaceElement element, bool typeLinks = true)
		{
			return string.Format(
				"<a href=\"{0}\">{1}</a>",
				Documentation.FilenameProvider.Filename(element),
				element.Namespace);
		}

		public override string Syntax(NamespaceElement element, bool typeLinks = true)
		{
			throw new NotImplementedException();
		}

		public override string Signature(NamespaceElement element, bool typeLinks = true)
		{
			throw new NotImplementedException();
		}
	}
}

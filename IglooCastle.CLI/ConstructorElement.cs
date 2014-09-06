using System.Reflection;
using System;
using System.Text;

namespace IglooCastle.CLI
{
	public class ConstructorElement : MethodBaseElement<ConstructorInfo>
	{
		public ConstructorElement(Documentation documentation, TypeElement ownerType, ConstructorInfo constructor) :
			base(documentation, ownerType, constructor)
		{
		}

		internal ConstructorInfo Constructor
		{
			get { return Member; }
		}

		protected override IXmlComment GetXmlComment()
		{
			// M:IglooCastle.CLI.NamespaceElement.#ctor(IglooCastle.CLI.Documentation)
			return Documentation.GetMethodDocumentation(OwnerType.Type, "#ctor", Constructor.GetParameters());
		}

		protected override IPrinter GetPrinter()
		{
			return Documentation.PrinterFactory.GetConstructorPrinter();
		}

		public string Filename()
		{
			return Documentation.FilenameProvider.Filename(this);
		}

		public string Link()
		{
			if (DeclaringType.IsLocalType)
			{
				return Documentation.FilenameProvider.Filename(this);
			}

			return null;
		}
	}
}

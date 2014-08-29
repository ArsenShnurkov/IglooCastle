﻿using System.Reflection;

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

		public string ToHtml()
		{
			return Documentation.TypePrinter.Print(this);
		}

		public string ToSignature()
		{
			return Documentation.TypePrinter.Signature(this);
		}

		public string ToSyntax()
		{
			return Documentation.TypePrinter.Syntax(this);
		}

		public string Filename()
		{
			return Documentation.FilenameProvider.Filename(this);
		}
	}
}

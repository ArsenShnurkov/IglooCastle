using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	class ConstructorPrinter : MethodBasePrinter<ConstructorElement,ConstructorInfo>
	{
		public ConstructorPrinter(Documentation documentation) : base(documentation) { }

		public override string Link(ConstructorElement constructor)
		{
			if (constructor.DeclaringType.IsLocalType)
			{
				return Documentation.FilenameProvider.Filename(constructor);
			}

			return null;
		}

		public override string Print(ConstructorElement constructorElement, bool typeLinks = true)
		{
			string text = Signature(constructorElement);
			string link = Link(constructorElement);
			if (link == null)
			{
				return text;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", link.Escape(), text);
		}

		public override string Syntax(ConstructorElement constructorElement, bool typeLinks = true)
		{
			string access = AccessPrefix(constructorElement);
			string args = Parameters(constructorElement, typeLinks);
			return " ".JoinNonEmpty(
				SyntaxOfAttributes(constructorElement),
				access,
				string.Format(
					"{0}({1})",
					constructorElement.DeclaringType.ToString("n"),
					args)
				);
		}

		public override string Signature(ConstructorElement constructorElement, bool typeLinks = true)
		{
			string text = constructorElement.DeclaringType.ToString("n");
			if (constructorElement.DeclaringType.Constructors.Count() > 1)
			{
				text += ParameterSignature(constructorElement);
			}

			return text;
		}
	}
}

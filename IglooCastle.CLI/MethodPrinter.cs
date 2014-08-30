using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	class MethodPrinter : MethodBasePrinter<MethodElement, MethodInfo>
	{
		public MethodPrinter(Documentation documentation) : base(documentation) { }

		public override string Link(MethodElement method)
		{
			if (Documentation.IsLocalType(method.DeclaringType))
			{
				return Documentation.FilenameProvider.Filename(method);
			}

			return null;
		}

		public override string Print(MethodElement methodInfo, bool typeLinks = true)
		{
			string text = Signature(methodInfo);
			string link = Link(methodInfo);
			if (link == null)
			{
				return text;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", link.Escape(), text);
		}

		public override string Signature(MethodElement methodElement, bool typeLinks = true)
		{
			string text;
			if (methodElement.IsOverload)
			{
				text = methodElement.Name + ParameterSignature(methodElement);
			}
			else
			{
				text = methodElement.Name;
			}

			return text;
		}

		public override string Syntax(MethodElement method, bool typeLinks = true)
		{
			string access = AccessPrefix(method);
			string modifiers = Modifiers(method);
			string returnType = method.ReturnType.ToString(typeLinks ? "l" : "L");
			string args = Parameters(method, typeLinks);
			return " ".JoinNonEmpty(access, modifiers, returnType, method.Name).TrimStart(' ') + "(" + args + ")";
		}
	}
}

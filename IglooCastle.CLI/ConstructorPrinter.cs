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

		public override string Print(ConstructorElement constructorElement, bool typeLinks = true)
		{
			string text = Signature(constructorElement);
			string link = constructorElement.Link();
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
				SyntaxOfAttributes(constructorElement, typeLinks),
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

		public override string Format(ConstructorElement element, string format)
		{
			if (format == null || format.Length <= 1)
			{
				return base.Format(element, format);
			}

			return ToStringLongFormat(element, format);
		}

		private string ToStringLongFormatTranslate(ConstructorElement element, string variable)
		{
			switch (variable)
			{
				case "typename":
					return element.DeclaringType.ToString("n");
				case "args":
					return ParameterSignature(element);
				default:
					throw new FormatException("Unknown format specifier: " + variable);
			}
		}

		private void ToStringLongFormat(ConstructorElement element, StringBuilder buffer, string format, int startIndex)
		{
			if (startIndex >= format.Length)
			{
				return;
			}

			int idx = format.IndexOf('{', startIndex);
			if (idx < 0)
			{
				buffer.Append(format, startIndex, format.Length - startIndex);
			}
			else
			{
				buffer.Append(format, startIndex, idx - startIndex);
				int closeidx = format.IndexOf('}', idx + 1);
				if (closeidx < 0)
				{
					throw new FormatException("Missing closing '}'");
				}

				buffer.Append(ToStringLongFormatTranslate(
					element,
					format.Substring(idx + 1, closeidx - idx - 1)));
				ToStringLongFormat(element, buffer, format, closeidx + 1);
			}
		}

		private string ToStringLongFormat(ConstructorElement element, string format)
		{
			StringBuilder result = new StringBuilder();
			ToStringLongFormat(element, result, format, 0);
			return result.ToString();
		}
	}
}

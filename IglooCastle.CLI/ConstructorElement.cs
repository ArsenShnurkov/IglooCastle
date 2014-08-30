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

		public override string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format))
			{
				return base.ToString(format, formatProvider);
			}

			return ToStringShortFormat(format) ?? ToStringLongFormat(format);
		}

		private string ToStringShortFormat(string format)
		{
			switch (format)
			{
				case "x":
					return Documentation.TypePrinter.Syntax(this, typeLinks: false);
				default:
					return null;
			}
		}

		private string ToStringLongFormatTranslate(string variable)
		{
			switch (variable)
			{
				case "typename":
					return Documentation.TypePrinter.Name(DeclaringType, TypePrinter.NameComponents.Name);
				case "args":
					return Documentation.TypePrinter.ParameterSignature(this);
				default:
					throw new FormatException("Unknown format specifier: " + variable);
			}
		}

		private void ToStringLongFormat(StringBuilder buffer, string format, int startIndex)
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

				buffer.Append(ToStringLongFormatTranslate(format.Substring(idx + 1, closeidx - idx - 1)));
				ToStringLongFormat(buffer, format, closeidx + 1);
			}
		}

		private string ToStringLongFormat(string format)
		{
			StringBuilder result = new StringBuilder();
			ToStringLongFormat(result, format, 0);
			return result.ToString();
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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IglooCastle.CLI
{

	public class CustomAttributeDataElement : DocumentationElement<CustomAttributeData>
	{
		public CustomAttributeDataElement(Documentation documentation, CustomAttributeData member) : base(documentation, member)
		{
		}
		
		#region implemented abstract members of DocumentationElement
		public override IXmlComment XmlComment
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		public TypeElement AttributeType
		{
			get
			{
				return Documentation.Find(Member.AttributeType);
			}
		}

		private bool IsSpecialAttribute()
		{
			return Member.AttributeType == typeof(ExtensionAttribute);
		}

		private object FmtArg(object value)
		{
			if (value is string)
			{
				return "\"" + value + "\"";
			}

			return value;
		}

		public string ToSyntax()
		{
			if (IsSpecialAttribute())
			{
				return string.Empty;
			}

			string name = Member.AttributeType.Name;
			if (name.EndsWith("Attribute"))
			{
				name = name.Substring(0, name.Length - "Attribute".Length);
			}

			var ci = Member.Constructor;
			var cargs = Member.ConstructorArguments;
			var nargs = Member.NamedArguments;

			if (cargs.Any() || nargs.Any())
			{
				name += "(";
				name += string.Join(", ",
					cargs.Select(c => FmtArg(c.Value))
					.Concat(
					nargs.Select(n => n.MemberName + " = " + FmtArg(n.TypedValue.Value)))
				);
				name += ")";
			}

			return "[" + name + "]";
		}
	}
}

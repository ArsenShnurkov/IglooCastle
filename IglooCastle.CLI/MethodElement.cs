using System;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public class MethodElement : MethodBaseElement<MethodInfo>
	{
		public MethodElement(Documentation documentation, TypeElement ownerType, MethodInfo method)
			: base(documentation, ownerType, method)
		{
		}

		internal MethodInfo Method
		{
			get { return Member; }
		}

		public bool IsStatic
		{
			get { return Member.IsStatic; }
		}

		public bool IsAbstract
		{
			get { return Member.IsAbstract; }
		}

		public bool IsVirtual
		{
			get { return Member.IsVirtual; }
		}

		public bool IsOverride
		{
			get { return Member.IsOverride(); }
		}

		public bool IsOverload
		{
			get { return Member.IsOverload(); }
		}


		public TypeElement ReturnType
		{
			get { return Documentation.Find(Member.ReturnType); }
		}

		public bool IsPrivate
		{
			get
			{
				return Member.IsPrivate;
			}
		}

		public bool IsExtension()
		{
			return Member.IsExtension();
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

		public override string ToString(string format, IFormatProvider formatProvider)
		{
			switch (format)
			{
				case "x":
					return Documentation.TypePrinter.Syntax(this, typeLinks: false);
				default:
					return base.ToString(format, formatProvider);
			}
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetMethodDocumentation(OwnerType.Type, Method.Name, Method.GetParameters());
		}

		public bool IsFinal { get { return Member.IsFinal; } }
	}
}

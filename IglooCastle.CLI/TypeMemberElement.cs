using System;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public abstract class TypeMemberElement<T> : ReflectedElement<T>
		where T: MemberInfo
	{
		protected TypeMemberElement(Documentation documentation, TypeElement ownerType, T member) : base(documentation, member)
		{
			OwnerType = ownerType;
		}

		public TypeElement OwnerType { get; private set; }

		public TypeElement ReflectedType { get { return Documentation.Find(Member.ReflectedType); } }

		public TypeElement DeclaringType { get { return Documentation.Find(Member.DeclaringType); } }

		public override NamespaceElement NamespaceElement
		{
			get { return Documentation.Namespaces.Single(n => n.Namespace == OwnerType.Member.Namespace); }
		}

		public bool IsInherited
		{
			get
			{
				return Member.DeclaringType != OwnerType.Type;
			}
		}

		protected abstract PrinterBase GetPrinter();

		public override string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format))
			{
				return base.ToString(format, formatProvider);
			}

			switch (format)
			{
				case "x":
					return GetPrinter().Syntax(this, typeLinks: false);
				case "X":
					return GetPrinter().Syntax(this, typeLinks: true);
				default:
					return base.ToString(format, formatProvider);
			}
		}

		public string ToSignature()
		{
			return GetPrinter().Signature(this);
		}

		public string ToSyntax()
		{
			return GetPrinter().Syntax(this);
		}

		public string ToHtml()
		{
			return GetPrinter().Print(this);
		}
	}
}

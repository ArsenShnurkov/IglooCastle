using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace IglooCastle.CLI
{
	public abstract class PrinterBase : IPrinter
	{
		private readonly Documentation _documentation;

		protected PrinterBase(Documentation documentation)
		{
			_documentation = documentation;
		}

		public Documentation Documentation{ get { return _documentation; } }

		public abstract string Link(object element);
		public abstract string Print(object element, bool typeLinks = true);
		public abstract string Syntax(object element, bool typeLinks = true);
		public abstract string Signature(object element, bool typeLinks = true);
		public abstract string Format(object element, string format);
	}

	abstract class PrinterBase<T,TMember> : PrinterBase
		where T : DocumentationElement<TMember>
	{
		protected PrinterBase(Documentation documentation) : base(documentation) { }

		public abstract string Link(T element);
		public abstract string Print(T element, bool typeLinks = true);
		public abstract string Syntax(T element, bool typeLinks = true);
		public abstract string Signature(T element, bool typeLinks = true);

		public virtual string Format(T element, string format)
		{
			switch (format)
			{
				case "x":
					return Syntax(element, typeLinks: false);
				case "X":
					return Syntax(element, typeLinks: true);
				default:
					return element.Member.ToString();
			}
		}

		#region implemented abstract members of PrinterBase

		public sealed override string Link(object element)
		{
			return Link((T)element);
		}

		public sealed override string Print(object element, bool typeLinks = true)
		{
			return Print((T)element, typeLinks);
		}

		public sealed override string Syntax(object element, bool typeLinks = true)
		{
			return Syntax((T)element, typeLinks);
		}

		public sealed override string Signature(object element, bool typeLinks = true)
		{
			return Signature((T)element, typeLinks);
		}

		public sealed override string Format(object element, string format)
		{
			return Format((T)element, format);
		}

		#endregion

		protected string SyntaxOfAttributes<TElement>(ReflectedElement<TElement> element)
			where TElement : MemberInfo
		{
			return string.Join("", element.GetCustomAttributesData().Select(c => c.ToSyntax()));
		}
	}
}

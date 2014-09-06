using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace IglooCastle.CLI
{
	public abstract class PrinterBase
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
	}

	abstract class PrinterBase<T,TMember> : PrinterBase
		where T : DocumentationElement<TMember>
	{
		protected PrinterBase(Documentation documentation) : base(documentation) { }

		public abstract string Link(T element);
		public abstract string Print(T element, bool typeLinks = true);
		public abstract string Syntax(T element, bool typeLinks = true);
		public abstract string Signature(T element, bool typeLinks = true);

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

		#endregion

		protected string SyntaxOfAttributes<TElement>(ReflectedElement<TElement> element)
			where TElement : MemberInfo
		{
			return string.Join("", element.GetCustomAttributesData().Select(c => c.ToSyntax()));
		}
	}
}

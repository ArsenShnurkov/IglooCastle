using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public abstract class ReflectedElement<T> : DocumentationElement<T>
		where T: MemberInfo
	{
		protected ReflectedElement(Documentation documentation, T member)
			: base(documentation, member)
		{
		}

		public override IXmlComment XmlComment
		{
			get { return Documentation.GetXmlComment(this) ?? new MissingXmlComment(); }
		}

		public bool HasAttribute(string attributeName)
		{
			return Member.GetCustomAttributes().Any(a => a.GetType().FullName == attributeName || a.GetType().FullName == attributeName + "Attribute");
		}

		public Attribute GetAttribute(string attributeName)
		{
			return Member.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == attributeName || a.GetType().FullName == attributeName + "Attribute");
		}

		/// <summary>
		/// Gets the name of the reflected element.
		/// </summary>
		public string Name
		{
			get { return Member.Name; }
		}

		public Type DeclaringType
		{
			get { return Member.DeclaringType; }
		}

		public IEnumerable<Attribute> GetCustomAttributes(bool inherit)
		{
			return Member.GetCustomAttributes(inherit).Cast<Attribute>();
		}
	}
}

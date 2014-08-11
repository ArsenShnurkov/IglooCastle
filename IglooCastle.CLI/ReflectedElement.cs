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

		public string Name
		{
			get { return Member.Name; }
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
		/// Gets the custom attributes of this member.
		/// </summary>
		/// <param name="inherit">Determines if inherited attributes should be returned also.</param>
		/// <returns>A collection of <see cref="System.Attribute"/></returns>
		public IEnumerable<Attribute> GetCustomAttributes(bool inherit)
		{
			return Member.GetCustomAttributes(inherit).Cast<Attribute>();
		}
	}
}

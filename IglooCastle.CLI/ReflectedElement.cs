using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public abstract class ReflectedElement<T> : DocumentationElement<T>
		where T : MemberInfo
	{
		protected ReflectedElement(Documentation documentation, T member)
			: base(documentation, member)
		{
		}

		/// <summary>
		/// Gets the name of the reflected element.
		/// </summary>
		/// <seealso cref="MemberInfo.Name"/>
		public string Name
		{
			get { return Member.Name; }
		}

		public abstract NamespaceElement NamespaceElement
		{
			get;
		}

		public virtual MethodAttributes GetAccess()
		{
			return MethodAttributes.Public;
		}

		public override IXmlComment XmlComment
		{
			get { return GetXmlComment() ?? new MissingXmlComment(); }
		}

		public bool HasAttribute(string attributeName)
		{
			return Member.GetCustomAttributes().Any(a => a.GetType().FullName == attributeName || a.GetType().FullName == attributeName + "Attribute");
		}

		public AttributeElement GetAttribute(string attributeName)
		{
			var attribute = Member.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == attributeName || a.GetType().FullName == attributeName + "Attribute");
			return attribute == null ? null : new AttributeElement(Documentation, attribute);
		}

		/// <summary>
		/// Gets the custom attributes of this member.
		/// </summary>
		/// <param name="inherit">Determines if inherited attributes should be returned also.</param>
		/// <returns>A collection of <see cref="System.Attribute"/></returns>
		public IEnumerable<AttributeElement> GetCustomAttributes(bool inherit)
		{
			return Member.GetCustomAttributes(inherit).Cast<Attribute>().Select(a => new AttributeElement(Documentation, a));
		}

		protected abstract IXmlComment GetXmlComment();
	}
}

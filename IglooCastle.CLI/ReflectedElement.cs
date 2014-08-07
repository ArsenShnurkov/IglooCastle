using System;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public abstract class ReflectedElement<T> : DocumentationElement<T>
		where T: MemberInfo
	{
		public bool HasAttribute(string attributeName)
		{
			return Member.GetCustomAttributes().Any(a => a.GetType().FullName == attributeName || a.GetType().FullName == attributeName + "Attribute");
		}

		public Attribute GetAttribute(string attributeName)
		{
			return Member.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == attributeName || a.GetType().FullName == attributeName + "Attribute");
		}
	}
}

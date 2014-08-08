using System;
using System.Reflection;

namespace IglooCastle.CLI
{
	public class PropertyElement : TypeMemberElement<PropertyInfo>
	{
		public PropertyElement(Documentation documentation, TypeElement ownerType, PropertyInfo property)
			: base(documentation, ownerType, property)
		{
		}

		public PropertyInfo Property
		{
			get { return Member; }
		}

		public Type PropertyType
		{
			get { return Member.PropertyType; }
		}
	}
}

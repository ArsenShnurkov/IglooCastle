using System;
using System.Linq;
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

		public bool CanRead { get { return Member.CanRead; } }

		public bool CanWrite { get { return Member.CanWrite; } }

		public PropertyElement BasePropertyElement()
		{
			TypeElement baseTypeElement = OwnerType.BaseTypeElement;
			if (baseTypeElement == null)
			{
				return null;
			}

			return baseTypeElement.Properties.FirstOrDefault(p => p.Name == Name);
		}
	}
}

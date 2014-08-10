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

		internal PropertyInfo Property
		{
			get { return Member; }
		}

		public PropertyElement BasePropertyElement()
		{
			TypeElement baseTypeElement = OwnerType.BaseTypeElement;
			if (baseTypeElement == null)
			{
				return null;
			}

			return baseTypeElement.Properties.FirstOrDefault(p => p.Name == Name);
		}

		public MethodInfo GetGetMethod(bool nonPublic)
		{
			return Property.GetGetMethod(nonPublic);
		}

		public MethodInfo GetSetMethod(bool nonPublic)
		{
			return Property.GetSetMethod(nonPublic);
		}
	}
}

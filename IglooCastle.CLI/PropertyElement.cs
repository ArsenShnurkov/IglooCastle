﻿using System;
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

		public bool IsStatic
		{
			get
			{
				return Member.IsStatic();
			}
		}

		public override MethodAttributes GetAccess()
		{
			return Member.GetAccess();
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

		public bool IsPublic
		{
			get
			{
				return Property.IsPublic();
			}
		}

		public bool IsFamily
		{
			get
			{
				return Property.IsFamily();
			}
		}

		protected override IXmlComment GetXmlComment()
		{
			IXmlComment result = Documentation.GetXmlComment("//member[@name=\"P:" +
					Property.ReflectedType.FullName + "." +
					Property.Name + "\"]");

			if (result == null)
			{
				var basePropertyElement = BasePropertyElement();
				if (basePropertyElement != null)
				{
					result = basePropertyElement.GetXmlComment();
				}
			}

			return result;
		}
	}
}

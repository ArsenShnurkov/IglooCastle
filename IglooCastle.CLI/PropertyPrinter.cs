using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	class PropertyPrinter : PrinterBase<PropertyElement, PropertyInfo>
	{
		public PropertyPrinter(Documentation documentation) : base(documentation) { }

		public override string Print(PropertyElement property, bool typeLinks = true)
		{
			string link = property.Link();
			if (link == null)
			{
				return property.Name;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", link.Escape(), property.Name);
		}

		public override string Signature(PropertyElement property, bool typeLinks = true)
		{
			throw new NotImplementedException();
		}

		public override string Syntax(PropertyElement property, bool typeLinks = true)
		{
			var getter = property.CanRead ? property.GetMethod : null;
			var setter = property.CanWrite ? property.SetMethod : null;

			var getterAccess = getter != null ? getter.GetAccess() : MethodAttributes.PrivateScope;
			var setterAccess = setter != null ? setter.GetAccess() : MethodAttributes.PrivateScope;
			var maxAccess = ReflectionExtensions.Max(getterAccess, setterAccess);

			return " ".JoinNonEmpty(
				SyntaxOfAttributes(property, typeLinks),
				maxAccess.ToAccessString(),
				property.PropertyType.ToHtml(typeLinks),
				property.Name,
				"{",
				getter != null && !getter.IsPrivate ? ((getterAccess != maxAccess) ? getterAccess.ToAccessString() + " " : "") + "get;" : "",
				setter != null && !setter.IsPrivate ? ((setterAccess != maxAccess) ? setterAccess.ToAccessString() + " " : "") + "set;" : "",
				"}");
		}
	}
}

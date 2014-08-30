using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	internal sealed class TypePrinter
	{
		private readonly Documentation _documentation;

		public TypePrinter(Documentation documentation)
		{
			_documentation = documentation;
		}

		public string Print(TypeElement type, bool typeLinks = true)
		{
			if (type.IsArray)
			{
				return Print(type.GetElementType(), typeLinks) + "[]";
			}

			if (type.IsByRef)
			{
				return Print(type.GetElementType(), typeLinks);
			}

			string result;

			if (type.IsGenericType && !type.IsGenericParameter && !type.IsGenericTypeDefinition)
			{
				result = DoPrint(type.GetGenericTypeDefinition(), typeLinks);
			}
			else
			{
				result = DoPrint(type, typeLinks);
			}

			if (type.IsGenericType)
			{
				result += "&lt;";
				result += string.Join(", ", type.GetGenericArguments().Select(t => Print(t, typeLinks)));
				result += "&gt;";
			}

			return result;
		}

		private string DoPrint(TypeElement type, bool typeLinks)
		{
			string link = Link(type);
			string text = link != null ? ShortName(type) : FullName(type);
			if (link != null && typeLinks)
			{
				return string.Format("<a href=\"{0}\">{1}</a>", link.Escape(), text);
			}
			else
			{
				return text;
			}
		}

		private string FullName(TypeElement type)
		{
			if (type.IsGenericParameter)
			{
				return type.Name;
			}

			if (type.IsGenericType)
			{
				return type.Member.FullName.Split('`')[0];
			}

			return SystemTypes.Alias(type) ?? type.Member.FullName ?? ShortName(type);
		}

		private string ShortName(TypeElement type)
		{
			if (type.IsNested && !type.IsGenericParameter)
			{
				TypeElement containerType = type.DeclaringType;
				return ShortName(containerType) + "." + type.Name;
			}

			if (type.IsGenericType)
			{
				return type.Name.Split('`')[0];
			}

			return SystemTypes.Alias(type) ?? type.Name;
		}

		private bool IsSystemType(TypeElement type)
		{
			return type.Namespace == "System" || type.Namespace.StartsWith("System.");
		}

		private string Link(TypeElement type)
		{
			if (_documentation.IsLocalType(type))
			{
				return _documentation.FilenameProvider.Filename(type);
			}

			if (IsSystemType(type) && !type.IsGenericType)
			{
				return string.Format("http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx", type.Member.FullName.ToLowerInvariant());
			}

			return null;
		}

		[Flags]
		public enum NameComponents
		{
			Name = 0,
			GenericArguments = 1,
			Namespace = 2
		}

		public string Name(TypeElement type, NameComponents nameComponents)
		{
			string name = ShortName(type);

			if (type.IsGenericType && ((nameComponents & NameComponents.GenericArguments) == NameComponents.GenericArguments))
			{
				name = name + "&lt;" + string.Join(",", type.GetGenericArguments().Select(t => t.Name)) + "&gt;";
			}

			if ((nameComponents & NameComponents.Namespace) == NameComponents.Namespace)
			{
				name = type.Namespace + "." + name;
			}

			return name;
		}

		public string Syntax(TypeElement type, bool typeLinks = true)
		{
			string result = " ".JoinNonEmpty(
				"public",
				type.IsInterface ? "" :
				(type.IsStatic ? "static" : (type.IsSealed ? "sealed" : type.IsAbstract ? "abstract" : "")),
				type.IsClass ? "class" : type.IsEnum ? "enum" : type.IsInterface ? "interface" : "struct",
				type.ToString("s"));

			TypeElement baseType = type.BaseType;
			if (baseType != null && baseType.BaseType == null)
			{
				// every class derives from System.Object, that's not interesting
				baseType = null;
			}

			var interfaces = type.GetInterfaces();

			var baseTypes = new[] { baseType }.Concat(interfaces).Where(t => t != null).ToArray();
			if (baseTypes.Any())
			{
				result = result + " : " + string.Join(", ", baseTypes.Select(t => Print(t, typeLinks)));
			}

			return result;
		}

		public string Print(NamespaceElement namespaceElement)
		{
			return string.Format(
				"<a href=\"{0}\">{1}</a>",
				_documentation.FilenameProvider.Filename(namespaceElement),
				namespaceElement.Namespace);
		}
	}
}

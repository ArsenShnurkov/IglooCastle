using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public class TypePrinter
	{
		private readonly Documentation _documentation;
		private readonly FilenameProvider _filenameProvider;

		public TypePrinter(Documentation documentation, FilenameProvider filenameProvider)
		{
			_documentation = documentation;
			_filenameProvider = filenameProvider;
		}

		public string Print(TypeElement typeElement)
		{
			return Print(typeElement.Member);
		}

		public string Print(Type type)
		{
			if (type.IsArray)
			{
				return Print(type.GetElementType()) + "[]";
			}

			if (type.IsByRef)
			{
				return Print(type.GetElementType());
			}

			string result = DoPrint(type);

			if (type.IsGenericType)
			{
				result += "&lt;";
				result += string.Join(", ", type.GetGenericArguments().Select(t => Print(t)));
				result += "&gt;";
			}

			return result;
		}

		private string DoPrint(Type type)
		{
			string link = Link(type);
			string text = link != null ? ShortName(type) : FullName(type);
			if (link != null)
			{
				return string.Format("<a href=\"{0}\">{1}</a>", Escape(link), text);
			}
			else
			{
				return text;
			}
		}

		private Type GetContainerType(Type nestedType)
		{
			if (!nestedType.IsNested)
			{
				throw new ArgumentException("Type " + nestedType + " is not nested.");
			}

			if (_documentation.IsLocalType(nestedType))
			{
				TypeElement containerType = _documentation.FilterTypes(t => t.Member.GetNestedTypes().Contains(nestedType)).Single();
				return containerType.Member;
			}

			throw new NotImplementedException(nestedType.ToString());
		}

		private string FullName(Type type)
		{
			if (type.IsGenericType)
			{
				return type.FullName.Split('`')[0];
			}

			return SystemTypes.Alias(type) ?? type.FullName ?? ShortName(type);
		}

		private string ShortName(TypeElement typeElement)
		{
			return ShortName(typeElement.Type);
		}

		private string ShortName(Type type)
		{
			if (type.IsNested && !type.IsGenericParameter)
			{
				Type containerType = GetContainerType(type);
				return ShortName(containerType) + "." + type.Name;
			}

			if (type.IsGenericType)
			{
				return type.Name.Split('`')[0];
			}

			return SystemTypes.Alias(type) ?? type.Name;
		}

		private bool IsSystemType(Type type)
		{
			return type.Namespace == "System" || type.Namespace.StartsWith("System.");
		}

		private string Link(Type type)
		{
			if (_documentation.IsLocalType(type))
			{
				return _filenameProvider.Filename(type);
			}

			if (IsSystemType(type) && !type.IsGenericType)
			{
				return string.Format("http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx", type.FullName.ToLowerInvariant());
			}

			return null;
		}

		private string Link(PropertyInfo property)
		{
			if (_documentation.IsLocalType(property.DeclaringType))
			{
				return _filenameProvider.Filename(property);
			}

			return null;
		}

		private string Link(MethodInfo method)
		{
			if (_documentation.IsLocalType(method.DeclaringType))
			{
				return _filenameProvider.Filename(method);
			}

			return null;
		}

		private string Escape(string link)
		{
			return link.Replace("`", "%60");
		}

		public string Print(MethodElement methodElement)
		{
			return Print(methodElement.Method);
		}

		public string Print(PropertyElement propertyElement)
		{
			return Print(propertyElement.Member);
		}

		public string Print(PropertyInfo propertyInfo)
		{
			string link = Link(propertyInfo);
			if (link == null)
			{
				return propertyInfo.Name;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", Escape(link), propertyInfo.Name);
		}

		public string Print(MethodInfo methodInfo)
		{
			string text;
			if (methodInfo.IsOverload())
			{
				bool isExtension = methodInfo.IsExtension();
				text = methodInfo.Name + "(" + string.Join(", ", methodInfo.GetParameters().Select((p, index) => ((index == 0 && isExtension) ? "this " : "") + ShortName(p.ParameterType))) + ")";
			}
			else
			{
				text = methodInfo.Name;
			}

			string link = Link(methodInfo);
			if (link == null)
			{
				return text;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", Escape(link), text);
		}

		[Flags]
		public enum NameComponents
		{
			Name = 0,
			GenericArguments = 1,
			Namespace = 2
		}

		public string Name(TypeElement typeElement, NameComponents nameComponents)
		{
			return Name(typeElement.Member, nameComponents);
		}

		public string Name(Type type, NameComponents nameComponents)
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

		public string Syntax(MethodElement methodElement)
		{
			return Syntax(methodElement.Member);
		}

		private string AccessPrefix(MemberInfo member)
		{
			if (member.ReflectedType.IsInterface)
			{
				return string.Empty;
			}

			// TODO: more options + tests
			return "public";
		}

		private string Modifiers(MethodInfo method)
		{
			if (method.ReflectedType.IsInterface)
			{
				return string.Empty;
			}

			string modifiers = "";
			if (method.IsStatic)
			{
				modifiers += " static";
			}

			if (method.IsVirtual)
			{
				modifiers += method.IsOverride() ? " override" : " virtual";
			}

			if (method.IsAbstract)
			{
				modifiers += " abstract";
			}

			return modifiers.TrimStart(' ');
		}
		
		public string Syntax(MethodInfo method)
		{
			string access = AccessPrefix(method);
			string modifiers = Modifiers(method);			
			string returnType = Print(method.ReturnType);
			bool isExtension = method.IsExtension();
			string args = string.Join(", ", method.GetParameters().Select((p, index) => FormatParameter(p, isExtension && index == 0)));
			return Join(access, modifiers, returnType, method.Name).TrimStart(' ') + "(" + args + ")";
		}

		private string Join(params string[] args)
		{
			string result = "";
			foreach (string s in args.Where(arg => !string.IsNullOrWhiteSpace(arg)))
			{
				result += " " + s;
			}

			return result.TrimStart(' ');
		}

		private string FormatParameter(ParameterInfo parameterInfo, bool isExtensionThis)
		{
			string result = "";
			Type type = parameterInfo.ParameterType;
			if (type.IsByRef)
			{
				type = type.GetElementType();
				if (parameterInfo.IsOut)
				{
					result += "out ";
				}
				else
				{
					result += "ref ";
				}
			}

			if (isExtensionThis)
			{
				result += "this ";
			}

			result += Print(type);
			result += " ";
			result += parameterInfo.Name;
			return result;
		}
		
		public string Syntax(PropertyElement propertyElement)
		{
			return Syntax(propertyElement.Member);
		}

		public string Syntax(PropertyInfo property)
		{
			var getter = property.CanRead ? property.GetGetMethod() : null;
			var setter = property.CanWrite ? property.GetSetMethod() : null;

			return Join(AccessPrefix(getter), Print(property.PropertyType), property.Name, "{", 
				(getter != null ? "get;" : ""),
				(setter != null ? "set;" : ""),
				"}");
		}

		public string Print(NamespaceElement namespaceElement)
		{
			return string.Format(
				"<a href=\"{0}\">{1}</a>",
				_filenameProvider.Filename(namespaceElement),
				namespaceElement.Namespace);
		}
	}
}

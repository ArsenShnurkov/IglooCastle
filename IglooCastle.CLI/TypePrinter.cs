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

		[Obsolete]
		public string Print(Type type)
		{
			return Print(_documentation.Find(type));
		}

		public string Print(TypeElement type)
		{
			if (type.IsArray)
			{
				return Print(type.GetElementType()) + "[]";
			}

			if (type.IsByRef)
			{
				return Print(type.GetElementType());
			}

			string result;

			if (type.IsGenericType && !type.IsGenericParameter && !type.IsGenericTypeDefinition)
			{
				result = DoPrint(type.GetGenericTypeDefinition());
			}
			else
			{
				result = DoPrint(type);
			}

			if (type.IsGenericType)
			{
				result += "&lt;";
				result += string.Join(", ", type.GetGenericArguments().Select(t => Print(t)));
				result += "&gt;";
			}

			return result;
		}

		private string DoPrint(TypeElement type)
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

		private TypeElement GetContainerType(TypeElement nestedType)
		{
			if (!nestedType.IsNested)
			{
				throw new ArgumentException("Type " + nestedType + " is not nested.");
			}

			if (_documentation.IsLocalType(nestedType))
			{
				TypeElement containerType = _documentation.FilterTypes(t => t.GetNestedTypes().Contains(nestedType)).Single();
				return containerType;
			}

			throw new NotImplementedException(nestedType.ToString());
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
				TypeElement containerType = GetContainerType(type);
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
				return _filenameProvider.Filename(type);
			}

			if (IsSystemType(type) && !type.IsGenericType)
			{
				return string.Format("http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx", type.Member.FullName.ToLowerInvariant());
			}

			return null;
		}

		private string Link(PropertyElement property)
		{
			if (_documentation.IsLocalType(property.DeclaringType))
			{
				return _filenameProvider.Filename(property);
			}

			return null;
		}

		private string Link(MethodElement method)
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

		[Obsolete]
		public string Print(MethodInfo methodInfo)
		{
			return Print(_documentation.Find(methodInfo));
		}

		[Obsolete]
		public string Print(PropertyInfo property)
		{
			return Print(_documentation.Find(property));
		}

		public string Print(PropertyElement propertyElement)
		{
			string link = Link(propertyElement);
			if (link == null)
			{
				return propertyElement.Name;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", Escape(link), propertyElement.Name);
		}

		public string Print(MethodElement methodInfo)
		{
			string text;
			if (methodInfo.IsOverload)
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

		[Obsolete]
		public string Name(Type type, NameComponents nameComponents)
		{
			return Name(_documentation.Find(type), nameComponents);
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

		[Obsolete]
		public string Syntax(MethodInfo method)
		{
			return Syntax(_documentation.Find(method));
		}

		public string Syntax(MethodElement method)
		{
			string access = AccessPrefix(method);
			string modifiers = Modifiers(method);
			string returnType = Print(method.ReturnType);
			bool isExtension = method.IsExtension();
			string args = string.Join(", ", method.GetParameters().Select((p, index) => FormatParameter(p, isExtension && index == 0)));
			return Join(access, modifiers, returnType, method.Name).TrimStart(' ') + "(" + args + ")";
		}

		public string Access(MethodAttributes access)
		{
			switch (access)
			{
				case MethodAttributes.Family:
					return "protected";
				case MethodAttributes.Public:
					return "public";
				default:
					// TODO: more options + tests
					return access.ToString();
			}
		}

		private string AccessPrefix(MethodElement member)
		{
			if (member.ReflectedType.IsInterface)
			{
				return string.Empty;
			}

			MethodAttributes access = member.GetAccess();
			return Access(access);
		}

		private string Modifiers(MethodElement method)
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

			if (method.IsAbstract)
			{
				modifiers += " abstract";
			}
			else if (method.IsVirtual)
			{
				modifiers += method.IsOverride ? " override" : " virtual";
			}

			return modifiers.TrimStart(' ');
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

		private string FormatParameter(ParameterInfoElement parameterInfo, bool isExtensionThis)
		{
			string result = "";
			TypeElement type = parameterInfo.ParameterType;
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

		[Obsolete]
		public string Syntax(PropertyInfo property)
		{
			return Syntax(_documentation.Find(property));
		}

		public string Syntax(PropertyElement property)
		{
			var getter = property.CanRead ? property.GetMethod : null;
			var setter = property.CanWrite ? property.SetMethod : null;

			var getterAccess = getter != null ? getter.GetAccess() : MethodAttributes.PrivateScope;
			var setterAccess = setter != null ? setter.GetAccess() : MethodAttributes.PrivateScope;
			var maxAccess = ReflectionExtensions.Max(getterAccess, setterAccess);

			return Join(
				Access(maxAccess),
				Print(property.PropertyType),
				property.Name,
				"{",
				getter != null && !getter.IsPrivate ? ((getterAccess != maxAccess) ? Access(getterAccess) + " " : "") + "get;" : "",
				setter != null && !setter.IsPrivate ? ((setterAccess != maxAccess) ? Access(setterAccess) + " " : "") + "set;" : "",
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

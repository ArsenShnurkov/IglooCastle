﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	internal sealed class TypePrinter
	{
		private readonly Documentation _documentation;
		private readonly FilenameProvider _filenameProvider;

		public TypePrinter(Documentation documentation, FilenameProvider filenameProvider)
		{
			_documentation = documentation;
			_filenameProvider = filenameProvider;
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
				return string.Format("<a href=\"{0}\">{1}</a>", Escape(link), text);
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

		private string Link(ConstructorElement constructor)
		{
			if (_documentation.IsLocalType(constructor.DeclaringType))
			{
				return _filenameProvider.Filename(constructor);
			}

			return null;
		}

		private string Escape(string link)
		{
			return link.Replace("`", "%60");
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

		public string Signature(MethodElement methodElement)
		{
			string text;
			if (methodElement.IsOverload)
			{
				text = methodElement.Name + ParameterSignature(methodElement);
			}
			else
			{
				text = methodElement.Name;
			}

			return text;
		}

		public string Print(MethodElement methodInfo)
		{
			string text = Signature(methodInfo);
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

		public string Parameters<T>(MethodBaseElement<T> method, bool typeLinks = true)
			where T : MethodBase
		{
			MethodElement m = method as MethodElement;
			bool isExtension = m != null && m.IsExtension();
			string args = string.Join(
				", ",
				method.GetParameters().Select((p, index) => FormatParameter(p, isExtension && index == 0, typeLinks)));
			return args;
		}

		public string Syntax(MethodElement method, bool typeLinks = true)
		{
			string access = AccessPrefix(method);
			string modifiers = Modifiers(method);
			string returnType = Print(method.ReturnType, typeLinks);
			string args = Parameters(method, typeLinks);
			return Join(access, modifiers, returnType, method.Name).TrimStart(' ') + "(" + args + ")";
		}

		private string AccessPrefix<T>(MethodBaseElement<T> member)
			where T : MethodBase
		{
			if (member.ReflectedType.IsInterface)
			{
				return string.Empty;
			}

			MethodAttributes access = member.GetAccess();
			return access.ToAccessString();
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

			if (method.IsFinal)
			{
				modifiers += " sealed";
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

		private static string Join(params string[] args)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string s in args.Where(arg => !string.IsNullOrWhiteSpace(arg)))
			{
				if (builder.Length > 0)
				{
					builder.Append(' ');
				}

				builder.Append(s);
			}

			return builder.ToString();
		}

		private string FormatParameter(ParameterInfoElement parameterInfo, bool isExtensionThis, bool typeLinks)
		{
			string result = "";

			string @params = parameterInfo.IsParams ? "params" : "";

			string byref = null;
			TypeElement type = parameterInfo.ParameterType;
			if (type.IsByRef)
			{
				type = type.GetElementType();
				if (parameterInfo.IsOut)
				{
					byref = "out";
				}
				else
				{
					byref = "ref";
				}
			}

			string thisparam = isExtensionThis ? "this" : "";

			result = Join(
				@params,
				byref,
				thisparam,
				Print(type, typeLinks),
				parameterInfo.Name);

			return result;
		}

		public string Syntax(TypeElement type, bool typeLinks = true)
		{
			string result = Join(
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

		public string Syntax(PropertyElement property)
		{
			var getter = property.CanRead ? property.GetMethod : null;
			var setter = property.CanWrite ? property.SetMethod : null;

			var getterAccess = getter != null ? getter.GetAccess() : MethodAttributes.PrivateScope;
			var setterAccess = setter != null ? setter.GetAccess() : MethodAttributes.PrivateScope;
			var maxAccess = ReflectionExtensions.Max(getterAccess, setterAccess);

			return Join(
				maxAccess.ToAccessString(),
				Print(property.PropertyType),
				property.Name,
				"{",
				getter != null && !getter.IsPrivate ? ((getterAccess != maxAccess) ? getterAccess.ToAccessString() + " " : "") + "get;" : "",
				setter != null && !setter.IsPrivate ? ((setterAccess != maxAccess) ? setterAccess.ToAccessString() + " " : "") + "set;" : "",
				"}");
		}

		public string Print(NamespaceElement namespaceElement)
		{
			return string.Format(
				"<a href=\"{0}\">{1}</a>",
				_filenameProvider.Filename(namespaceElement),
				namespaceElement.Namespace);
		}

		public string Print(ConstructorElement constructorElement)
		{
			string text = Signature(constructorElement);
			string link = Link(constructorElement);
			if (link == null)
			{
				return text;
			}

			return string.Format("<a href=\"{0}\">{1}</a>", Escape(link), text);
		}

		public string ParameterSignature<T>(MethodBaseElement<T> methodBaseElement)
			where T : MethodBase
		{
			return "(" + string.Join(", ", methodBaseElement.GetParameters().Select(p => ShortName(p.ParameterType))) + ")";
		}

		public string Signature(ConstructorElement constructorElement)
		{
			string text = Name(constructorElement.DeclaringType, NameComponents.Name);
			if (constructorElement.DeclaringType.Constructors.Count() > 1)
			{
				text += ParameterSignature(constructorElement);
			}

			return text;
		}

		public string Syntax(ConstructorElement constructorElement, bool typeLinks = true)
		{
			string access = AccessPrefix(constructorElement);
			string args = Parameters(constructorElement, typeLinks);
			return string.Format(
				"{0} {1}({2})",
				access,
				Name(constructorElement.DeclaringType, NameComponents.Name),
				args);
		}
	}
}

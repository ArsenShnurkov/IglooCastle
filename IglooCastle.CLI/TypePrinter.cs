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

		public string Print(Type type)
		{
			if (type.IsArray)
			{
				return Print(type.GetElementType()) + "[]";
			}

			string result;
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				// concrete generic type
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				result = BeginPrint(genericTypeDefinition);
				result += string.Join(", ", type.GetGenericArguments().Select(Print));
				result += EndPrint(genericTypeDefinition);
			}
			else
			{
				result = BeginPrint(type);
				result += EndPrint(type);
			}

			return result;
		}

		public virtual string BeginPrint(Type type)
		{
			if (type.IsGenericTypeDefinition)
			{
				return type.FullName.Split('`')[0] + "&lt;";
			}

			if (_documentation.IsLocalType(type))
			{
				return string.Format("<a href=\"{0}\">{1}</a>", _filenameProvider.Filename(type), type.Name);
			}

			string alias = SystemTypes.Alias(type);
			if (alias != null)
			{
				return string.Format(
					"<a href=\"http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx\">{1}</a>",
					type.FullName.ToLowerInvariant(),
					alias);
			}

			if (type.Namespace == "System" || type.Namespace.StartsWith("System."))
			{
				return string.Format(
					"<a href=\"http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx\">{1}</a>",
					type.FullName.ToLowerInvariant(),
					type.Name);
			}

			return type.FullName ?? type.Name;
		}

		public virtual string EndPrint(Type type)
		{
			if (type.IsGenericTypeDefinition)
			{
				return "&gt;";
			}

			return string.Empty;
		}

		public string Print(MethodElement methodElement)
		{
			return Print(methodElement.Member);
		}

		public string Print(MethodInfo methodInfo)
		{
			return methodInfo.Name + "(" + string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.Name)) + ")";
		}
	}
}

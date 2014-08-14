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
			return Print(type, null);
		}

		public string Print(Type type, PrintOptions printOptions)
		{
			if (type.IsArray)
			{
				return Print(type.GetElementType(), printOptions) + "[]";
			}

			string result;
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				// concrete generic type
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				result = BeginPrint(genericTypeDefinition, printOptions);
				result += string.Join(", ", type.GetGenericArguments().Select(t => Print(t, printOptions)));
				result += EndPrint(genericTypeDefinition, printOptions);
			}
			else
			{
				result = BeginPrint(type, printOptions);
				result += EndPrint(type, printOptions);
			}

			return result;
		}

		public virtual string BeginPrint(Type type, PrintOptions printOptions)
		{
			printOptions = printOptions ?? PrintOptions.Default;
			if (type.IsGenericTypeDefinition)
			{
				return type.FullName.Split('`')[0] + "&lt;";
			}

			if (_documentation.IsLocalType(type))
			{
				if (printOptions.Links)
				{
					return string.Format("<a href=\"{0}\">{1}</a>", _filenameProvider.Filename(type), type.Name);
				}
				else
				{
					if (printOptions.ShortName)
					{
						return type.Name;
					}
					else
					{
						return type.FullName;
					}
				}
			}

			string alias = SystemTypes.Alias(type);
			if (alias != null)
			{
				if (printOptions.Links)
				{
					return string.Format(
						"<a href=\"http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx\">{1}</a>",
						type.FullName.ToLowerInvariant(),
						alias);
				}
				else
				{
					return alias;
				}
			}

			if (type.Namespace == "System" || type.Namespace.StartsWith("System."))
			{
				if (printOptions.Links)
				{
					return string.Format(
						"<a href=\"http://msdn.microsoft.com/en-us/library/{0}%28v=vs.110%29.aspx\">{1}</a>",
						type.FullName.ToLowerInvariant(),
						type.Name);
				}
				else
				{
					if (printOptions.ShortName)
					{
						return type.Name;
					}
					else
					{
						return type.FullName;
					}
				}
			}

			return type.FullName ?? type.Name;
		}

		public virtual string EndPrint(Type type, PrintOptions printOptions)
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

		public string Print(MethodElement methodElement, PrintOptions printOptions)
		{
			return Print(methodElement.Member, printOptions);
		}

		public string Print(MethodInfo methodInfo)
		{
			return Print(methodInfo, PrintOptions.MethodOverload);
		}

		public string Print(MethodInfo methodInfo, PrintOptions printOptions)
		{
			return methodInfo.Name + "(" + string.Join(",", methodInfo.GetParameters().Select(p => Print(p.ParameterType, printOptions))) + ")";
		}

		public sealed class PrintOptions
		{
			public static readonly PrintOptions MethodOverload = new PrintOptions { Links = false, ShortName = true };
			public static readonly PrintOptions Default = new PrintOptions { Links = true, ShortName = false };
			public bool Links { get; set; }
			public bool ShortName { get; set; }
		}
	}
}

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
			return Print(type, null);
		}

		public string Print(TypeElement typeElement, PrintOptions printOptions)
		{
			return Print(typeElement.Member, printOptions);
		}

		public string Print(Type type, PrintOptions printOptions)
		{
			if (type.IsArray)
			{
				return Print(type.GetElementType(), printOptions) + "[]";
			}

			string result = DoPrint(type, printOptions);

			if (type.IsGenericType)
			{
				result += "&lt;";
				result += string.Join(", ", type.GetGenericArguments().Select(t => Print(t, printOptions)));
				result += "&gt;";
			}

			return result;
		}

		private string DoPrint(Type type, PrintOptions printOptions)
		{
			printOptions = printOptions ?? PrintOptions.Default;
			string link = printOptions.Links ? Link(type) : null;
			string text = link != null || printOptions.ShortName ? ShortName(type) : FullName(type);
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

			return type.FullName ?? ShortName(type);
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

		private string Escape(string link)
		{
			return link.Replace("`", "%60");
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
			return methodInfo.Name + "(" + string.Join(", ", methodInfo.GetParameters().Select(p => Print(p.ParameterType, printOptions))) + ")";
		}

		public sealed class PrintOptions
		{
			public static readonly PrintOptions MethodOverload = new PrintOptions { Links = false, ShortName = true };
			public static readonly PrintOptions Default = new PrintOptions { Links = true, ShortName = false };
			public bool Links { get; set; }
			public bool ShortName { get; set; }
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
	}
}

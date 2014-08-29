using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
	public sealed class FilenameProvider
	{
		public string Filename(NamespaceElement @namespace, string prefix = "N", string suffix = "html")
		{
			return string.Format("{0}_{1}.{2}", prefix, @namespace.Namespace, suffix);
		}

		public string Filename(TypeElement type, string prefix = "T", string suffix = ".html")
		{
			return Filename(type.Member, prefix, suffix);
		}

		[Obsolete]
		public string Filename(Type type, string prefix = "T", string suffix = ".html")
		{
			return string.Format("{0}_{1}{2}", prefix, FilenamePartForMainType(type), suffix);
		}

		public string Filename(ConstructorElement constructorElement)
		{
			string parameters = string.Join(",", constructorElement.GetParameters().Select(p => FilenamePartForParameter(p.ParameterType)));
			string suffix = string.IsNullOrEmpty(parameters) ? string.Empty : "-" + parameters;
			var result = Filename(constructorElement.DeclaringType, "C", string.Format("{0}.html", suffix));
			return result;
		}

		public string Filename(PropertyElement property)
		{
			return Filename(property.Member);
		}

		[Obsolete]
		public string Filename(PropertyInfo property)
		{
			return Filename(property.DeclaringType, "P", string.Format(".{0}.html", property.Name));
		}

		public string Filename(MethodElement method)
		{
			string parameters = string.Join(",", method.GetParameters().Select(p => FilenamePartForParameter(p.ParameterType)));
			var result = Filename(method.DeclaringType, "M", string.Format(".{0}{1}.html", method.Name, string.IsNullOrEmpty(parameters) ? string.Empty : "-" + parameters));
			return result;
		}

		[Obsolete]
		private string FilenamePartForMainType(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}

			return type.FullName;
		}

		private string FilenamePartForParameter(TypeElement parameterType)
		{
			if (parameterType.IsGenericType)
			{
				TypeElement[] genericArguments = parameterType.GetGenericArguments();
				// FIX: do not use Member here
				string genericType = parameterType.GetGenericTypeDefinition().Member.FullName.Split('`')[0];
				return genericType + "`" + string.Join(",", genericArguments.Select(FilenamePartForParameter));
			}
			else
			{
				// FIX: do not use Member here
				return SystemTypes.Alias(parameterType) ?? parameterType.Member.FullName;
			}
		}
	}
}

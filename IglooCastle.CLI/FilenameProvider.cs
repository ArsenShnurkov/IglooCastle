using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
	public class FilenameProvider
	{
		public string Filename(NamespaceElement @namespace, string prefix = "N", string suffix = "html")
		{
			return string.Format("{0}_{1}.{2}", prefix, @namespace.Namespace, suffix);
		}

		public string Filename(TypeElement type, string prefix = "T", string suffix = "html")
		{
			return Filename(type.Member, prefix, suffix);
		}

		public string Filename(Type type, string prefix = "T", string suffix = "html")
		{
			return string.Format("{0}_{1}.{2}", prefix, FilenamePartForMainType(type), suffix);
		}

		public string Filename(PropertyElement property)
		{
			return Filename(property.Member);
		}

		public string Filename(PropertyInfo property)
		{
			return Filename(property.DeclaringType, "P", string.Format("{0}.html", property.Name));
		}

		public string Filename(MethodElement method)
		{
			return Filename(method.Member);
		}

		public string Filename(MethodInfo method)
		{
			string parameters = string.Join(",", method.GetParameters().Select(p => FilenamePartForParameter(p.ParameterType)));
			var result = Filename(method.DeclaringType, "M", string.Format("{0}{1}.html", method.Name, string.IsNullOrEmpty(parameters) ? string.Empty : "-" + parameters));
			return result;
		}

		private string FilenamePartForMainType(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}

			return type.FullName;
		}

		private string FilenamePartForParameter(Type parameterType)
		{
			if (parameterType.IsGenericType)
			{
				Type[] genericArguments = parameterType.GetGenericArguments();
				string genericType = parameterType.GetGenericTypeDefinition().FullName.Split('`')[0];
				return genericType + "`" + string.Join(",", genericArguments.Select(FilenamePartForParameter));
			}
			else
			{
				return SystemTypes.Alias(parameterType) ?? parameterType.FullName;
			}
		}
	}
}

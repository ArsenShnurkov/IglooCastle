using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
	public class FilenameProvider
	{
		public string Filename(NamespaceElement @namespace)
		{
			return string.Format("N_{0}.html", @namespace.Namespace);
		}

		public string Filename(NamespaceElement @namespace, string prefix)
		{
			return string.Format("{0}_{1}", prefix, Filename(@namespace));
		}

		public string Filename(TypeElement type)
		{
			return Filename(type, "T");
		}

		public string Filename(Type type)
		{
			return Filename(type, "T");
		}

		public string Filename(TypeElement type, string prefix)
		{
			return string.Format("{0}_{1}.html", prefix, TypeFilename(type));
		}

		public string Filename(Type type, string prefix)
		{
			return string.Format("{0}_{1}.html", prefix, TypeFilename(type));
		}

		public string Filename(PropertyElement property)
		{
			return string.Format("P_{0}.{1}.html", TypeFilename(property.Property.DeclaringType), property.Name);
		}

		public string Filename(MethodElement method)
		{
			string parameters = string.Join(",", method.Method.GetParameters().Select(p => FilenamePartForParameter(p.ParameterType)));
			var result = string.Format("M_{0}.{1}{2}.html", TypeFilename(method.Method.DeclaringType), method.Name, string.IsNullOrEmpty(parameters) ? string.Empty : "-" + parameters);
			return result;
		}

		public string FilenamePartForParameter(Type parameterType)
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

		public string TypeFilename(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}

			var result = type.FullName;
			//var result = type.FullName.Replace('`', '_');
			return result;
		}

		public string TypeFilename(TypeElement type)
		{
			return TypeFilename(type.Type);
		}
	}
}

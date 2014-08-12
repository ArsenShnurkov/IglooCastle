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
			string parameters = string.Join("_", method.Method.GetParameters().Select(p => FilenamePartForParameter(p.ParameterType)));

			/*
* # BUG:
# Writing file M_IglooCastle.CLI.TypeContainerExtensions.FilterTypes-IglooCastle.C
# LI.ITypeContainer_System.Predicate`1[[IglooCastle.CLI.TypeElement, IglooCastle.C
# LI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].html
*/

			var result = string.Format("M_{0}.{1}{2}", TypeFilename(method.Method.DeclaringType), method.Name, string.IsNullOrEmpty(parameters) ? string.Empty : "-" + parameters);
			// BUG
			if (result.Length > 80)
			{
				result = result.Substring(0, 80);
			}

			result += ".html";

			return result;
		}

		public string FilenamePartForParameter(Type parameterType)
		{
			return SystemTypes.Alias(parameterType) ?? parameterType.FullName;
		}

		public string TypeFilename(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}

			var result = type.FullName.Replace('`', '_');
			return result;
		}

		public string TypeFilename(TypeElement type)
		{
			return TypeFilename(type.Type);
		}
	}
}

using System;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace IglooCastle.CLI
{
	public class Documentation
	{
		private NamespaceElement[] _namespaces = new NamespaceElement[0];
		private TypeElement[] _types = new TypeElement[0];
		private XmlDocument[] _documentationSources = new XmlDocument[0];

		public NamespaceElement[] Namespaces
		{
			get { return _namespaces; }
			set { _namespaces = value ?? new NamespaceElement[0]; }
		}

		public TypeElement[] Types
		{
			get { return _types; }
			set { _types = value ?? new TypeElement[0]; }
		}

		public XmlDocument[] DocumentationSources
		{
			get { return _documentationSources; }
			set { _documentationSources = value ?? new XmlDocument[0]; }
		}

		public Documentation Merge(Documentation that)
		{
			return new Documentation
				{
					Namespaces = Namespaces.Union(that.Namespaces).ToArray(),
					Types = Types.Union(that.Types).ToArray(),
					DocumentationSources = DocumentationSources.Union(that.DocumentationSources).ToArray()
				};
		}

		public bool IsLocalType(Type type)
		{
			Type normalizedType = Normalize(type);
			if (normalizedType.IsGenericParameter)
			{
				return false;
			}

			return Types.Any(t => t.Type == normalizedType);
		}

		private Type Normalize(Type type)
		{
			if (type.ContainsGenericParameters && type.IsGenericType)
			{
				return type.GetGenericTypeDefinition();
			}

			return type;
		}

		internal IXmlComment GetXmlComment<T>(ReflectedElement<T> reflectedElement)
			where T : MemberInfo
		{
			return GetXmlComment(reflectedElement as ConstructorElement)
			       ?? GetXmlComment(reflectedElement as PropertyElement)
			       ?? GetXmlComment(reflectedElement as MethodElement)
				   ?? GetXmlComment(reflectedElement as TypeElement);
		}

		private IXmlComment GetXmlComment(ConstructorElement constructorElement)
		{
			// M:IglooCastle.CLI.NamespaceElement.#ctor(IglooCastle.CLI.Documentation)
			return constructorElement != null
				       ? GetMethodDocumentation(constructorElement.OwnerType.Type, "#ctor", constructorElement.Constructor.GetParameters())
				       : null;
		}

		private XmlComment GetMethodDocumentation(Type type, string methodName, ParameterInfo[] parameters)
		{
			string paramString = string.Join(",", parameters.Select(p => p.ParameterType.FullName));
			string attributeValue = type.FullName + "." + methodName + "(" + paramString + ")";
			return GetXmlComment("//member[@name=\"M:" + attributeValue + "\"]");
		}

		private IXmlComment GetXmlComment(PropertyElement propertyElement)
		{
			return propertyElement != null
				       ? GetXmlComment(
						   "//member[@name=\"P:" + propertyElement.Property.ReflectedType.FullName + "." + propertyElement.Property.Name + "\"]")
				       : null;
		}

		private IXmlComment GetXmlComment(MethodElement methodElement)
		{
			return methodElement != null
				       ? GetMethodDocumentation(methodElement.OwnerType.Type, methodElement.Method.Name,
				                                methodElement.Method.GetParameters())
				       : null;
		}

		private IXmlComment GetXmlComment(TypeElement typeElement)
		{
			return typeElement != null ? GetXmlComment("//member[@name=\"T:" + typeElement.Type.FullName + "\"]") : null;
		}

		private XmlComment GetXmlComment(string selector)
		{
			return DocumentationSources.Select(xmlDoc => GetXmlComment(xmlDoc, selector)).FirstOrDefault(c => c != null);
		}

		private static XmlComment GetXmlComment(XmlDocument doc, string selector)
		{
			if (doc == null)
			{
				return null;
			}

			XmlNode node = doc.SelectSingleNode(selector);
			if (node == null)
			{
				return null;
			}

			return new XmlComment((XmlElement)node);
		}
	}
}

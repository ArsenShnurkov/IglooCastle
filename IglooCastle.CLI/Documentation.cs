﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace IglooCastle.CLI
{
	public class Documentation : ITypeContainer
	{
		private NamespaceElement[] _namespaces = new NamespaceElement[0];
		private TypeElement[] _types = new TypeElement[0];
		private readonly List<XmlDocument> _documentationSources = new List<XmlDocument>();

		public Documentation()
		{
			FilenameProvider = new FilenameProvider();
			TypePrinter = new TypePrinter(this, FilenameProvider);
		}

		public FilenameProvider FilenameProvider { get; set; }
		public TypePrinter TypePrinter { get; set; }

		public NamespaceElement[] Namespaces
		{
			get { return _namespaces; }
			set { _namespaces = value ?? new NamespaceElement[0]; }
		}

		public ICollection<TypeElement> Types
		{
			get { return _types; }
			set { _types = value != null ? value.ToArray() : new TypeElement[0]; }
		}

		public ICollection<XmlDocument> DocumentationSources
		{
			get
			{
				return _documentationSources;
			}

			private set 
			{
				_documentationSources.Clear();
				_documentationSources.AddRange(value ?? Enumerable.Empty<XmlDocument>());
			}
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

		public void Scan(Assembly assembly)
		{
			HashSet<string> namespaces = new HashSet<string>();
			List<TypeElement> types = new List<TypeElement>();
			foreach (Type type in assembly.GetTypes().Where(t => t.IsVisible))
			{
				namespaces.Add(type.Namespace ?? string.Empty);
				types.Add(new TypeElement(this, type));
			}

			Namespaces = namespaces.Select(n => new NamespaceElement(this, n)).ToArray();
			Types = types.ToArray();

			Console.WriteLine(Environment.CurrentDirectory);
			Console.WriteLine(assembly.Location);
		}

		public bool IsLocalType(TypeElement type)
		{
			return true;
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

		/// <summary>
		/// Normalizes the given type element.
		/// </summary>
		public TypeElement Normalize(TypeElement type)
		{
			return type;
		}

		/// <summary>
		/// Normalizes the given type.
		/// </summary>
		public Type Normalize(Type type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
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
			IXmlComment result = null;

			while (propertyElement != null && result == null)
			{
				result = GetXmlComment("//member[@name=\"P:" +
					propertyElement.Property.ReflectedType.FullName + "." +
					propertyElement.Property.Name + "\"]");

				if (result == null)
				{
					// try base type?
					propertyElement = propertyElement.BasePropertyElement();
				}
			}

			return result;
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

		public bool AddDocumentation(Assembly assembly)
		{
			return AddDocumentationFromAssemblyFile(assembly, assembly.Location);
		}

		public bool AddDocumentationFromAssemblyFile(Assembly assembly, string assemblyFile)
		{
			return AddDocumentationFromXmlFile(assembly, Path.ChangeExtension(assemblyFile, "xml"))
				|| AddDocumentationFromXmlFile(assembly, Path.ChangeExtension(assemblyFile, "XML"));
		}

		private bool AddDocumentationFromXmlFile(Assembly assembly, string xmlFile)
		{
			if (!File.Exists(xmlFile))
			{
				Console.WriteLine("Could not find xml file {0}", xmlFile);
				return false;
			}

			XmlDocument doc = new XmlDocument();
			doc.Load(xmlFile);
			DocumentationSources.Add(doc);
			return true;
		}
	}
}

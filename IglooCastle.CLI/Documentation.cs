using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace IglooCastle.CLI
{
	/// <summary>
	/// Represents the documentation of one or more assemblies.
	/// </summary>
	public class Documentation : ITypeContainer
	{
		private NamespaceElement[] _namespaces = new NamespaceElement[0];
		private TypeElement[] _types = new TypeElement[0];
		private readonly List<XmlDocument> _documentationSources = new List<XmlDocument>();

		/// <summary>
		/// Creates an instance of this class.
		/// </summary>
		/// <remarks>
		/// The <see cref="FilenameProvider"/> and <see cref="TypePrinter"/> are set to default values.
		/// </remarks>
		public Documentation()
		{
			FilenameProvider = new FilenameProvider();
			TypePrinter = new TypePrinter(this, FilenameProvider);
		}

		/// <summary>
		/// Gets or sets the filename provider.
		/// </summary>
		internal FilenameProvider FilenameProvider { get; set; }

		/// <summary>
		/// Gets or sets the type printer.
		/// </summary>
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
			var normalizedType = Normalize(type);
			if (normalizedType.IsGenericParameter)
			{
				return false;
			}

			return Types.Any(t => t.Type == normalizedType.Type);
		}

		/// <summary>
		/// Normalizes the given type element.
		/// </summary>
		public TypeElement Normalize(TypeElement type)
		{
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				return type.GetGenericTypeDefinition();
			}

			return type;
		}

		internal XmlComment GetMethodDocumentation(Type type, string methodName, ParameterInfo[] parameters)
		{
			string paramString = string.Join(",", parameters.Select(p => p.ParameterType.FullName));
			string attributeValue = type.FullName + "." + methodName + "(" + paramString + ")";
			return GetXmlComment("//member[@name=\"M:" + attributeValue + "\"]");
		}

		internal XmlComment GetXmlComment(string selector)
		{
			return DocumentationSources.Select(xmlDoc => GetXmlComment(xmlDoc, selector)).FirstOrDefault(c => c != null);
		}

		private XmlComment GetXmlComment(XmlDocument doc, string selector)
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

			return new XmlComment(this, (XmlElement)node);
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

		internal MethodElement Find(MethodInfo method)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method", "Method cannot be null");
			}

			var type = Find(method.ReflectedType);
			return type.Find(method);
		}

		internal PropertyElement Find(PropertyInfo propertyInfo)
		{
			return Types.SelectMany(t => t.Properties).SingleOrDefault(p => p.Property == propertyInfo);
		}

		public TypeElement Find(Type type)
		{
			return Types.FirstOrDefault(t => t.Type == type) ?? new ExternalTypeElement(this, type);
		}
	}
}

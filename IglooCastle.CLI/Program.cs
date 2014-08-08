using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IronPython.Hosting;
using System.Xml;

namespace IglooCastle.CLI
{
	class Program
	{
		/// <summary>
		/// For every assembly that is loaded, its path is added to this list.
		/// When attempting to resolve an assembly all these paths will be examined.
		/// </summary>
		private readonly List<string> _possibleAssemblyPaths = new List<string>();

		static void Main(string[] args)
		{
			Program p = new Program();
			p.AddAssemblyResolver();
			Documentation documentation = new Documentation();

#if DEBUG
			if (args.Length == 0)
			{
				args = new string[]
					{
						Assembly.GetExecutingAssembly().Location
					};
			}
#endif

			documentation = args.Aggregate(documentation, (current, arg) => current.Merge(p.ProcessAssembly(arg)));
			p.RunGenerator(documentation);
		}

		private void AddAssemblyResolver()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}
		
		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return (
				from p in _possibleAssemblyPaths
				let fullPath = Path.Combine(p, args.Name) + ".dll"
				where File.Exists(fullPath)
				select Assembly.LoadFrom(fullPath)
			).FirstOrDefault();
		}

		private void RunGenerator(Documentation documentation)
		{
			var ipy = Python.CreateRuntime();
			dynamic generator = ipy.UseFile("generator.py");
			generator.Generate(documentation);
		}

		private Documentation ProcessAssembly(string file)
		{
			Console.WriteLine("Processing assembly {0}", file);

			try
			{
				string fullPath = Path.GetFullPath(file);
				XmlDocument xmlDoc = FindMatchingDocumentation(fullPath);
				_possibleAssemblyPaths.Add(Path.GetDirectoryName(fullPath));
				Assembly assembly = Assembly.LoadFrom(fullPath);
				Documentation documentation = new Documentation();
				HashSet<string> namespaces = new HashSet<string>();
				List<TypeElement> types = new List<TypeElement>();
				foreach (Type type in assembly.GetTypes().Where(t => t.IsVisible))
				{
					namespaces.Add(type.Namespace ?? string.Empty);
					types.Add(new TypeElement
						{
							Type = type,
							XmlComment = GetTypeDocumentation(type, xmlDoc),
							Properties =
								type.GetProperties()
								    .Select(p => new PropertyElement {Property = p, XmlComment = GetPropertyDocumentation(p, xmlDoc)})
								    .ToArray(),
							Methods = type.GetMethods().Where(m => !m.IsSpecialName).Select(m => new MethodElement {Method = m}).ToArray()
						});
					Console.WriteLine("Processing type {0}", type);
				}

				documentation.Namespaces = namespaces.Select(n => new NamespaceElement(documentation) { Namespace = n }).ToArray();
				documentation.Types = types.ToArray();
				return documentation;
			}
			catch (ReflectionTypeLoadException ex)
			{
				Console.WriteLine("Could not load assembly {0}", file);
				foreach (Exception loaderEx in ex.LoaderExceptions)
				{

					Console.WriteLine(loaderEx);
				}

				throw;
			}
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

			return new XmlComment((XmlElement)node);
		}

		private XmlComment GetTypeDocumentation(Type t, XmlDocument doc)
		{
			return GetXmlComment(doc, "//member[@name=\"T:" + t.FullName + "\"]");
		}

		private XmlComment GetPropertyDocumentation(PropertyInfo pi, XmlDocument doc)
		{
			return GetXmlComment(
				doc,
				"//member[@name=\"P:" + pi.ReflectedType.FullName + "." + pi.Name + "\"]");
		}

		private XmlDocument FindMatchingDocumentation(string file)
		{
			string xmlFile = Path.ChangeExtension(file, "xml");
			if (!File.Exists(xmlFile))
			{
				Console.WriteLine("Could not find matching xml file {0}", xmlFile);
				return null;
			}

			XmlDocument doc = new XmlDocument();
			doc.Load(xmlFile);
			return doc;
		}
	}
}

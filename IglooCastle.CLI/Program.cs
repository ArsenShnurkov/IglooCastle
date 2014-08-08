using IronPython.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
				_possibleAssemblyPaths.Add(Path.GetDirectoryName(fullPath));
				Assembly assembly = Assembly.LoadFrom(fullPath);
				Documentation documentation = new Documentation
					{
						DocumentationSources = new[]
							{
								FindMatchingDocumentation(fullPath)
							}
					};

				HashSet<string> namespaces = new HashSet<string>();
				List<TypeElement> types = new List<TypeElement>();
				foreach (Type type in assembly.GetTypes().Where(t => t.IsVisible))
				{
					namespaces.Add(type.Namespace ?? string.Empty);
					types.Add(new TypeElement(documentation, type));
				}

				documentation.Namespaces = namespaces.Select(n => new NamespaceElement(documentation, n)).ToArray();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;

namespace IglooCastle.CLI
{
	class Program
	{
		static void Main(string[] args)
		{
			Program p = new Program();
			Documentation documentation = new Documentation();
			documentation = args.Aggregate(documentation, (current, arg) => current.Merge(p.ProcessAssembly(arg)));
			p.RunGenerator(documentation);
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
			string fullPath = Path.GetFullPath(file);
			Assembly assembly = Assembly.LoadFile(fullPath);
			Documentation documentation = new Documentation();
			HashSet<string> namespaces = new HashSet<string>();
			List<string> types = new List<string>();
			foreach (Type type in assembly.GetTypes())
			{
				namespaces.Add(type.Namespace ?? string.Empty);
				types.Add(type.FullName);
				Console.WriteLine("Processing type {0}", type);
				foreach (MemberInfo member in type.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				{
					Console.WriteLine("Processing member {0}", member);
					Console.WriteLine("Declaring type: {0}", member.DeclaringType);
				}
			}

			documentation.Namespaces = namespaces.ToArray();
			documentation.Types = types.ToArray();
			return documentation;
		}

		protected bool Test { get; set; }
		public override string ToString()
		{
			return base.ToString();
		}
	}
}

using System;
using System.Linq;

namespace IglooCastle.CLI
{
	public class Options
	{
		private Options()
		{
		}

		public static Options Parse(string[] args)
		{
			return new Options
			{
				InputAssemblies = args.Where(a => !string.IsNullOrEmpty(a) && !a.StartsWith("-")).ToArray(),
				OutputDirectory = Find(args, "--output=")
			};
		}

		public string[] InputAssemblies
		{
			get;
			set;
		}

		public string OutputDirectory
		{
			get;
			set;
		}

		private static string Find(string[] args, string arg)
		{
			string value = args.FirstOrDefault(a => a.StartsWith(arg));
			if (value != null)
			{
				return value.Substring(arg.Length).Trim();
			}

			return null;
		}
	}
}

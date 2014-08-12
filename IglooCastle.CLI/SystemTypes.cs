using System;
using System.Collections.Generic;

namespace IglooCastle.CLI
{
	public static class SystemTypes
	{
		private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>
			{
				{ typeof(string), "string" },
				{ typeof(bool), "bool" },
				{ typeof(int), "int" },
				{ typeof(void), "void"},
				{ typeof(object), "object" }
			};

		public static string Alias(Type type)
		{
			return Aliases.ContainsKey(type) ? Aliases[type] : null;
		}

		public static string Alias(TypeElement type)
		{
			return Alias(type.Member);
		}
	}
}
using System;
using System.Linq;

namespace IglooCastle.CLI
{
	[Serializable]
	public class Documentation
	{
		private string[] _namespaces = new string[0];
		private TypeElement[] _types = new TypeElement[0];

		public string[] Namespaces
		{
			get { return _namespaces; }
			set { _namespaces = value ?? new string[0]; }
		}

		public TypeElement[] Types
		{
			get { return _types; }
			set { _types = value ?? new TypeElement[0]; }
		}

		public Documentation Merge(Documentation that)
		{
			return new Documentation
				{
					Namespaces = Namespaces.Union(that.Namespaces).ToArray(),
					Types = Types.Union(that.Types).ToArray()
				};
		}
	}
}
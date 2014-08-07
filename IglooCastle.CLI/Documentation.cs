using System;
using System.Linq;

namespace IglooCastle.CLI
{
	public class Documentation
	{
		private NamespaceElement[] _namespaces = new NamespaceElement[0];
		private TypeElement[] _types = new TypeElement[0];

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

		public Documentation Merge(Documentation that)
		{
			return new Documentation
				{
					Namespaces = Namespaces.Union(that.Namespaces).ToArray(),
					Types = Types.Union(that.Types).ToArray()
				};
		}

		public bool IsLocalType(Type type)
		{
			return Types.Any(t => t.Type == Normalize(type));
		}

		public Type Normalize(Type type)
		{
			if (type.ContainsGenericParameters || type.IsGenericType)
			{
				return type.GetGenericTypeDefinition();
			}

			return type;
		}

		public string TypeFullName(Type type)
		{
			return Normalize(type).FullName;
		}
	}
}
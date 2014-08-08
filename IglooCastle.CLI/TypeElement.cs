using System;
using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.CLI
{
	public class TypeElement : ReflectedElement<Type>
	{
		private ConstructorElement[] _constructors = new ConstructorElement[0];
		private PropertyElement[] _properties = new PropertyElement[0];
		private MethodElement[] _methods = new MethodElement[0];

		public Type Type
		{
			get { return Member; }
			set { Member = value; }
		}

		public ConstructorElement[] Constructors
		{
			get { return _constructors; }
			set { _constructors = value ?? new ConstructorElement[0]; }
		}

		public IEnumerable<PropertyElement> Properties
		{
			get { return _properties.OrderBy(p => p.Property.Name); }
			set { _properties = (value ?? Enumerable.Empty<PropertyElement>()).ToArray(); }
		}

		public MethodElement[] Methods
		{
			get { return _methods; }
			set { _methods = value ?? new MethodElement[0]; }
		}
	}
}

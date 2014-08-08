using System;

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

		public PropertyElement[] Properties
		{
			get { return _properties; }
			set { _properties = value ?? new PropertyElement[0]; }
		}

		public MethodElement[] Methods
		{
			get { return _methods; }
			set { _methods = value ?? new MethodElement[0]; }
		}
	}
}

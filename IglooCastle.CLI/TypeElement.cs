using System;

namespace IglooCastle.CLI
{
	public class TypeElement : DocumentationElement
	{
		private PropertyElement[] _properties = new PropertyElement[0];
		private MethodElement[] _methods = new MethodElement[0];

		public Type Type { get; set; }

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
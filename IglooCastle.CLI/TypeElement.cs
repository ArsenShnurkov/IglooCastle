using System;
using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.CLI
{
	public class TypeElement : ReflectedElement<Type>
	{
		public TypeElement(Documentation owner, Type type)
			: base(owner, type)
		{
		}

		public Type BaseType
		{
			get { return Member.BaseType; }
		}

		public Type Type
		{
			get { return Member; }
		}

		public ICollection<ConstructorElement> Constructors
		{
			get
			{
				return Type.GetConstructors()
						   .Select(c => new ConstructorElement(Documentation, this, c))
						   .ToList();
			}
		}

		public ICollection<PropertyElement> Properties
		{
			get
			{
				return Type.GetProperties()
					.OrderBy(p => p.Name)
					.Select(p => new PropertyElement(Documentation, this, p))
					.ToList();
			}
		}

		public ICollection<MethodElement> Methods
		{
			get
			{
				return Type.GetMethods()
						   .Where(m => !m.IsSpecialName)
						   .OrderBy(m => m.Name)
						   .Select(m => new MethodElement(Documentation, this, m))
						   .ToList();
			}
		}
	}
}

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

		public bool IsArray
		{
			get { return Type.IsArray; }
		}

		public bool IsGenericType
		{
			get { return Type.IsGenericType; }
		}

		public bool IsGenericParameter
		{
			get { return Type.IsGenericParameter; }
		}

		public bool ContainsGenericParameters
		{
			get { return Type.ContainsGenericParameters; }
		}

		public bool IsGenericTypeDefinition
		{
			get { return Type.IsGenericTypeDefinition; }
		}

		public string FullName
		{
			get { return Type.FullName; }
		}

		public Type[] GetInterfaces()
		{
			return Member.GetInterfaces();
		}

		public ICollection<TypeElement> GetDerivedTypes()
		{
			return Documentation.Types.Where(t => t.Type != this.Type && Type.IsAssignableFrom(t.Type) ).ToList();
		}

		public Type[] GetGenericArguments()
		{
			return Member.GetGenericArguments();
		}
	}
}

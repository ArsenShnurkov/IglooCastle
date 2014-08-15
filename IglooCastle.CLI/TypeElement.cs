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

		public override NamespaceElement NamespaceElement
		{
			get { return Documentation.Namespaces.Single(n => n.Namespace == Member.Namespace); }
		}

		internal Type Type
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

		public Type[] GetInterfaces()
		{
			return Member.GetInterfaces();
		}

		public ICollection<TypeElement> GetDerivedTypes()
		{
			return Documentation.Types.Where(t =>
				(
					t.Type != this.Type && Type.IsAssignableFrom(t.Type))
					||
					t.HasBaseType(this)
				).ToList();
		}

		public Type[] GetGenericArguments()
		{
			return Member.GetGenericArguments();
		}

		public TypeElement BaseTypeElement
		{
			get
			{
				Type baseType = Type.BaseType;
				if (baseType == null)
				{
					return null;
				}

				baseType = Documentation.Normalize(baseType);
				return Documentation.Types.FirstOrDefault(t => t.Type == baseType);
			}
		}

		public bool HasBaseType(TypeElement typeElement)
		{
			if (typeElement == null)
			{
				return false;
			}

			TypeElement baseTypeElement = BaseTypeElement;
			while (baseTypeElement != null && baseTypeElement.Type != typeElement.Type)
			{
				baseTypeElement = baseTypeElement.BaseTypeElement;
			}

			return baseTypeElement != null;
		}
	}
}

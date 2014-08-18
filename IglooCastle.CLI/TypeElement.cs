using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
				return Type.GetPublicAndProtectedProperties()
					.OrderBy(p => p.Name)
					.Select(p => new PropertyElement(Documentation, this, p))
					.ToList();
			}
		}

		public ICollection<MethodElement> Methods
		{
			get
			{
				return Type.GetPublicAndProtectedMethods()
						   .OrderBy(m => m.Name)
						   .Select(m => new MethodElement(Documentation, this, m))
						   .ToList();
			}
		}

		public ICollection<MethodElement> ExtensionMethods
		{
			get
			{
				return Documentation.Types.SelectMany(t => t.Methods).Where(m => m.IsExtension() && m.GetParameters()[0].ParameterType.IsAssignableFrom(Member)).ToList();
			}
		}

		public ICollection<EnumMemberElement> EnumMembers
		{
			get
			{
				return Type.IsEnum ? 
					Enum.GetNames(Type).Select(n => new EnumMemberElement(Documentation, this, n)).ToList()
					: new List<EnumMemberElement>(0);
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

		public string ShortName
		{
			get
			{
				return Documentation.TypePrinter.Name(this, TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments);
			}
		}

		public string FullName
		{
			get
			{
				return Documentation.TypePrinter.Name(this, TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments | TypePrinter.NameComponents.Namespace);
			}
		}

		public string TypeKind
		{
			get
			{
				if (Type.IsEnum)
				{
					return "Enumeration";
				}

				if (Type.IsValueType)
				{
					return "Struct";
				}

				if (Type.IsInterface)
				{
					return "Interface";
				}

				if (Type.IsClass)
				{
					return "Class";
				}

				// what else?
				return "Type";
			}
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetXmlComment("//member[@name=\"T:" + Type.FullName + "\"]");
		}
	}
}

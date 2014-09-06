using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	/// <summary>
	/// Encapsulates a <see cref="Type"/>.
	/// </summary>
	public class TypeElement : ReflectedElement<Type>, IFormattable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IglooCastle.CLI.TypeElement"/> class.
		/// </summary>
		/// <param name="owner">The documentation that owns this instance.</param>
		/// <param name="type">The type that this instance represents.</param>
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

		#region Unchanged properties of contained type, exposed.

		/// <summary>
		/// Gets the assembly.
		/// </summary>
		/// <value>The assembly.</value>
		/// <seealso cref="System.Type.Assembly"/>
		public Assembly Assembly
		{
			get { return Member.Assembly; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is enum.
		/// </summary>
		/// <value><c>true</c> if this instance is enum; otherwise, <c>false</c>.</value>
		/// <seealso cref="System.Type.IsEnum"/>
		public bool IsEnum
		{
			get { return Member.IsEnum; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is class.
		/// </summary>
		/// <value><c>true</c> if this instance is class; otherwise, <c>false</c>.</value>
		/// <seealso cref="System.Type.IsClass"/>
		public bool IsClass
		{
			get { return Member.IsClass; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is interface.
		/// </summary>
		/// <value><c>true</c> if this instance is interface; otherwise, <c>false</c>.</value>
		/// <seealso cref="System.Type.IsInterface"/>
		public bool IsInterface
		{
			get { return Member.IsInterface; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is generic type.
		/// </summary>
		/// <value><c>true</c> if this instance is generic type; otherwise, <c>false</c>.</value>
		/// <seealso cref="System.Type.IsGenericType"/>
		public bool IsGenericType
		{
			get { return Member.IsGenericType; }
		}

		public string Namespace { get { return Member.Namespace; } }

		/// <summary>
		/// Gets a value indicating whether this instance is array.
		/// </summary>
		/// <value><c>true</c> if this instance is array; otherwise, <c>false</c>.</value>
		/// <seealso cref="System.Type.IsArray"/>
		public bool IsArray { get { return Member.IsArray; } }

		public bool IsByRef { get { return Member.IsByRef; } }

		/// <summary>
		/// Gets a value indicating whether this instance is abstract.
		/// </summary>
		/// <value><c>true</c> if this instance is abstract; otherwise, <c>false</c>.</value>
		/// <seealso cref="System.Type.IsAbstract"/>
		public bool IsAbstract { get { return Member.IsAbstract; } }

		public bool IsSealed { get { return Member.IsSealed; } }

		public bool IsGenericParameter { get { return Member.IsGenericParameter; } }

		public bool IsGenericTypeDefinition { get { return Member.IsGenericTypeDefinition; } }

		public bool IsNested { get { return Member.IsNested; } }

		public GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				return Member.GenericParameterAttributes;
			}
		}

		#endregion

		public TypeElement BaseType
		{
			get
			{
				return Documentation.Find(Type.BaseType);
			}
		}

		public TypeElement DeclaringType
		{
			get
			{
				return Documentation.Find(Type.DeclaringType);
			}
		}

		public ICollection<ConstructorElement> Constructors
		{
			get
			{
				return Type.GetPublicAndProtectedConstructors()
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
				return Documentation.Types.SelectMany(t => t.Methods).Where(m => m.IsExtension() && m.GetParameters()[0].ParameterType.IsAssignableFrom(this)).ToList();
			}
		}

		public bool IsAssignableFrom(TypeElement t)
		{
			return Member.IsAssignableFrom(t.Member);
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

		public TypeElement[] GetInterfaces()
		{
			return Member.GetInterfaces().Select(t => Documentation.Find(t)).ToArray();
		}

		public ICollection<TypeElement> GetChildTypes()
		{
			return Documentation.Types.Where(t => t.IsChildTypeOf(this)).ToList();
		}

		public ICollection<TypeElement> GetDescendantTypes()
		{
			return Documentation.Types.Where(t => t.IsDescendantTypeOf(this)).ToList();
		}

		public TypeElement[] GetGenericArguments()
		{
			return Member.GetGenericArguments().Select(t => Documentation.Find(t)).ToArray();
		}

		public TypeElement[] GetGenericParameterConstraints()
		{
			return Member.GetGenericParameterConstraints().Select(t => Documentation.Find(t)).ToArray();
		}

		public bool IsChildTypeOf(TypeElement parentElement)
		{
			if (parentElement == null)
			{
				return false;
			}

			TypeElement myBaseType = BaseType;
			if (myBaseType == null)
			{
				return false;
			}

			if (parentElement.Equals(myBaseType))
			{
				return true;
			}

			if (parentElement.IsGenericTypeDefinition && !myBaseType.IsGenericTypeDefinition && myBaseType.IsGenericType)
			{
				// break my base type down to a definition too
				if (parentElement.Equals(myBaseType.GetGenericTypeDefinition()))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsDescendantTypeOf(TypeElement ancestorElement)
		{
			if (IsChildTypeOf(ancestorElement))
			{
				return true;
			}

			TypeElement myBaseType = BaseType;
			if (myBaseType == null)
			{
				return false;
			}

			return myBaseType.IsDescendantTypeOf(ancestorElement);
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

		public virtual string Filename(string prefix = "T")
		{
			return Documentation.FilenameProvider.Filename(this, prefix);
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetXmlComment("//member[@name=\"T:" + Type.FullName + "\"]");
		}

		public TypeElement GetElementType()
		{
			return Documentation.Find(Member.GetElementType());
		}

		public bool IsStatic { get { return Member.IsAbstract && Member.IsSealed; } }

		public TypeElement GetGenericTypeDefinition()
		{
			return Documentation.Find(Member.GetGenericTypeDefinition());
		}

		public TypeElement[] GetNestedTypes()
		{
			return Member.GetNestedTypes().Select(t => Documentation.Find(t)).ToArray();
		}

		public MethodElement GetMethod(string methodName)
		{
			return Methods.Where(m => m.Name == methodName).SingleOrDefault();
		}

		public MethodElement GetMethod(string methodName, params Type[] parameterTypes)
		{
			return Methods.Where(m => m.Name == methodName 
				&& m.GetParameters().Select(p => p.ParameterType.Member).SequenceEqual(parameterTypes)).SingleOrDefault();
		}

		/// <summary>
		/// Finds a property by its name.
		/// </summary>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns>The property. If no such property exists, <c>null</c> is returned.</returns>
		public PropertyElement GetProperty(string propertyName)
		{
			return Properties.SingleOrDefault(p => p.Name == propertyName);
		}

		public ConstructorElement GetConstructor(params TypeElement[] types)
		{
			return Constructors.SingleOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
		}

		public virtual bool IsLocalType
		{
			get
			{
				return true;
			}
		}

		protected override IPrinter GetPrinter()
		{
			return Documentation.PrinterFactory.GetTypePrinter();
		}
	}
}

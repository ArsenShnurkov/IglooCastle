using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public class TypeElement : ReflectedElement<Type>, IFormattable
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

		public bool IsEnum
		{
			get { return Member.IsEnum; }
		}

		public bool IsClass
		{
			get { return Member.IsClass; }
		}

		public bool IsInterface
		{
			get { return Member.IsInterface; }
		}

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

		public Assembly Assembly
		{
			get { return Member.Assembly; }
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

		public ICollection<TypeElement> GetDerivedTypes()
		{
			return Documentation.Types.Where(t =>
				(
					t.Type != this.Type && Type.IsAssignableFrom(t.Type))
					||
					t.HasBaseType(this)
				).ToList();
		}

		public bool IsGenericType
		{
			get { return Member.IsGenericType; }
		}

		public string Namespace { get { return Member.Namespace; } }

		public TypeElement[] GetGenericArguments()
		{
			return Member.GetGenericArguments().Select(t => Documentation.Find(t)).ToArray();
		}

		public bool HasBaseType(TypeElement typeElement)
		{
			if (typeElement == null)
			{
				return false;
			}

			TypeElement baseTypeElement = BaseType;
			while (baseTypeElement != null && baseTypeElement.Type != typeElement.Type)
			{
				baseTypeElement = baseTypeElement.BaseType;
			}

			return baseTypeElement != null;
		}

		/// <summary>
		/// Uses <see cref="TypePrinter" /> to print this type's name.
		/// </summary>
		/// <param name="format">Controls the output options. See remarks section.</param>
		/// <returns>A string representing this type element.</returns>
		/// <remarks>
		/// 	<paramref name="format" /> can be:
		/// 	<list type="table">
		/// 		<listheader>
		/// 			<term>Value</term>
		/// 			<description>Output</description>
		/// 		</listheader>
		/// 		<item>
		/// 			<term><c>null</c></term>
		/// 			<description>The containing type's ToString method is called as-is.</description>
		/// 		</item>
		/// 		<item>
		/// 			<term><c>"s"</c></term>
		/// 			<description>The short name is returned (without namespace).</description>
		/// 		</item>
		/// 		<item>
		/// 			<term><c>"f"</c></term>
		/// 			<description>The full name is returned (with namespace).</description>
		/// 		</item>
		/// 	</list>
		/// </remarks>
		/// <example>
		/// 	This example prints the full name of a type element:
		/// 	<code>
		/// 	Console.WriteLine(typeElement.ToString("f"));
		/// 	</code>
		/// </example>
		public override string ToString(string format, IFormatProvider formatProvider)
		{
			if (format == null)
			{
				return Member.ToString();
			}

			if (format == "l")
			{
				return Documentation.TypePrinter.Print(this);
			}

			TypePrinter.NameComponents nameComponents = TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments;
			if (format == "f")
			{
				nameComponents = nameComponents | TypePrinter.NameComponents.Namespace;
			}

			return Documentation.TypePrinter.Name(this, nameComponents);
		}

		public string ToHtml()
		{
			return ToString("l");
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

		public bool IsArray { get { return Member.IsArray; } }

		public TypeElement GetElementType()
		{
			return Documentation.Find(Member.GetElementType());
		}

		public bool IsByRef { get { return Member.IsByRef; } }

		public bool IsGenericParameter { get { return Member.IsGenericParameter; } }

		public bool IsGenericTypeDefinition { get { return Member.IsGenericTypeDefinition; } }

		public TypeElement GetGenericTypeDefinition()
		{
			return Documentation.Find(Member.GetGenericTypeDefinition());
		}

		public bool IsNested { get { return Member.IsNested; } }

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
	}

	internal sealed class ExternalTypeElement : TypeElement
	{
		public ExternalTypeElement(Documentation owner, Type type) : base(owner, type)
		{
		}

		public override string Filename(string prefix = "T")
		{
			return null;
		}
	}
}

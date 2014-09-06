using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IglooCastle.CLI
{
	public class MethodElement : MethodBaseElement<MethodInfo>
	{
		public MethodElement(Documentation documentation, TypeElement ownerType, MethodInfo method)
			: base(documentation, ownerType, method)
		{
		}

		internal MethodInfo Method
		{
			get { return Member; }
		}

		public bool IsStatic
		{
			get { return Member.IsStatic; }
		}

		public bool IsAbstract
		{
			get { return Member.IsAbstract; }
		}

		public bool IsVirtual
		{
			get { return Member.IsVirtual; }
		}

		public bool IsOverride
		{
			get { return Member.IsOverride(); }
		}

		public bool IsOverload
		{
			get { return Member.IsOverload(); }
		}

		public TypeElement ReturnType
		{
			get { return Documentation.Find(Member.ReturnType); }
		}

		public bool IsPrivate
		{
			get
			{
				return Member.IsPrivate;
			}
		}

		/// <summary>
		/// Checks if this method is an extension method.
		/// </summary>
		/// <returns><c>true</c> if this is an extension method, <c>false</c> otherwise.</returns>
		public bool IsExtension()
		{
			bool isExtension = Member.GetCustomAttribute<ExtensionAttribute>() != null;
			return isExtension;
		}

		protected override PrinterBase GetPrinter()
		{
			return new MethodPrinter(Documentation);
		}

		public string Filename()
		{
			return Documentation.FilenameProvider.Filename(this);
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetMethodDocumentation(OwnerType.Type, Method.Name, Method.GetParameters());
		}

		public bool IsFinal { get { return Member.IsFinal; } }
	}
}

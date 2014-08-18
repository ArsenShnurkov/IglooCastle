using System;
using System.Linq;
using System.Reflection;

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

		public ParameterInfoElement[] GetParameters()
		{
			return Member.GetParameters().Select(p => new ParameterInfoElement(Documentation, p)).ToArray();
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

		public override MethodAttributes GetAccess()
		{
			return Member.GetAccess();
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

		public bool IsExtension()
		{
			return Member.IsExtension();
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetMethodDocumentation(OwnerType.Type, Method.Name, Method.GetParameters());
		}
	}

	public class ParameterInfoElement : DocumentationElement<ParameterInfo>
	{
		public ParameterInfoElement(Documentation documentation, ParameterInfo parameterInfo)
			: base(documentation, parameterInfo)
		{
		}

		public override IXmlComment XmlComment
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsOut
		{
			get
			{
				return Member.IsOut;
			}
		}

		public string Name
		{
			get
			{
				return Member.Name;
			}
		}

		public TypeElement ParameterType
		{
			get
			{
				return Documentation.Find(Member.ParameterType);
			}
		}
	}
}

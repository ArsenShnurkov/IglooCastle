using System;
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

		public bool IsExtension()
		{
			return Member.IsExtension();
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetMethodDocumentation(OwnerType.Type, Method.Name, Method.GetParameters());
		}
	}
}

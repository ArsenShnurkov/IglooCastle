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

		public MethodInfo Method
		{
			get { return Member; }
		}

		public Type ReturnType
		{
			get { return Member.ReturnType; }
		}
	}
}

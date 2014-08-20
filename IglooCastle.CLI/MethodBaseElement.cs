using System;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public abstract class MethodBaseElement<T> : TypeMemberElement<T>
		where T : MethodBase
	{
		protected MethodBaseElement(Documentation documentation, TypeElement ownerType, T member)
			: base(documentation, ownerType, member)
		{
		}

		public ParameterInfoElement[] GetParameters()
		{
			return Member.GetParameters().Select(p => new ParameterInfoElement(Documentation, p)).ToArray();
		}
	}
}

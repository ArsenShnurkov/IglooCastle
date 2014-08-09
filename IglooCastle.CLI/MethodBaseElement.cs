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

		public ParameterInfo[] GetParameters()
		{
			return Member.GetParameters();
		}

		public MethodAttributes Attributes
		{
			get
			{
				return Member.Attributes;
			}
		}
	}
}

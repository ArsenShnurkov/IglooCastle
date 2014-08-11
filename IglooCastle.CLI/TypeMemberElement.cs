using System;
using System.Reflection;

namespace IglooCastle.CLI
{
	public abstract class TypeMemberElement<T> : ReflectedElement<T>
		where T: MemberInfo
	{
		protected TypeMemberElement(Documentation documentation, TypeElement ownerType, T member) : base(documentation, member)
		{
			OwnerType = ownerType;
		}

		public TypeElement OwnerType { get; private set; }

		public bool IsDeclaredIn(TypeElement typeElement)
		{
			return Member.DeclaringType == typeElement.Type;
		}

		public bool IsDeclaredIn(Type type)
		{
			return Member.DeclaringType == type;
		}
	}
}

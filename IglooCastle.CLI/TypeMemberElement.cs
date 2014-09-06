using System;
using System.Linq;
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

		public TypeElement ReflectedType { get { return Documentation.Find(Member.ReflectedType); } }

		public TypeElement DeclaringType { get { return Documentation.Find(Member.DeclaringType); } }

		public override NamespaceElement NamespaceElement
		{
			get { return Documentation.Namespaces.Single(n => n.Namespace == OwnerType.Member.Namespace); }
		}

		public bool IsInherited
		{
			get
			{
				return Member.DeclaringType != OwnerType.Type;
			}
		}
	}
}

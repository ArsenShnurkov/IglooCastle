using System.Reflection;

namespace IglooCastle.CLI
{
	public class ConstructorElement : MethodBaseElement<ConstructorInfo>
	{
		public ConstructorElement(Documentation documentation, TypeElement ownerType, ConstructorInfo constructor) :
			base(documentation, ownerType, constructor)
		{
		}

		public ConstructorInfo Constructor
		{
			get { return Member; }
		}
	}
}

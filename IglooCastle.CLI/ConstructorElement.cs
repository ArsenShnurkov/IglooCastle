using System.Reflection;

namespace IglooCastle.CLI
{
	public class ConstructorElement : ReflectedElement<ConstructorInfo>
	{
		public ConstructorInfo Constructor
		{
			get { return Member; }
			set { Member = value; }
		}
	}
}

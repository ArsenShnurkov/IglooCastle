using System.Reflection;

namespace IglooCastle.CLI
{
	public class MethodElement : DocumentationElement<MethodInfo>
	{
		public MethodInfo Method
		{
			get { return Member; }
			set { Member = value; }
		}
	}
}

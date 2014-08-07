using System.Reflection;

namespace IglooCastle.CLI
{
	public class PropertyElement : ReflectedElement<PropertyInfo>
	{
		public PropertyInfo Property
		{
			get { return Member; }
			set { Member = value; }
		}
	}
}

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace IglooCastle.CLI
{
	public class ParameterInfoElement : DocumentationElement<ParameterInfo>
	{
		public ParameterInfoElement(Documentation documentation, ParameterInfo parameterInfo)
			: base(documentation, parameterInfo)
		{
		}

		public override IXmlComment XmlComment
		{
			get { throw new NotImplementedException(); }
		}

		public bool IsOut
		{
			get
			{
				return Member.IsOut;
			}
		}

		public string Name
		{
			get
			{
				return Member.Name;
			}
		}

		public TypeElement ParameterType
		{
			get
			{
				return Documentation.Find(Member.ParameterType);
			}
		}

		public IEnumerable<AttributeElement> GetCustomAttributes(bool inherit)
		{
			return Member.GetCustomAttributes(inherit).Cast<Attribute>().Select(a => new AttributeElement(Documentation, a));
		}
	}
}

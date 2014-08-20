using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace IglooCastle.CLI
{
	public class AttributeElement : DocumentationElement<Attribute>
	{
		public AttributeElement(Documentation documentation, Attribute member) : base(documentation, member)
		{
		}

		#region implemented abstract members of DocumentationElement

		public override IXmlComment XmlComment
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		public TypeElement AttributeType
		{
			get
			{
				return Documentation.Find(Member.GetType());
			}
		}

		public bool IsInstance(params Type[] types)
		{
			return types.Any(t => t.IsInstanceOfType(Member));
		}
	}
}

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace IglooCastle.CLI
{

	public class CustomAttributeDataElement : DocumentationElement<CustomAttributeData>
	{
		public CustomAttributeDataElement(Documentation documentation, CustomAttributeData member) : base(documentation, member)
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
				return Documentation.Find(Member.AttributeType);
			}
		}
	}
}

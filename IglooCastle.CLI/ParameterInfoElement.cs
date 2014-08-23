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

		/// <summary>
		/// Gets a value indicating whether this is a <c>params</c> parameter.
		/// </summary>
		/// <value><c>true</c> if this parameter is <c>params</c>; otherwise, <c>false</c>.</value>
		public bool IsParams
		{
			get
			{
				return Member.GetCustomAttribute<ParamArrayAttribute>() != null;
			}
		}

		public IEnumerable<AttributeElement> GetCustomAttributes(bool inherit)
		{
			return Member.GetCustomAttributes(inherit).Cast<Attribute>().Select(a => new AttributeElement(Documentation, a));
		}
	}
}

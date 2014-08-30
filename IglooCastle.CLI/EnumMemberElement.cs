using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
	public class EnumMemberElement : TypeMemberElement<FieldInfo>
	{
		public EnumMemberElement(Documentation documentation, TypeElement ownerType, string enumName)
			: base(documentation, ownerType, ownerType.Type.GetField(enumName))
		{
		}

		public int Value
		{
			get
			{
				return (int)Enum.Parse(OwnerType.Type, Member.Name);
			}
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetXmlComment("//member[@name=\"F:" +
					OwnerType.Member.FullName + "." +
					Member.Name + "\"]");
		}

		protected override PrinterBase GetPrinter()
		{
			throw new NotImplementedException();
		}
	}
}

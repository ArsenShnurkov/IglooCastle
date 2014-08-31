using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	internal sealed class ExternalTypeElement : TypeElement
	{
		public ExternalTypeElement(Documentation owner, Type type) : base(owner, type)
		{
		}

		public override string Filename(string prefix = "T")
		{
			return null;
		}

		public override bool IsLocalType
		{
			get
			{
				if (IsGenericParameter)
				{
					return false;
				}

				if (IsGenericType && !IsGenericTypeDefinition)
				{
					return GetGenericTypeDefinition().IsLocalType;
				}

				return false;
			}
		}
	}
}

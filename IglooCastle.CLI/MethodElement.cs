﻿using System;
using System.Linq;
using System.Reflection;

namespace IglooCastle.CLI
{
	public class MethodElement : MethodBaseElement<MethodInfo>
	{
		public MethodElement(Documentation documentation, TypeElement ownerType, MethodInfo method)
			: base(documentation, ownerType, method)
		{
		}

		internal MethodInfo Method
		{
			get { return Member; }
		}

		public ParameterInfoElement[] GetParameters()
		{
			return Member.GetParameters().Select(p => new ParameterInfoElement(Documentation, p)).ToArray();
		}

		public bool IsStatic
		{
			get { return Member.IsStatic; }
		}

		public bool IsAbstract
		{
			get { return Member.IsAbstract; }
		}

		public bool IsVirtual
		{
			get { return Member.IsVirtual; }
		}

		public bool IsOverride
		{
			get { return Member.IsOverride(); }
		}

		public bool IsOverload
		{
			get { return Member.IsOverload(); }
		}

		public override MethodAttributes GetAccess()
		{
			return Member.GetAccess();
		}

		public TypeElement ReturnType
		{
			get { return Documentation.Find(Member.ReturnType); }
		}

		public bool IsPrivate
		{
			get
			{
				return Member.IsPrivate;
			}
		}

		public bool IsExtension()
		{
			return Member.IsExtension();
		}

		public string ToHtml()
		{
			return Documentation.TypePrinter.Print(this);
		}

		public string ToSignature()
		{
			return Documentation.TypePrinter.Signature(this);
		}

		public string ToSyntax()
		{
			return Documentation.TypePrinter.Syntax(this);
		}

		public string Filename()
		{
			return Documentation.FilenameProvider.Filename(this);
		}

		protected override IXmlComment GetXmlComment()
		{
			return Documentation.GetMethodDocumentation(OwnerType.Type, Method.Name, Method.GetParameters());
		}
	}
}

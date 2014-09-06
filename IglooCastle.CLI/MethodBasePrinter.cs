using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IglooCastle.CLI
{
	abstract class MethodBasePrinter<T, TMember> : PrinterBase<T, TMember>
		where T : MethodBaseElement<TMember>
		where TMember : MethodBase
	{
		protected MethodBasePrinter(Documentation documentation) : base(documentation) { }

		public string ParameterSignature(T methodBaseElement)
		{
			return "(" + string.Join(", ", methodBaseElement.GetParameters().Select(p => p.ParameterType.ToString("s"))) + ")";
		}

		public string Parameters(T method, bool typeLinks = true)
		{
			MethodElement m = method as MethodElement;
			bool isExtension = m != null && m.IsExtension();
			string args = string.Join(
				", ",
				method.GetParameters().Select((p, index) => FormatParameter(p, isExtension && index == 0, typeLinks)));
			return args;
		}

		protected string AccessPrefix(T member)
		{
			if (member.ReflectedType.IsInterface)
			{
				return string.Empty;
			}

			MethodAttributes access = member.GetAccess();
			return access.ToAccessString();
		}

		protected string Modifiers(MethodElement method)
		{
			if (method.ReflectedType.IsInterface)
			{
				return string.Empty;
			}

			string modifiers = "";
			if (method.IsStatic)
			{
				modifiers += " static";
			}

			if (method.IsFinal)
			{
				modifiers += " sealed";
			}

			if (method.IsAbstract)
			{
				modifiers += " abstract";
			}
			else if (method.IsVirtual)
			{
				modifiers += method.IsOverride ? " override" : " virtual";
			}

			return modifiers.TrimStart(' ');
		}

		protected string FormatParameter(ParameterInfoElement parameterInfo, bool isExtensionThis, bool typeLinks)
		{
			string result = "";

			string @params = parameterInfo.IsParams ? "params" : "";

			string byref = null;
			TypeElement type = parameterInfo.ParameterType;
			if (type.IsByRef)
			{
				type = type.GetElementType();
				if (parameterInfo.IsOut)
				{
					byref = "out";
				}
				else
				{
					byref = "ref";
				}
			}

			string thisparam = isExtensionThis ? "this" : "";

			result = " ".JoinNonEmpty(
				@params,
				byref,
				thisparam,
				type.ToHtml(typeLinks),
				parameterInfo.Name);

			return result;
		}
	}
}

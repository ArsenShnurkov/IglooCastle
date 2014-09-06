using System;

namespace IglooCastle.CLI
{
	public interface IPrinter
	{
		string Print(object element, bool typeLinks = true);
		string Syntax(object element, bool typeLinks = true);
		string Signature(object element, bool typeLinks = true);
		string Format(object element, string format);
	}
}

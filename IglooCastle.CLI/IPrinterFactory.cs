using System;

namespace IglooCastle.CLI
{
	public interface IPrinterFactory
	{
		IPrinter GetTypePrinter();
		IPrinter GetConstructorPrinter();
		IPrinter GetPropertyPrinter();
		IPrinter GetMethodPrinter();
	}
}

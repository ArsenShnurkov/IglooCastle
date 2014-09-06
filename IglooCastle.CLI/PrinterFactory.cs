using System;

namespace IglooCastle.CLI
{
	class PrinterFactory : IPrinterFactory
	{
		private readonly Documentation _documentation;

		public PrinterFactory(Documentation documentation)
		{
			_documentation = documentation;
		}

		public IPrinter GetTypePrinter()
		{
			return new TypePrinter(_documentation);
		}

		public IPrinter GetConstructorPrinter()
		{
			return new ConstructorPrinter(_documentation);
		}

		public IPrinter GetPropertyPrinter()
		{
			return new PropertyPrinter(_documentation);
		}

		public IPrinter GetMethodPrinter()
		{
			return new MethodPrinter(_documentation);
		}

		public IPrinter GetCustomAttributeDataPrinter()
		{
			return new CustomAttributeDataPrinter(_documentation);
		}
	}
}

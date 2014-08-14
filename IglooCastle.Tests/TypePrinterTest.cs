using IglooCastle.CLI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class TypePrinterTest
	{
		private Documentation _documentation;

		[SetUp]
		public void SetUp()
		{
			_documentation = new Documentation();
			_documentation.Scan(typeof(FilenameProvider).Assembly);
		}

		[Test]
		public void TestPropertyLink()
		{
			// public ICollection<PropertyElement> Properties
			const string expected =
				"System.Collections.Generic.ICollection&lt;" +
				"<a href=\"T_IglooCastle.CLI.PropertyElement.html\">PropertyElement</a>" +
				"&gt;";
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(TypeElement));
			PropertyElement targetProperty = targetTypeElement.Properties.Single(p => p.Name == "Properties");

			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			Assert.AreEqual(expected, typePrinter.Print(targetProperty.Member.PropertyType));
		}

		[Test]
		public void TestSystemString()
		{
			// public string InnerText
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a>";
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(IXmlComment));
			PropertyElement targetProperty = targetTypeElement.Properties.Single(p => p.Name == "InnerText");

			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			Assert.AreEqual(expected, typePrinter.Print(targetProperty.Member.PropertyType));
		}

		[Test]
		public void TestArray()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.NamespaceElement.html\">NamespaceElement</a>[]";
			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			Assert.AreEqual(expected, typePrinter.Print(typeof(NamespaceElement[])));
		}

		[Test]
		public void TestSystemType()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.type%28v=vs.110%29.aspx\">Type</a>";
			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			Assert.AreEqual(expected, typePrinter.Print(typeof(Type)));
		}

		[Test]
		public void TestGenericArgument()
		{
			const string expected = "T";
			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			Assert.AreEqual(expected, typePrinter.Print(typeof(DocumentationElement<>).GetProperty("Member").PropertyType));
		}

		[Test]
		public void TestMethodNameWithParameterTypes()
		{
			const string expected = "Normalize(Type)";
			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			var method = typeof(Documentation).GetMethod("Normalize", new[] { typeof(Type) });
			Assert.AreEqual(expected, typePrinter.Print(method));
		}

		[Test]
		public void TestMethodNameWithAliasParameter()
		{
			const string expected = "Equals(object)";
			TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
			var method = typeof(Documentation).GetMethod("Equals", new[] { typeof(object) });
			Assert.AreEqual(expected, typePrinter.Print(method));
		}
	}
}

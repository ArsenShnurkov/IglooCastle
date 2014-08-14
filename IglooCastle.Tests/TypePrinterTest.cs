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
		private TypePrinter _typePrinter;

		[SetUp]
		public void SetUp()
		{
			_documentation = new Documentation();
			_documentation.Scan(typeof(FilenameProvider).Assembly);
			_typePrinter = new TypePrinter(_documentation, new FilenameProvider());
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

			Assert.AreEqual(expected, _typePrinter.Print(targetProperty.Member.PropertyType));
		}

		[Test]
		public void TestSystemString()
		{
			// public string InnerText
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a>";
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(IXmlComment));
			PropertyElement targetProperty = targetTypeElement.Properties.Single(p => p.Name == "InnerText");

			Assert.AreEqual(expected, _typePrinter.Print(targetProperty.Member.PropertyType));
		}

		[Test]
		public void TestArray()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.NamespaceElement.html\">NamespaceElement</a>[]";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(NamespaceElement[])));
		}

		[Test]
		public void TestSystemType()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.type%28v=vs.110%29.aspx\">Type</a>";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(Type)));
		}

		[Test]
		public void TestGenericArgument()
		{
			const string expected = "TMember";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(DocumentationElement<>).GetProperty("Member").PropertyType));
		}

		[Test]
		public void TestMethodNameWithParameterTypes()
		{
			const string expected = "Normalize(Type)";
			var method = typeof(Documentation).GetMethod("Normalize", new[] { typeof(Type) });
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void TestMethodNameWithAliasParameter()
		{
			const string expected = "Equals(object)";
			var method = typeof(Documentation).GetMethod("Equals", new[] { typeof(object) });
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void TestShortNameOfNestedClass()
		{
			const string expected = "TypePrinter.PrintOptions";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(TypePrinter.PrintOptions), new TypePrinter.PrintOptions
			{
				Links = false,
				ShortName = true
			}));
		}

		[Test]
		public void TestShortNameOfNestedClassInLink()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.TypePrinter+PrintOptions.html\">TypePrinter.PrintOptions</a>";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(TypePrinter.PrintOptions)));
		}

		[Test]
		public void TestGenericTypeDefinition()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.MethodBaseElement%601.html\">MethodBaseElement</a>&lt;T&gt;";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(MethodBaseElement<>)));
		}

		[Test]
		public void TestNameOfGenericType()
		{
			Assert.AreEqual("DocumentationElement", _typePrinter.Name(typeof(DocumentationElement<>), TypePrinter.NameComponents.Name));
			Assert.AreEqual("DocumentationElement&lt;TMember&gt;", _typePrinter.Name(typeof(DocumentationElement<>), TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments));
			Assert.AreEqual("IglooCastle.CLI.DocumentationElement&lt;TMember&gt;", _typePrinter.Name(typeof(DocumentationElement<>), TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments | TypePrinter.NameComponents.Namespace));
		}

		[Test]
		public void TestCompleteGenericType()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.DocumentationElement%601.html\">DocumentationElement</a>&lt;" +
				"<a href=\"T_IglooCastle.CLI.TypePrinter.html\">TypePrinter</a>" +
				"&gt;";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(DocumentationElement<TypePrinter>)));
		}
	}
}

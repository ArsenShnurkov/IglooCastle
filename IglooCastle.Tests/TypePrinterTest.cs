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
			Assert.AreEqual(expected, _typePrinter.Print(method, new TypePrinter.PrintOptions { HideParametersForNonOverloads = false }));
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

		[Test]
		public void TestMethodSyntax()
		{
			const string expected = "public void Scan(Assembly assembly)";
			var method = typeof(Documentation).GetMethod("Scan");
			Assert.AreEqual(expected, _typePrinter.Syntax(method, new TypePrinter.PrintOptions { Links = false, ShortName = true }));
		}

		[Test]
		public void TestStaticMethodSyntax()
		{
			const string expected = "public static string Alias(Type type)";
			var method = typeof(SystemTypes).GetMethod("Alias", new[] { typeof(Type) });
			Assert.AreEqual(expected, _typePrinter.Syntax(method, new TypePrinter.PrintOptions { Links = false, ShortName = true }));
		}

		[Test]
		public void TestInterfaceMethodSyntax()
		{
			const string expected = "string Section(string name)";
			var method = typeof(IXmlComment).GetMethod("Section");
			Assert.AreEqual(expected, _typePrinter.Syntax(method, new TypePrinter.PrintOptions { Links = false, ShortName = true }));
		}

		[Test]
		public void TestExtensionMethodSyntax()
		{
			const string expected = "public static string Summary(this IXmlComment xmlComment)";
			var method = typeof(XmlCommentExtensions).GetMethod("Summary");
			Assert.AreEqual(expected, _typePrinter.Syntax(method, new TypePrinter.PrintOptions { Links = false, ShortName = true }));
		}

		[Test]
		public void TestPrintExtensionMethod()
		{
			const string expected = "Summary";
			var method = typeof(XmlCommentExtensions).GetMethod("Summary");
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void TestPrintEqualsMethod()
		{
			const string expected = "Equals";
			var method = typeof(XmlComment).GetMethod("Equals");
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void TestOverrideMethodSyntax()
		{
			const string expected = "public override bool TryGetMember(GetMemberBinder binder, out object result)";
			var method = typeof(DocumentationElement<>).GetMethod("TryGetMember");
			Assert.AreEqual(expected, _typePrinter.Syntax(method, new TypePrinter.PrintOptions { Links = false, ShortName = true }));
		}

		[Test]
		public void TestPrintRefType()
		{
			const string expected = "object";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(object).MakeByRefType(), new TypePrinter.PrintOptions { Links = false, ShortName = true }));
		}
	}
}

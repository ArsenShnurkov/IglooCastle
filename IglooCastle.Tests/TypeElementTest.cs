using System;
using System.Collections.Generic;
using System.Linq;
using IglooCastle.CLI;
using NUnit.Framework;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class TypeElementTest : TestBase
	{
		[Test]
		public void TestName()
		{
			var typeElement = Documentation.Find(typeof(TypeElement));
			Assert.IsNotNull(typeElement);
			Assert.AreEqual("TypeElement", typeElement.Name);
		}

		[Test]
		public void TestNameOfGenericDefinition()
		{
			var typeElement = Documentation.Find(typeof(DocumentationElement<>));
			Assert.AreEqual("DocumentationElement`1", typeElement.Name);
		}

		[Test]
		public void TestShortNameGenericDefinition()
		{
			var typeElement = Documentation.Find(typeof(DocumentationElement<>));
			Assert.AreEqual("DocumentationElement&lt;TMember&gt;", typeElement.ToString("s"));
		}

		[Test]
		public void TestFullNameGenericDefinition()
		{
			var typeElement = Documentation.Find(typeof(DocumentationElement<>));
			Assert.AreEqual("IglooCastle.CLI.DocumentationElement&lt;TMember&gt;", typeElement.ToString("f"));
		}

		[Test]
		public void TestShortNameNestedClass()
		{
			var typeElement = Documentation.Find(typeof(Sample.NestedSample));
			Assert.AreEqual("Sample.NestedSample", typeElement.ToString("s"));
		}
	
		[Test]
		public void TestShortNameSecondLevelNestedClass()
		{
			var typeElement = Documentation.Find(typeof(Sample.NestedSample.SecondLevelNest));
			Assert.AreEqual("Sample.NestedSample.SecondLevelNest", typeElement.ToString("s"));
		}

		[Test]
		public void TestFullNameSecondLevelNestedClass()
		{
			var typeElement = Documentation.Find(typeof(Sample.NestedSample.SecondLevelNest));
			Assert.AreEqual(
				"IglooCastle.Tests.Sample.NestedSample.SecondLevelNest",
				typeElement.ToString("f"));
		}

		[Test]
		public void TestHtmlSystemString()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a>";
			Assert.AreEqual(expected, Documentation.Find(typeof(string)).ToHtml());
		}

		[Test]
		public void TestHtmlSystemType()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.type%28v=vs.110%29.aspx\">Type</a>";
			Assert.AreEqual(expected, Documentation.Find(typeof(Type)).ToHtml());
		}

		[Test]
		public void TestHtmlSystemObjectByRef()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.object%28v=vs.110%29.aspx\">object</a>";
			Assert.AreEqual(expected, Documentation.Find(typeof(object).MakeByRefType()).ToHtml());
		}

		[Test]
		public void TestHtml_GenericContainerIsNotLocalType_GenericArgumentIsLocalType()
		{
			// public ICollection<PropertyElement> Properties
			const string expected =
				"System.Collections.Generic.ICollection&lt;" +
				"<a href=\"T_IglooCastle.CLI.PropertyElement.html\">PropertyElement</a>" +
				"&gt;";
			TypeElement targetTypeElement = Documentation.Find(typeof(TypeElement));
			PropertyElement targetProperty = targetTypeElement.GetProperty("Properties");
			Assert.AreEqual(expected, targetProperty.PropertyType.ToHtml());
		}

		[Test]
		public void TestHtmlArray()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.NamespaceElement.html\">NamespaceElement</a>[]";
			Assert.AreEqual(expected, Documentation.Find(typeof(NamespaceElement[])).ToHtml());
		}

		[Test]
		public void TestHtmlGenericArgument()
		{
			const string expected = "TMember";
			TypeElement targetTypeElement = Documentation.Find(typeof(DocumentationElement<>));
			PropertyElement targetProperty = targetTypeElement.GetProperty("Member");
			Assert.AreEqual(expected, targetProperty.PropertyType.ToHtml());
		}

		[Test]
		public void TestHtmlNestedClass()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.Documentation+Demo.html\">Documentation.Demo</a>";
			Assert.AreEqual(expected, Documentation.Find(typeof(Documentation.Demo)).ToHtml());
		}

		[Test]
		public void TestHtmlGenericTypeDefinition()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.MethodBaseElement%601.html\">MethodBaseElement</a>&lt;T&gt;";
			Assert.AreEqual(expected, Documentation.Find(typeof(MethodBaseElement<>)).ToHtml());
		}

		[Test]
		public void TestHtmlCompleteGenericType()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.DocumentationElement%601.html\">DocumentationElement</a>&lt;" +
				"<a href=\"T_IglooCastle.CLI.PropertyElement.html\">PropertyElement</a>" +
				"&gt;";
			Assert.AreEqual(expected, Documentation.Find(typeof(DocumentationElement<PropertyElement>)).ToHtml());
		}
	}
}

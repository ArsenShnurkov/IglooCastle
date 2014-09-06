using IglooCastle.CLI;
using IglooCastle.Demo;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class PropertyElementTest : TestBase
	{
		[Test]
		public void TestHtml()
		{
			const string expected = "<a href=\"P_IglooCastle.CLI.TypeElement.Methods.html\">Methods</a>";
			Assert.AreEqual(
				expected,
				Documentation.Find(typeof(TypeElement)).GetProperty("Methods").ToHtml());
		}

		[Test]
		public void TestSyntaxWithLinks()
		{
			const string expected = "public TMember Member { get; }";
			Assert.AreEqual(
				expected,
				Documentation.Find(typeof(DocumentationElement<>)).GetProperty("Member").ToSyntax());
		}

		[Test]
		public void TestSyntaxWithoutLinksAnnotatedProperty()
		{
			const string expected = "[Demo(\"name\")] public string Name { get; set; }";
			Assert.AreEqual(
				expected,
				Documentation.Find(typeof(AnnotatedDemo)).GetProperty("Name").ToString("x"));
		}
	}
}

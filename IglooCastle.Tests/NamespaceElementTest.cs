using IglooCastle.CLI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class NamespaceElementTest : TestBase
	{
		[Test]
		public void TestNamespace()
		{
			var namespaceElement = Documentation.FindNamespace("IglooCastle.CLI");
			Assert.IsNotNull(namespaceElement);
			Assert.AreEqual("IglooCastle.CLI", namespaceElement.Namespace);
		}

		[Test]
		public void TestHtml()
		{
			const string expected = "<a href=\"N_IglooCastle.CLI.html\">IglooCastle.CLI</a>";
			Assert.AreEqual(expected, Documentation.Namespaces.First().ToHtml());
		}
	}
}

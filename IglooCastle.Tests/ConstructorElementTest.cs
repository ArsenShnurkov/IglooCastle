using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using IglooCastle.CLI;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class ConstructorElementTest : TestBase
	{
		[Test]
		public void Test_Filename()
		{
			ConstructorElement constructorElement = 
				Documentation.Find(typeof(Documentation))
				.GetConstructor();

			Assert.AreEqual("C_IglooCastle.CLI.Documentation.html", constructorElement.Filename());
		}

		[Test]
		public void Test_Print()
		{
			ConstructorElement constructorElement = 
				Documentation.Find(typeof(Documentation))
					.GetConstructor();

			Assert.AreEqual(
				"<a href=\"C_IglooCastle.CLI.Documentation.html\">Documentation</a>",
				constructorElement.ToHtml());
		}

		[Test]
		public void Test_Syntax()
		{
			ConstructorElement constructorElement = 
				Documentation.Find(typeof(Documentation))
					.GetConstructor();

			Assert.AreEqual(
				"public Documentation()",
				constructorElement.ToSyntax());
		}

		[Test]
		public void Test_Syntax_ProtectedConstructor()
		{
			ConstructorElement constructorElement = 
				Documentation.Find(typeof(DocumentationElement<>))
					.Constructors.SingleOrDefault();
			Assert.IsNotNull(constructorElement, "Could not find protected constructor");
			Assert.AreEqual(
				"protected DocumentationElement(Documentation documentation, TMember member)",
				constructorElement.ToString("x"));
		}
	}
}

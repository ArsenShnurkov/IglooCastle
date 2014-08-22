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
	public class MethodElementTest : TestBase
	{
		[Test]
		public void Syntax_SealedMethod()
		{
			const string expected = "public sealed override string ToString()";

			MethodElement methodElement = Documentation
				.Find(typeof(DocumentationElement<>)).
				FindMethod("ToString", new Type[0]);

			Assert.AreEqual(expected, methodElement.ToString("x"));
		}
	}
}

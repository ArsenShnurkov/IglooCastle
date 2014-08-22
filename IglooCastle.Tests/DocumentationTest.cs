using IglooCastle.CLI;
using NUnit.Framework;
using System;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class DocumentationTest : TestBase
	{
		[Test]
		public void FindType()
		{
			var typeElement = Documentation.Find(typeof(Documentation));
			Assert.IsNotNull(typeElement);
			Assert.IsNotNull(typeElement.Filename());

			var sysTypeElement = Documentation.Find(typeof(Type));
			Assert.IsNotNull(sysTypeElement);
			Assert.IsNull(sysTypeElement.Filename());
		}
	}
}

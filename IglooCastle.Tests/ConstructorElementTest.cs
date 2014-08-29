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
	}
}

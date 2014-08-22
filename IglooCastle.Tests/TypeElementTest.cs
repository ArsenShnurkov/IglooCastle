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
	}
}

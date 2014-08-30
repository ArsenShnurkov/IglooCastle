using IglooCastle.CLI;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using IglooCastle.Demo;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class XmlCommentTest : TestBase
	{
		[Test]
		public void TestParam()
		{
			var xmlComment = Documentation.Find(typeof(CalculatorDemo))
				.GetMethod("Add").XmlComment;
			Assert.IsNotNull(xmlComment);
			Assert.IsNotInstanceOf(typeof(MissingXmlComment), xmlComment);
			Assert.AreEqual("Adds two numbers.", xmlComment.Summary());
			Assert.AreEqual("The first number to add.", xmlComment.Param("x"));
			Assert.AreEqual("The second number to add.", xmlComment.Param("y"));
			Assert.AreEqual("The sum of the two parameters.", xmlComment.Returns());
		}

		[Test]
		public void TestReturns()
		{
			var xmlComment = Documentation.Types.Single(t => t.Member == typeof(ReflectedElement<>)).Methods.Single(m => m.Name == "GetCustomAttributes").XmlComment;
			Assert.AreEqual(
				"A collection of <a href=\"http://msdn.microsoft.com/en-us/library/system.attribute%28v=vs.110%29.aspx\">Attribute</a>",
				xmlComment.Returns());
		}
	}
}

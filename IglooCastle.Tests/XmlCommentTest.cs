using IglooCastle.CLI;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class XmlCommentTest : TestBase
	{
		[Test]
		public void TestParam()
		{
			/**
			 * /// <summary>
			 * /// Checks if this method is an extension method.
			 * /// </summary>
			 * /// <param name="method">This method.</param>
			 * /// <returns><c>true</c> if this is an extension method, <c>false</c> otherwise.</returns>
			 */
			MethodInfo method = typeof(ReflectionExtensions).GetMethod("IsExtension");
			var xmlComment = Documentation.Namespaces[0].Methods.Single(m => m.Member == method).XmlComment;
			Assert.IsNotNull(xmlComment);
			Assert.IsNotInstanceOf(typeof(MissingXmlComment), xmlComment);
			Assert.AreEqual("Checks if this method is an extension method.", xmlComment.Summary());
			Assert.AreEqual("This method.", xmlComment.Param("method"));
			Assert.AreEqual("<code>true</code> if this is an extension method, <code>false</code> otherwise.", xmlComment.Returns());
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

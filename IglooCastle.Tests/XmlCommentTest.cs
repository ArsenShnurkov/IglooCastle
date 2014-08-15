using IglooCastle.CLI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class XmlCommentTest
	{
		private Documentation _documentation;

		[SetUp]
		public void SetUp()
		{
			_documentation = new Documentation();

			Assembly assembly = typeof(Documentation).Assembly;
			_documentation.Scan(assembly);

			if (!_documentation.AddDocumentation(assembly))
			{
				string assemblyFile = Path.Combine(Environment.CurrentDirectory, Path.GetFileName(assembly.Location));
				if (!_documentation.AddDocumentationFromAssemblyFile(assembly, assemblyFile))
				{
					Assert.Fail("Could not find documentation");
				}
			}
		}

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
			var xmlComment = _documentation.Namespaces[0].Methods.Single(m => m.Member == method).XmlComment;
			Assert.IsNotNull(xmlComment);
			Assert.IsNotInstanceOf(typeof(MissingXmlComment), xmlComment);
			Assert.AreEqual("Checks if this method is an extension method.", xmlComment.Summary());
			Assert.AreEqual("This method.", xmlComment.Param("method"));
			Assert.AreEqual("<code>true</code> if this is an extension method, <code>false</code> otherwise.", xmlComment.Returns());
		}

		[Test]
		public void TestReturns()
		{
			MethodInfo method = typeof(ReflectedElement<>).GetMethod("GetCustomAttributes");
			var xmlComment = _documentation.Find(method).XmlComment;
			Assert.AreEqual(
				"A collection of <a href=\"http://msdn.microsoft.com/en-us/library/system.attribute%28v=vs.110%29.aspx\">Attribute</a>",
				xmlComment.Returns());
		}
	}
}

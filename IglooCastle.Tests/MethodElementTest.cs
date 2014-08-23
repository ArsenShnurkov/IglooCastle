using IglooCastle.CLI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
				.Find(typeof(DocumentationElement<>))
				.GetMethod("ToString", new Type[0]);

			Assert.AreEqual(expected, methodElement.ToString("x"));
		}

		[Test]
		public void Syntax_ParamsMethod()
		{
			const string expected = "public bool IsInstance(params Type[] types)";
			MethodElement methodElement = Documentation
				.Find(typeof(AttributeElement))
				.GetMethod("IsInstance", typeof(Type[]));

			Assert.AreEqual(expected, methodElement.ToString("x"));
		}

		[Test]
		public void TestHtml()
		{
			const string expected = "<a href=\"M_IglooCastle.CLI.Documentation.Normalize-IglooCastle.CLI.TypeElement.html\">Normalize</a>";
			var method = Documentation.Find(typeof(Documentation))
				.GetMethod("Normalize", typeof(TypeElement));
			Assert.AreEqual(expected, method.ToHtml());
		}

		[Test]
		public void TestHtmlSystemObjectParameter()
		{
			const string expected = "Equals(object)";
			var method = Documentation.Find(typeof(Sample))
				.GetMethod("Equals", new[] { typeof(object) });
			Assert.AreEqual(expected, method.ToHtml());
		}

		[Test]
		public void TestHtmlExtensionMethod()
		{
			const string expected = "<a href=\"M_IglooCastle.CLI.XmlCommentExtensions.Summary-IglooCastle.CLI.IXmlComment.html\">Summary</a>";
			var method = Documentation.Find(typeof(XmlCommentExtensions)).GetMethod("Summary");
			Assert.AreEqual(expected, method.ToHtml());
		}

		[Test]
		public void TestHtmlEquals()
		{
			const string expected = "Equals";
			var method = Documentation.Find(typeof(XmlComment)).GetMethod("Equals");
			Assert.IsNotNull(method);
			Assert.AreEqual(expected, method.ToHtml());
		}

		[Test]
		public void TestSyntaxWithLinks()
		{
			const string expected = "public <a href=\"http://msdn.microsoft.com/en-us/library/system.void%28v=vs.110%29.aspx\">void</a> Scan(<a href=\"http://msdn.microsoft.com/en-us/library/system.reflection.assembly%28v=vs.110%29.aspx\">Assembly</a> assembly)";
			var method = Documentation.Find(typeof(Documentation)).GetMethod("Scan");
			Assert.AreEqual(expected, method.ToSyntax());
		}

		[Test]
		public void TestSyntaxWithLinksStaticMethod()
		{
			const string expected = "public static <a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> Alias(<a href=\"http://msdn.microsoft.com/en-us/library/system.type%28v=vs.110%29.aspx\">Type</a> type)";
			var method = Documentation.Find(typeof(SystemTypes))
				.GetMethod("Alias", new[] { typeof(Type) });
			Assert.AreEqual(expected, method.ToSyntax());
		}

		[Test]
		public void TestSyntaxWithLinksProtectedMethod()
		{
			const string expected = "protected abstract <a href=\"T_IglooCastle.CLI.IXmlComment.html\">IXmlComment</a> GetXmlComment()";
			var method = Documentation.Find(typeof(ReflectedElement<>))
				.GetMethod("GetXmlComment");
			Assert.AreEqual(expected, method.ToSyntax());
		}

		[Test]
		public void TestSyntaxWithLinksInterfaceMethod()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> Section(<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> sectionName)";
			var method = Documentation.Find(typeof(IXmlComment))
				.GetMethod("Section", new[] { typeof(string) });
			Assert.AreEqual(expected, method.ToSyntax());
		}

		[Test]
		public void TestSyntaxWithLinksOverride()
		{
			const string expected = "public override <a href=\"http://msdn.microsoft.com/en-us/library/system.reflection.methodattributes%28v=vs.110%29.aspx\">MethodAttributes</a> GetAccess()";
			var method = Documentation.Find(typeof(MethodElement))
				.GetMethod("GetAccess");
			Assert.AreEqual(expected, method.ToSyntax());
		}

		[Test]
		public void TestSyntaxWithLinksProtectedOverride()
		{
			const string expected = "protected override <a href=\"T_IglooCastle.CLI.IXmlComment.html\">IXmlComment</a> GetXmlComment()";
			var method = Documentation.Find(typeof(PropertyElement))
				.GetMethod("GetXmlComment");
			Assert.AreEqual(expected, method.ToSyntax());
		}

		[Test]
		public void TestSyntaxWithLinksExtensionMethod()
		{
			const string expected = "public static <a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> Summary(this <a href=\"T_IglooCastle.CLI.IXmlComment.html\">IXmlComment</a> xmlComment)";
			var method = Documentation.Find(typeof(XmlCommentExtensions))
				.GetMethod("Summary");
			Assert.AreEqual(expected, method.ToSyntax());
		}
	}
}

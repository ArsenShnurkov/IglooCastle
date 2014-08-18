using IglooCastle.CLI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.Tests
{
	public sealed class Sample : IEquatable<Sample>
	{
		public bool Equals(Sample other)
		{
			throw new NotImplementedException();
		}
	}

	[TestFixture]
	public class TypePrinterTest
	{
		private Documentation _documentation;
		private TypePrinter _typePrinter;

		[SetUp]
		public void SetUp()
		{
			_documentation = new Documentation();
			_documentation.Scan(typeof(FilenameProvider).Assembly);
			_typePrinter = _documentation.TypePrinter;
		}

		[Test]
		public void Name_Type_GenericTypeDefinition()
		{
			Assert.AreEqual("DocumentationElement", _typePrinter.Name(typeof(DocumentationElement<>), TypePrinter.NameComponents.Name));
			Assert.AreEqual("DocumentationElement&lt;TMember&gt;", _typePrinter.Name(typeof(DocumentationElement<>), TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments));
			Assert.AreEqual("IglooCastle.CLI.DocumentationElement&lt;TMember&gt;", _typePrinter.Name(typeof(DocumentationElement<>), TypePrinter.NameComponents.Name | TypePrinter.NameComponents.GenericArguments | TypePrinter.NameComponents.Namespace));
		}

		[Test]
		public void Name_Type_NestedClass()
		{
			const string expected = "TypePrinter.NameComponents";
			Assert.AreEqual(expected, _typePrinter.Name(typeof(TypePrinter.NameComponents), TypePrinter.NameComponents.Name));
		}

		[Test]
		public void Print_MethodInfo()
		{
			const string expected = "<a href=\"M_IglooCastle.CLI.Documentation.Normalize-System.Type.html\">Normalize(Type)</a>";
			var method = typeof(Documentation).GetMethod("Normalize", new[] { typeof(Type) });
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void Print_MethodInfo_SystemAliasParameter()
		{
			const string expected = "Equals(object)";
			var method = typeof(Sample).GetMethod("Equals", new[] { typeof(object) });
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void Print_MethodInfo_Extension()
		{
			const string expected = "<a href=\"M_IglooCastle.CLI.XmlCommentExtensions.Summary-IglooCastle.CLI.IXmlComment.html\">Summary</a>";
			var method = typeof(XmlCommentExtensions).GetMethod("Summary");
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void Print_MethodInfo_Equals()
		{
			const string expected = "Equals";
			var method = typeof(XmlComment).GetMethod("Equals");
			Assert.AreEqual(expected, _typePrinter.Print(method));
		}

		[Test]
		public void Print_NamespaceElement()
		{
			const string expected = "<a href=\"N_IglooCastle.CLI.html\">IglooCastle.CLI</a>";
			Assert.AreEqual(expected, _typePrinter.Print(_documentation.Namespaces.First()));
		}

		[Test]
		public void Print_PropertyInfo()
		{
			const string expected = "<a href=\"P_IglooCastle.CLI.TypeElement.Methods.html\">Methods</a>";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(TypeElement).GetProperty("Methods")));
		}

		[Test]
		public void Print_Type_GenericContainerIsNotLocalType_GenericArgumentIsLocalType()
		{
			// public ICollection<PropertyElement> Properties
			const string expected =
				"System.Collections.Generic.ICollection&lt;" +
				"<a href=\"T_IglooCastle.CLI.PropertyElement.html\">PropertyElement</a>" +
				"&gt;";
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(TypeElement));
			PropertyElement targetProperty = targetTypeElement.Properties.Single(p => p.Name == "Properties");

			Assert.AreEqual(expected, _typePrinter.Print(targetProperty.Member.PropertyType));
		}

		[Test]
		public void Print_Type_SystemAlias()
		{
			// public string InnerText
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a>";
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(IXmlComment));
			PropertyElement targetProperty = targetTypeElement.Properties.Single(p => p.Name == "InnerText");

			Assert.AreEqual(expected, _typePrinter.Print(targetProperty.Member.PropertyType));
		}

		[Test]
		public void Print_Type_Array()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.NamespaceElement.html\">NamespaceElement</a>[]";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(NamespaceElement[])));
		}

		[Test]
		public void Print_Type_SystemNamespace()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.type%28v=vs.110%29.aspx\">Type</a>";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(Type)));
		}

		[Test]
		public void Print_Type_GenericArgument()
		{
			const string expected = "TMember";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(DocumentationElement<>).GetProperty("Member").PropertyType));
		}

		[Test]
		public void Print_Type_NestedClass()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.TypePrinter+NameComponents.html\">TypePrinter.NameComponents</a>";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(TypePrinter.NameComponents)));
		}

		[Test]
		public void Print_Type_GenericTypeDefinition()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.MethodBaseElement%601.html\">MethodBaseElement</a>&lt;T&gt;";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(MethodBaseElement<>)));
		}

		[Test]
		public void Print_Type_CompleteGenericType()
		{
			const string expected = "<a href=\"T_IglooCastle.CLI.DocumentationElement%601.html\">DocumentationElement</a>&lt;" +
				"<a href=\"T_IglooCastle.CLI.TypePrinter.html\">TypePrinter</a>" +
				"&gt;";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(DocumentationElement<TypePrinter>)));
		}

		[Test]
		public void Print_Type_RefType()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.object%28v=vs.110%29.aspx\">object</a>";
			Assert.AreEqual(expected, _typePrinter.Print(typeof(object).MakeByRefType()));
		}

		[Test]
		public void Syntax_MethodInfo()
		{
			const string expected = "public <a href=\"http://msdn.microsoft.com/en-us/library/system.void%28v=vs.110%29.aspx\">void</a> Scan(<a href=\"http://msdn.microsoft.com/en-us/library/system.reflection.assembly%28v=vs.110%29.aspx\">Assembly</a> assembly)";
			var method = typeof(Documentation).GetMethod("Scan");
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_MethodInfo_Static()
		{
			const string expected = "public static <a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> Alias(<a href=\"http://msdn.microsoft.com/en-us/library/system.type%28v=vs.110%29.aspx\">Type</a> type)";
			var method = typeof(SystemTypes).GetMethod("Alias", new[] { typeof(Type) });
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_MethodInfo_Protected()
		{
			const string expected = "protected abstract <a href=\"T_IglooCastle.CLI.IXmlComment.html\">IXmlComment</a> GetXmlComment()";
			var method = typeof(ReflectedElement<>).GetMethod("GetXmlComment", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_MethodInfo_Interface()
		{
			const string expected = "<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> Section(<a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> sectionName)";
			var method = typeof(IXmlComment).GetMethod("Section", new[] { typeof(string) });
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_MethodInfo_Override()
		{
			const string expected = "public override <a href=\"http://msdn.microsoft.com/en-us/library/system.boolean%28v=vs.110%29.aspx\">bool</a> TryGetMember(<a href=\"http://msdn.microsoft.com/en-us/library/system.dynamic.getmemberbinder%28v=vs.110%29.aspx\">GetMemberBinder</a> binder, out <a href=\"http://msdn.microsoft.com/en-us/library/system.object%28v=vs.110%29.aspx\">object</a> result)";
			var method = typeof(DocumentationElement<>).GetMethod("TryGetMember");
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_MethodInfo_ProtectedOverride()
		{
			const string expected = "protected override <a href=\"T_IglooCastle.CLI.IXmlComment.html\">IXmlComment</a> GetXmlComment()";
			var method = typeof(PropertyElement).GetMethod("GetXmlComment", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_MethodInfo_Extension()
		{
			const string expected = "public static <a href=\"http://msdn.microsoft.com/en-us/library/system.string%28v=vs.110%29.aspx\">string</a> Summary(this <a href=\"T_IglooCastle.CLI.IXmlComment.html\">IXmlComment</a> xmlComment)";
			var method = typeof(XmlCommentExtensions).GetMethod("Summary");
			Assert.AreEqual(expected, _typePrinter.Syntax(method));
		}

		[Test]
		public void Syntax_PropertyInfo()
		{
			const string expected = "public TMember Member { get; }";
			Assert.AreEqual(expected, _typePrinter.Syntax(typeof(DocumentationElement<>).GetProperty("Member")));
		}
	}
}

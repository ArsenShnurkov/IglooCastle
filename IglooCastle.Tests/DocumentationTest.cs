using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using IglooCastle.CLI;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class DocumentationTest
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
		public void FindType()
		{
			var typeElement = _documentation.Find(typeof(Documentation));
			Assert.IsNotNull(typeElement);
			Assert.IsNotNull(typeElement.Filename());

			var sysTypeElement = _documentation.Find(typeof(Type));
			Assert.IsNotNull(sysTypeElement);
			Assert.IsNull(sysTypeElement.Filename());
		}
	}
}

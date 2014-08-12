using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IglooCastle.CLI;
using NUnit.Framework;

namespace IglooCastle.Tests
{
	[TestFixture]
    public class FilenameProviderTest
	{
		private Documentation _documentation;

		[SetUp]
		public void SetUp()
		{
			_documentation = new Documentation();
			_documentation.Scan(typeof(FilenameProvider).Assembly);
		}

		[Test]
		public void TestNamespace()
		{
			const string expected = "N_IglooCastle.CLI.html";
			FilenameProvider filenameProvider = new FilenameProvider();
			Assert.AreEqual(expected, filenameProvider.Filename(_documentation.Namespaces[0]));
		}

		[Test]
		public void TestMethodWithStringAlias()
		{
			// public string Filename(TypeElement type, string prefix)
			const string expected = "M_IglooCastle.CLI.FilenameProvider.Filename-IglooCastle.CLI.TypeElement_string.html";

			// locate type
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(FilenameProvider));

			Func<ParameterInfo[], bool> matchTargetMethod =
				parameters => parameters.Length == 2
					&& parameters[0].ParameterType == typeof(TypeElement)
					&& parameters[1].ParameterType == typeof(string);
			MethodElement targetMethod = targetTypeElement.Methods.Single(m => matchTargetMethod(m.Member.GetParameters()));

			FilenameProvider filenameProvider = new FilenameProvider();
			Assert.AreEqual(expected, filenameProvider.Filename(targetMethod));
		}
    }
}

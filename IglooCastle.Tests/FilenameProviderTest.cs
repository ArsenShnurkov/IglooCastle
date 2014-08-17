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
			// public string Filename(TypeElement type, string prefix, string suffix)
			const string expected = "M_IglooCastle.CLI.FilenameProvider.Filename-IglooCastle.CLI.TypeElement,string,string.html";

			// locate type
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(FilenameProvider));

			Func<ParameterInfo[], bool> matchTargetMethod =
				parameters => parameters.Length == 3
					&& parameters[0].ParameterType == typeof(TypeElement)
					&& parameters[1].ParameterType == typeof(string)
					&& parameters[2].ParameterType == typeof(string);
			MethodElement targetMethod = targetTypeElement.Methods.Single(m => matchTargetMethod(m.Member.GetParameters()));

			FilenameProvider filenameProvider = new FilenameProvider();
			Assert.AreEqual(expected, filenameProvider.Filename(targetMethod));
		}

		[Test]
		public void TestMethodWithParameterOfGenericType()
		{
			// public static ICollection<TypeElement> FilterTypes(this ITypeContainer typeContainer, Predicate<TypeElement> predicate)
			const string expected =
				"M_IglooCastle.CLI.TypeContainerExtensions.FilterTypes-IglooCastle.CLI.ITypeContainer,System.Predicate`IglooCastle.CLI.TypeElement.html";

			// locate type
			TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(TypeContainerExtensions));

			Func<ParameterInfo[], bool> matchTargetMethod =
				parameters => parameters.Length == 2
					&& parameters[0].ParameterType == typeof(ITypeContainer)
					&& parameters[1].ParameterType == typeof(Predicate<TypeElement>);
			MethodElement targetMethod = targetTypeElement.Methods.Single(m => matchTargetMethod(m.Member.GetParameters()));

			FilenameProvider filenameProvider = new FilenameProvider();
			Assert.AreEqual(expected, filenameProvider.Filename(targetMethod));
		}
	}
}

using NUnit.Framework;
using IglooCastle.CLI;

namespace IglooCastle.Tests
{
	[TestFixture]
	public class OptionsTest
	{
		[Test]
		public void ParseEmpty()
		{
			Options options = Options.Parse(new string[0]);
			Assert.IsNotNull(options);
		}

		[Test]
		public void ParseAssembly()
		{
			Options options = Options.Parse(new[] { "test.dll", "test2.dll" });
			CollectionAssert.AreEqual(
				new[] { "test.dll", "test2.dll" },
				options.InputAssemblies);
		}

		[Test]
		public void ParseOutputDirectory()
		{
			Options options = Options.Parse(new[] { "test.dll", "--output=../html" });
			CollectionAssert.AreEqual(
				new[] { "test.dll" },
				options.InputAssemblies);

			Assert.AreEqual("../html", options.OutputDirectory);
		}
	}
}

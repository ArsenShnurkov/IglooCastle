using IglooCastle.CLI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.Tests
{
    [TestFixture]
    public class TypePrinterTest
    {
        private Documentation _documentation;

        [SetUp]
        public void SetUp()
        {
            _documentation = new Documentation();
            _documentation.Scan(typeof(FilenameProvider).Assembly);
        }

        [Test]
        public void TestPropertyLink()
        {
            // public ICollection<PropertyElement> Properties
            const string expected =
                "System.Collections.Generic.ICollection&lt;" +
                "<a href=\"T_IglooCastle.CLI.PropertyElement.html\">PropertyElement</a>" +
                "&gt;";
            TypeElement targetTypeElement = _documentation.Types.Single(t => t.Member == typeof(TypeElement));
            PropertyElement targetProperty = targetTypeElement.Properties.Single(p => p.Name == "Properties");

            TypePrinter typePrinter = new TypePrinter(_documentation, new FilenameProvider());
            Assert.AreEqual(expected, typePrinter.Print(targetProperty.Member.PropertyType));
        }
    }
}

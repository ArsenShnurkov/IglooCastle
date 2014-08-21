﻿using IglooCastle.CLI;
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
	public class TypeElementTest
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
		public void TestName()
		{
			var typeElement = _documentation.Find(typeof(TypeElement));
			Assert.IsNotNull(typeElement);
			Assert.AreEqual("TypeElement", typeElement.Name);
		}
	}
}

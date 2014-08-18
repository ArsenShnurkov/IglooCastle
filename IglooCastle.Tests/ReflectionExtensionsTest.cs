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
	[TestFixture]
	public class ReflectionExtensionsTest
	{
		[Test]
		public void GetAccess_MethodInfo_Public()
		{
			var method = typeof(Documentation).GetMethod("Normalize", new[] { typeof(TypeElement) });
			Assert.AreEqual(MethodAttributes.Public, method.GetAccess());
		}
	}
}

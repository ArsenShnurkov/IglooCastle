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

		public sealed class NestedSample
		{
			public sealed class SecondLevelNest
			{

			}
		}
	}
}

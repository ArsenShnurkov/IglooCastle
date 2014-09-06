using System;

namespace IglooCastle.Demo
{
	/// <summary>
	/// Basic calculator demo.
	/// </summary>
	public sealed class CalculatorDemo
	{
		/// <summary>
		/// Adds two numbers.
		/// </summary>
		/// <param name="x">The first number to add.</param>
		/// <param name="y">The second number to add.</param>
		/// <returns>The sum of the two parameters.</returns>
		public int Add(int x, int y)
		{
			return x + y;
		}
	}

	public sealed class NestingDemo
	{
		private NestingDemo() {}

		public sealed class NestedDemo
		{
			private NestedDemo() { }
		}
	}

	public sealed class DemoMultipleConstructors
	{
		public DemoMultipleConstructors()
		{
		}

		public DemoMultipleConstructors(int someValue)
		{
		}
	}

	public interface IVariance<in T1, out T2>
	{
	}

	public class GenericConstraints<T1, T2, T3> : IVariance<T1, T2>
		where T1 : class
		where T2 : T1, new()
		where T3 : struct
	{

	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public sealed class DemoAttribute : Attribute
	{
		private readonly string _name;

		public DemoAttribute()
		{
		}

		public DemoAttribute(string name)
		{
			_name = name;
		}

		public string Name
		{
			get { return _name; }
		}

		public int Size { get; set; }
	}

	[Demo]
	public class AnnotatedDemo
	{
		[Demo("name")]
		public string Name { get; set; }

		[Demo("name", Size = 42)]
		public void Test()
		{ }
	}
}

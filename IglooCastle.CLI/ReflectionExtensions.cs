using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
	/// <summary>
	/// Extension methods to help with reflection.
	/// </summary>
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Checks if this method is an extension method.
		/// </summary>
		/// <param name="method">This method.</param>
		/// <returns><c>true</c> if this is an extension method, <c>false</c> otherwise.</returns>
		public static bool IsExtension(this MethodInfo method)
		{
			bool isExtension = method.GetCustomAttribute<ExtensionAttribute>() != null;
			return isExtension;
		}
	}
}

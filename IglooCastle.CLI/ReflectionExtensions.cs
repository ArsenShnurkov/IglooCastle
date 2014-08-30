using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
		public static MethodAttributes GetAccess(this MethodBase method)
		{
			return method.Attributes & (MethodAttributes.Public | MethodAttributes.Family | MethodAttributes.Assembly | MethodAttributes.FamANDAssem | MethodAttributes.FamORAssem);
		}

		public static MethodAttributes GetAccess(this PropertyInfo property)
		{
			MethodAttributes getterAccess = property.CanRead ? property.GetMethod.GetAccess() : MethodAttributes.PrivateScope;
			MethodAttributes setterAccess = property.CanWrite ? property.SetMethod.GetAccess() : MethodAttributes.PrivateScope;
			return Max(getterAccess, setterAccess);
		}

		public static string ToAccessString(this MethodAttributes access)
		{
			switch (access)
			{
				case MethodAttributes.Family:
					return "protected";
				case MethodAttributes.Public:
					return "public";
				default:
					// TODO: more options + tests
					return access.ToString();
			}
		}

		public static MethodAttributes Max(MethodAttributes a, MethodAttributes b)
		{
			if (a == b)
			{
				return a;
			}

			MethodAttributes[] order = new[]
			{
				MethodAttributes.PrivateScope,
				MethodAttributes.Private,
				MethodAttributes.FamANDAssem,
				MethodAttributes.Assembly,
				MethodAttributes.Family,
				MethodAttributes.FamORAssem,
				MethodAttributes.Public
			};

			for (int idx = order.Length - 1; idx >= 0; idx--)
			{
				if (a == order[idx])
				{
					return a;
				}

				if (b == order[idx])
				{
					return b;
				}
			}

			return MethodAttributes.PrivateScope;
		}

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

		public static bool IsOverload(this MethodInfo method)
		{
			return method.ReflectedType.GetPublicAndProtectedMethods().Count(m => m.Name == method.Name) >= 2;
		}

		public static bool IsOverride(this MethodInfo method)
		{
			for (Type t = method.DeclaringType.BaseType; t != null; t = t.BaseType)
			{
				if (t.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, method.GetParameters().Select(p => p.ParameterType).ToArray(), null) != null)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the public and protected constructors of this type.
		/// </summary>
		/// <remarks>
		/// Typically, public and protected members are the ones that need documentation.
		/// </remarks>
		/// <param name="type">This type.</param>
		/// <returns>A collection of <see cref="ConstructorInfo"/> instances.</returns>
		public static IEnumerable<ConstructorInfo> GetPublicAndProtectedConstructors(this Type type)
		{
			return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(c => c.IsPublic || c.IsFamily);
		}

		/// <summary>
		/// Gets the public and protected properties of this type.
		/// </summary>
		/// <remarks>
		/// Typically, public and protected members are the ones that need documentation.
		/// </remarks>
		/// <param name="type">This type.</param>
		/// <returns>A collection of <see cref="PropertyInfo"/> instances.</returns>
		public static IEnumerable<PropertyInfo> GetPublicAndProtectedProperties(this Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
						   .Where(p => !p.IsSpecialName && (p.IsPublic() || p.IsFamily()));
		}

		/// <summary>
		/// Gets the public and protected methods of this type.
		/// </summary>
		/// <remarks>
		/// Typically, public and protected members are the ones that need documentation.
		/// </remarks>
		/// <param name="type">This type.</param>
		/// <returns>A collection of <see cref="MethodInfo"/> instances.</returns>
		public static IEnumerable<MethodInfo> GetPublicAndProtectedMethods(this Type type)
		{
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(m => !m.IsSpecialName && (m.IsPublic || m.IsFamily));
		}

		/// <summary>
		/// Checks if this property is public.
		/// A property is public if at least one of the accessors is public.
		/// </summary>
		/// <param name="property">This property.</param>
		/// <returns><c>true</c> if at least one of the property accessors is public; <c>false</c> otherwise.</returns>
		public static bool IsPublic(this PropertyInfo property)
		{
			return (property.CanRead && property.GetMethod.IsPublic)
				|| (property.CanWrite && property.SetMethod.IsPublic);
		}

		/// <summary>
		/// Checks if this property is protected.
		/// A property is protected if at least one of the accessors is protected and none of the accessors is public.
		/// </summary>
		/// <param name="property">This property.</param>
		/// <returns><c>true</c> if at least one of the property accessors is protected while neither is public; <c>false</c> otherwise.</returns>
		public static bool IsFamily(this PropertyInfo property)
		{
			return !property.IsPublic() && 
				((property.CanRead && property.GetMethod.IsFamily)
				|| (property.CanWrite && property.SetMethod.IsFamily));
		}

		public static bool IsStatic(this PropertyInfo property)
		{
			return (property.CanRead && property.GetMethod.IsStatic)
				|| (property.CanWrite && property.SetMethod.IsStatic);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.CLI
{
	public static class TypeContainerExtensions
	{
		public static ICollection<TypeElement> Classes(this ITypeContainer typeContainer)
		{
			return typeContainer.FilterTypes(t => t.Type.IsClass);
		}

		public static ICollection<TypeElement> Interfaces(this ITypeContainer typeContainer)
		{
			return typeContainer.FilterTypes(t => t.Type.IsInterface);
		}

		public static ICollection<TypeElement> ValueTypes(this ITypeContainer typeContainer)
		{
			return typeContainer.FilterTypes(t => t.Type.IsValueType);
		}

		public static ICollection<TypeElement> Enums(this ITypeContainer typeContainer)
		{
			return typeContainer.FilterTypes(t => t.Type.IsEnum);
		}

		public static ICollection<TypeElement> FilterTypes(this ITypeContainer typeContainer, Predicate<TypeElement> predicate)
		{
			return typeContainer.Types.Where(t => predicate(t)).OrderBy(t => t.Name).ToList();
		}
	}
}

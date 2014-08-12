using System.Collections.Generic;

namespace IglooCastle.CLI
{
	public interface ITypeContainer
	{
		ICollection<TypeElement> Types { get; }
	}
}

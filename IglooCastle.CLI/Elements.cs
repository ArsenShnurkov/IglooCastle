using System;

namespace IglooCastle.CLI
{
	[Flags]
	public enum Elements
	{
		None = 0,
		Namespace = 1,
		Type = 2,
		Constructor = 4,
		Property = 8,
		Method = 16
	}
}

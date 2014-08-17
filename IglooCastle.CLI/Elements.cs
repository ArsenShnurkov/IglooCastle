using System;

namespace IglooCastle.CLI
{
	/// <summary>
	/// Defines the various elements that can be returned as documentation elements.
	/// </summary>
	[Flags]
	public enum Elements
	{
		None = 0,

		/// <summary>
		/// A namespace element.
		/// </summary>
		Namespace = 1,
		Type = 2,
		Constructor = 4,
		Property = 8,
		Method = 16
	}
}

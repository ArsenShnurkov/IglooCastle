﻿using System;
using System.Dynamic;

namespace IglooCastle.CLI
{
	/// <summary>
	/// This is the root of all documentation elements.
	/// </summary>
	/// <typeparam name="TMember">The type of the member that this class documents.</typeparam>
	public abstract class DocumentationElement<TMember>
	{
		private readonly Documentation _documentation;

		protected DocumentationElement(Documentation documentation, TMember member)
		{
			_documentation = documentation;
			Member = member;
		}

		/// <summary>
		/// Gets the XML comment that documents this code element.
		/// </summary>
		/// <remarks>
		/// This value will never be <c>null</c>.
		/// </remarks>
		/// <value>The XML comment.</value>
		public abstract IXmlComment XmlComment { get; }

		public TMember Member { get; private set; }

		public Documentation Documentation
		{
			get { return _documentation; }
		}
	}
}

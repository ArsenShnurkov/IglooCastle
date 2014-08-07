namespace IglooCastle.CLI
{
	/// <summary>
	/// This is the root of all documentation elements.
	/// </summary>
	public abstract class DocumentationElement<T>
	{
		private XmlComment _xmlComment;

		/// <summary>
		/// Gets or sets the XML comment that documents this code element.
		/// </summary>
		/// <remarks>
		/// This value will never be <c>null</c>.
		/// </remarks>
		/// <value>The XML comment.</value>
		public XmlComment XmlComment
		{
			get { return _xmlComment ?? new MissingXmlComment(); }
			set { _xmlComment = value; }
		}

		public T Member { get; set; }
	}
}

namespace IglooCastle.CLI
{
	public sealed class MissingXmlComment : IXmlComment
	{
		public string Section(string name)
		{
			return string.Empty;
		}

		public string InnertText
		{
			get { return string.Empty; }
		}
	}
}

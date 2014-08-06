namespace IglooCastle.CLI
{
	public class MissingXmlComment : XmlComment
	{
		public MissingXmlComment() : base(null)
		{
		}

		public override string Section(string name)
		{
			return string.Empty;
		}

		public override string InnertText
		{
			get { return string.Empty; }
		}
	}
}
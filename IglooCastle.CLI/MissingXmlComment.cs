namespace IglooCastle.CLI
{
	public sealed class MissingXmlComment : IXmlComment
	{
		public string Section(string name)
		{
			return string.Empty;
		}

		public string InnerText
		{
			get { return string.Empty; }
		}

		public string Section(string sectionName, string attributeName, string attributeValue)
		{
			return string.Empty;
		}
	}
}

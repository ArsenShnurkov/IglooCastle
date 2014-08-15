namespace IglooCastle.CLI
{
	public interface IXmlComment
	{
		string InnerText { get; }

		string Section(string sectionName);

		string Section(string sectionName, string attributeName, string attributeValue);
	}
}

namespace IglooCastle.CLI
{
	public interface IXmlComment
	{
		string InnerText { get; }

		string Section(string name);
	}
}

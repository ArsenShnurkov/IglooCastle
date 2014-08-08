namespace IglooCastle.CLI
{
	public interface IXmlComment
	{
		string InnertText { get; }

		string Section(string name);
	}
}

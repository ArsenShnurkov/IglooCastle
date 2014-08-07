namespace IglooCastle.CLI
{
	public class NamespaceElement : DocumentationElement<string>
	{
		public string Namespace
		{
			get { return Member; }
			set { Member = value; }
		}
	}
}

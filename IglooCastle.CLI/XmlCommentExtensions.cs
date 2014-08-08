namespace IglooCastle.CLI
{
	public static class XmlCommentExtensions
	{
		public static string Summary(this IXmlComment xmlComment)
		{
			return xmlComment.Section("summary");
		}
	}
}

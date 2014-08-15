namespace IglooCastle.CLI
{
	public static class XmlCommentExtensions
	{
		public static string Summary(this IXmlComment xmlComment)
		{
			return xmlComment.Section("summary");
		}

		public static string Param(this IXmlComment xmlComment, string paramName)
		{
			return xmlComment.Section("param", "name", paramName);
		}
	}
}

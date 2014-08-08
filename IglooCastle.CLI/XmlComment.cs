using System.Xml;

namespace IglooCastle.CLI
{
	public sealed class XmlComment : IXmlComment
	{
		private readonly XmlElement _documentationNode;

		public XmlComment(XmlElement documentationNode)
		{
			_documentationNode = documentationNode;
		}

		public string Section(string name)
		{
			return _documentationNode.SelectSingleNode(name).InnerXml;
		}

		public string InnertText
		{
			get { return _documentationNode.InnerText; }
		}
	}
}

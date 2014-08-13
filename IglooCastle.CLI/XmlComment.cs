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
			XmlNode node = _documentationNode.SelectSingleNode(name);
			return node != null ? node.InnerXml : string.Empty;
		}

		public string InnerText
		{
			get { return _documentationNode.InnerText; }
		}
	}
}

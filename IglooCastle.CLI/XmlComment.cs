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
			return FormatComment(_documentationNode.SelectSingleNode(name));
		}

		public string Section(string sectionName, string attributeName, string attributeValue)
		{
			return FormatComment(
				_documentationNode.SelectSingleNode(
					string.Format("{0}[@{1}=\"{2}\"]", sectionName, attributeName, attributeValue)));
		}

		public string InnerText
		{
			get { return _documentationNode.InnerText; }
		}

		private string FormatComment(XmlNode node)
		{
			return node != null ? FormatComment(node.InnerXml ?? string.Empty) : string.Empty;
		}

		private string FormatComment(string innerXml)
		{
			return innerXml.Trim();
		}
	}
}

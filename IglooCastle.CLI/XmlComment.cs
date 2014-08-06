using System.Xml;

namespace IglooCastle.CLI
{
	public class XmlComment
	{
		private readonly XmlElement _documentationNode;

		public XmlComment(XmlElement documentationNode)
		{
			_documentationNode = documentationNode;
		}

		public virtual string Section(string name)
		{
			return _documentationNode.SelectSingleNode(name).InnerXml;
		}

		public string Summary
		{
			get { return Section("summary"); }
		}

		public virtual string InnertText
		{
			get { return _documentationNode.InnerText; }
		}
	}
}
using System;
using System.Xml;

namespace IglooCastle.CLI
{
	/// <summary>
	/// This is the root of all documentation elements.
	/// </summary>
	public class DocumentationElement
	{
		private XmlComment _xmlComment;

		/// <summary>
		/// Gets or sets the XML comment that documents this code element.
		/// </summary>
		/// <remarks>
		/// This value will never be <c>null</c>.
		/// </remarks>
		/// <value>The XML comment.</value>
		public XmlComment XmlComment
		{
			get { return _xmlComment ?? new MissingXmlComment(); }
			set { _xmlComment = value; }
		}
	}

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

	public class MissingXmlComment : XmlComment
	{
		public MissingXmlComment() : base(null)
		{
		}

		public override string Section(string name)
		{
			return string.Empty;
		}

		public override string InnertText
		{
			get { return string.Empty; }
		}
	}
}
﻿using System;
using System.Text;
using System.Xml;

namespace IglooCastle.CLI
{
	public sealed class XmlComment : IXmlComment
	{
		private readonly XmlElement _documentationNode;
		private readonly Documentation _documentation;

		public XmlComment(Documentation documentation, XmlElement documentationNode)
		{
			_documentation = documentation;
			_documentationNode = documentationNode;
		}

		public string Section(string name)
		{
			return FormatComment(_documentationNode.SelectSingleNode(name)).Trim();
		}

		public string Section(string sectionName, string attributeName, string attributeValue)
		{
			return FormatComment(
				_documentationNode.SelectSingleNode(
					string.Format("{0}[@{1}=\"{2}\"]", sectionName, attributeName, attributeValue))).Trim();
		}

		public string InnerText
		{
			get { return _documentationNode.InnerText; }
		}

		private string FormatChildren(XmlNode node)
		{
			if (node == null)
			{
				return string.Empty;
			}

			StringBuilder builder = new StringBuilder();
			foreach (XmlNode n in node.ChildNodes)
			{
				builder.Append(FormatComment(n));
			}

			return builder.ToString();
		}

		private string FormatComment(XmlNode node)
		{
			if (node == null)
			{
				return string.Empty;
			}

			if (node is XmlText)
			{
				return ((XmlText)node).Value;
			}

			if (node is XmlElement)
			{
				if (node.Name == "c")
				{
					return "<code>" + FormatChildren(node) + "</code>"; 
				}

				if (node.Name == "see")
				{
					string cref = node.Attributes["cref"].Value;
					object resolvedCref = ResolveCref(cref);
					if (resolvedCref is TypeElement)
					{
						return new TypePrinter(_documentation).Print((TypeElement)resolvedCref);
					}

					return string.Format("<code>{0}</code>", cref);
				}
			}

			return FormatChildren(node);
		}

		private object ResolveCref(string cref)
		{
			string[] parts = cref.Split(':');
			if (parts[0] == "T")
			{
				return ResolveTypeCref(parts[1]);
			}

			// TODO: implement
			return null;
		}

		private TypeElement ResolveTypeCref(string typeName)
		{
			Type resolvedType = Type.GetType(typeName, false);
			if (resolvedType == null)
			{
				Console.WriteLine("Could not resolve type {0}", typeName);
				return null;
			}

			return _documentation.Find(resolvedType);
		}
	}
}

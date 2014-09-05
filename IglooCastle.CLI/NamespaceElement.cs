using System;
using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.CLI
{
	public class NamespaceElement : DocumentationElement<string>, ITypeContainer, IFormattable
	{
		/// <summary>
		/// Creates an instance of this class.
		/// </summary>
		/// <param name="documentation">The owner documentation instance.</param>
		/// <param name="name">The name of the namespace.</param>
		public NamespaceElement(Documentation documentation, string name)
			: base(documentation, name)
		{
		}

		public string Namespace
		{
			get { return Member; }
		}

		public override IXmlComment XmlComment
		{
			get
			{
				// TODO: figure out where to read namespace comments from
				return new MissingXmlComment();
			}
		}

		public string ToHtml()
		{
			return new NamespacePrinter(Documentation).Print(this);
		}

		/// <summary>
		/// Gets the types of the documentation that belong to this namespace.
		/// </summary>
		public ICollection<TypeElement> Types
		{
			get { return Documentation.FilterTypes(t => t.Type.Namespace == Namespace); }
		}

		public ICollection<MethodElement> Methods
		{
			get
			{
				return Types.SelectMany(m => m.Methods).ToList();
			}
		}

		public string Filename(string prefix = "N")
		{
			return Documentation.FilenameProvider.Filename(this, prefix);
		}

		#region IFormattable implementation

		public override string ToString(string format, IFormatProvider formatProvider)
		{
			if (format == "h")
			{
				return ToHtml();
			}

			throw new FormatException();
		}

		#endregion
	}
}

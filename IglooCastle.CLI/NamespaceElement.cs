using System.Collections.Generic;
using System.Linq;

namespace IglooCastle.CLI
{
	public class NamespaceElement : DocumentationElement<string>
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

		/// <summary>
		/// Gets the types of the documentation that belong to this namespace.
		/// </summary>
		public IEnumerable<TypeElement> Types
		{
			get { return Documentation.Types.Where(t => t.Type.Namespace == Namespace).OrderBy(t => t.Type.Name); }
		}
	}
}

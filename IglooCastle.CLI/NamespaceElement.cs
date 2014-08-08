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
		public NamespaceElement(Documentation documentation)
		{
			Documentation = documentation;
		}

		public string Namespace
		{
			get { return Member; }
			set { Member = value; }
		}

		private Documentation Documentation { get; set; }

		/// <summary>
		/// Gets the types of the documentation that belong to this namespace.
		/// </summary>
		public IEnumerable<TypeElement> Types
		{
			get { return Documentation.Types.Where(t => t.Type.Namespace == Namespace).OrderBy(t => t.Type.Name); }
		}
	}
}

using System;
using System.Dynamic;

namespace IglooCastle.CLI
{
	/// <summary>
	/// This is the root of all documentation elements.
	/// </summary>
	public abstract class DocumentationElement<T> : DynamicObject
	{
		private readonly Documentation _documentation;

		protected DocumentationElement(Documentation documentation, T member)
		{
			_documentation = documentation;
			Member = member;
		}

		/// <summary>
		/// Gets the XML comment that documents this code element.
		/// </summary>
		/// <remarks>
		/// This value will never be <c>null</c>.
		/// </remarks>
		/// <value>The XML comment.</value>
		public abstract IXmlComment XmlComment { get; }

		public T Member { get; private set; }

		public Documentation Documentation
		{
			get { return _documentation; }
		}

		public override bool TryGetMember(
			GetMemberBinder binder, out object result)
		{
			string name = binder.Name;
			var pi = typeof(T).GetProperty(name);
			var hasProperty = pi != null && pi.CanRead;
			result = hasProperty ? pi.GetValue(Member) : null;
			return hasProperty;
		}
	}
}

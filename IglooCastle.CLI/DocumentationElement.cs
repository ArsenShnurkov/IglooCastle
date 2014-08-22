using System;
using System.Dynamic;

namespace IglooCastle.CLI
{
	/// <summary>
	/// This is the root of all documentation elements.
	/// </summary>
	/// <typeparam name="TMember">The type of the member that this class documents.</typeparam>
	public abstract class DocumentationElement<TMember> : IFormattable
	{
		private readonly Documentation _documentation;

		protected DocumentationElement(Documentation documentation, TMember member)
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

		public TMember Member { get; private set; }

		public Documentation Documentation
		{
			get { return _documentation; }
		}

		public override bool Equals(object o)
		{
			if (ReferenceEquals(o, null))
			{
				return false;
			}

			if (ReferenceEquals(o, this))
			{
				return true;
			}

			if (o.GetType() != GetType())
			{
				return false;
			}

			DocumentationElement<TMember> that = (DocumentationElement<TMember>)o;
			return Member.Equals(that.Member);
		}

		public override int GetHashCode()
		{
			return Member.GetHashCode();
		}

		public virtual string ToString(string format, IFormatProvider formatProvider)
		{
			return Member.ToString();
		}

		public string ToString(string format)
		{
			return ToString(format, null);
		}

		public sealed override string ToString()
		{
			return ToString(null);
		}
	}
}

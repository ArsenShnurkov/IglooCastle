using System;
using System.Linq;
using System.Text;

namespace IglooCastle.CLI
{
	static class StringExtensions
	{
		public static string Escape(this string link)
		{
			return link.Replace("`", "%60");
		}

		public static string JoinNonEmpty(this string separator, params string[] args)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string s in args.Where(arg => !string.IsNullOrWhiteSpace(arg)))
			{
				if (builder.Length > 0)
				{
					builder.Append(separator);
				}

				builder.Append(s);
			}

			return builder.ToString();
		}
	}
}

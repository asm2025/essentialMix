using System.Text;
using System.Xml;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class XmlNamespaceManagerExtension
	{
		public static int Append([NotNull] this XmlNamespaceManager thisValue, [NotNull] params string[] namespaceURI)
		{
			if (namespaceURI.IsNullOrEmpty()) return 0;

			int n = 0;

			foreach (string s in namespaceURI)
			{
				if (string.IsNullOrEmpty(s))
					continue;

				int p = s.IndexOf('|');

				if (p > 0 && p < s.Length)
				{
					string[] names = s.Split(2, '|');
					int p2 = names[1].IndexOf('|');
					thisValue.AddNamespace(names[0], p2 > -1 ? names[1].Replace("|", string.Empty) : names[1]);
					++n;
				}
				else
				{
					thisValue.AddNamespace(string.Empty, p > -1 ? s.Replace("|", string.Empty) : s);
					++n;
				}
			}

			return n;
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNamespaceManager thisValue) { return CreateParserContext(thisValue, true, null, null); }

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNamespaceManager thisValue, bool ignoreWhitespace)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, null);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNamespaceManager thisValue, bool ignoreWhitespace, [NotNull] Encoding encoding)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, encoding);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNamespaceManager thisValue, bool ignoreWhitespace, XmlNameTable nt, [NotNull] Encoding encoding)
		{
			return new XmlParserContext(thisValue.NameTable, thisValue, null, ignoreWhitespace
																				? XmlSpace.None
																				: XmlSpace.Preserve, encoding.GetWebEncoding());
		}
	}
}
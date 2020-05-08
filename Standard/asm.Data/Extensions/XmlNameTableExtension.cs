using System.Text;
using System.Xml;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class XmlNameTableExtension
	{
		[NotNull] public static XmlNamespaceManager GetNamespaceManager([NotNull] this XmlNameTable thisValue) { return new XmlNamespaceManager(thisValue); }

		[NotNull] public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue) { return CreateParserContext(thisValue, true, null, null); }

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue, bool ignoreWhitespace)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, null);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue, bool ignoreWhitespace, [NotNull] Encoding encoding)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, encoding);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue, bool ignoreWhitespace, XmlNamespaceManager manager, [NotNull] Encoding encoding)
		{
			return new XmlParserContext(thisValue, manager ?? GetNamespaceManager(thisValue), null, ignoreWhitespace
																										? XmlSpace.None
																										: XmlSpace.Preserve, encoding.GetWebEncoding());
		}

		public static int Append([NotNull] this XmlNameTable thisValue, [NotNull] params string[] namespaceURI)
		{
			return namespaceURI.IsNullOrEmpty()
						? 0
						: GetNamespaceManager(thisValue).Append(namespaceURI);
		}
	}
}
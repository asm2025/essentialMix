using System.Text;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XmlNameTableExtension
	{
		[NotNull]
		public static XmlNamespaceManager GetNamespaceManager([NotNull] this XmlNameTable thisValue) { return new XmlNamespaceManager(thisValue); }

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue) { return CreateParserContext(thisValue, true, null, null); }

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue, bool ignoreWhitespace)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, null);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue, bool ignoreWhitespace, Encoding encoding)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, encoding);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this XmlNameTable thisValue, bool ignoreWhitespace, XmlNamespaceManager manager, Encoding encoding)
		{
			return new XmlParserContext(thisValue, manager ?? GetNamespaceManager(thisValue), null, ignoreWhitespace
																										? XmlSpace.None
																										: XmlSpace.Preserve, (encoding ?? Encoding.UTF8).GetWebEncoding());
		}

		public static int Append([NotNull] this XmlNameTable thisValue, [NotNull] params string[] namespaceURI)
		{
			return namespaceURI.IsNullOrEmpty()
						? 0
						: GetNamespaceManager(thisValue).Append(namespaceURI);
		}
	}
}
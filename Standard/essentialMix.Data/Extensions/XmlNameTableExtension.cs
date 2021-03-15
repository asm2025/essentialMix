using System.Text;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XmlNameTableExtension
	{
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
			return new XmlParserContext(thisValue, manager, null, ignoreWhitespace
																	? XmlSpace.None
																	: XmlSpace.Preserve, (encoding ?? Encoding.UTF8).GetWebEncoding());
		}
	}
}
using System.Text;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class NameTableExtension
	{
		public static XmlNamespaceManager GetNamespaceManager(this NameTable thisValue)
		{
			return thisValue == null
						? null
						: new XmlNamespaceManager(thisValue);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this NameTable thisValue) { return CreateParserContext(thisValue, true, null, null); }

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this NameTable thisValue, bool ignoreWhitespace)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, null);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this NameTable thisValue, bool ignoreWhitespace, Encoding encoding)
		{
			return CreateParserContext(thisValue, ignoreWhitespace, null, encoding);
		}

		[NotNull]
		public static XmlParserContext CreateParserContext([NotNull] this NameTable thisValue, bool ignoreWhitespace, XmlNamespaceManager manager, Encoding encoding)
		{
			return new XmlParserContext(thisValue, manager ?? GetNamespaceManager(thisValue), null, ignoreWhitespace
																										? XmlSpace.None
																										: XmlSpace.Preserve, (encoding ?? Encoding.UTF8).GetWebEncoding());
		}

		public static int Append([NotNull] this NameTable thisValue, params string[] namespaceURI) { return GetNamespaceManager(thisValue)?.Append(namespaceURI) ?? 0; }
	}
}
using System;
using System.IO;
using System.Text;
using System.Xml;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Data.Helpers
{
	public static class XmlDocumentHelper
	{
		[NotNull]
		public static string CreateDeclarationString() { return CreateDeclarationString(EncodingHelper.Default, true, 1.0f); }

		[NotNull]
		public static string CreateDeclarationString(Encoding encoding) { return CreateDeclarationString(encoding, true, 1.0f); }

		[NotNull]
		public static string CreateDeclarationString(Encoding encoding, bool standAlone) { return CreateDeclarationString(encoding, standAlone, 1.0f); }

		[NotNull]
		public static string CreateDeclarationString(Encoding encoding, bool standAlone, float version)
		{
			if (Math.Abs(version - 1.0f) > 0.001f) throw new ArgumentException("Currently the version is preserved and must be set to 1.0", nameof(version));
			
			StringBuilder sb = new StringBuilder("<?xml version=");
			sb.AppendFormat("{0:F1}", version);
			sb.AppendFormat(" encoding=\"{0}\"", (encoding ?? EncodingHelper.Default).WebName);
			if (standAlone) sb.Append(" standAlone=\"yes\"");
			return sb.ToString();
		}

		[NotNull]
		public static XmlDocument Create([NotNull] params string[] namespaceURI) { return Create(true, EncodingHelper.Default, true, 1.0f, namespaceURI); }

		[NotNull]
		public static XmlDocument Create(bool ignoreWhiteSpace, [NotNull] params string[] namespaceURI) { return Create(ignoreWhiteSpace, EncodingHelper.Default, true, 1.0f, namespaceURI); }

		[NotNull]
		public static XmlDocument Create(bool ignoreWhiteSpace, Encoding encoding, [NotNull] params string[] namespaceURI)
		{
			return Create(ignoreWhiteSpace, encoding, true, 1.0f, namespaceURI);
		}

		[NotNull]
		public static XmlDocument Create(bool ignoreWhiteSpace, Encoding encoding, bool standAlone, [NotNull] params string[] namespaceURI)
		{
			return Create(ignoreWhiteSpace, encoding, standAlone, 1.0f, namespaceURI);
		}

		[NotNull]
		public static XmlDocument Create(bool ignoreWhiteSpace, Encoding encoding, bool standAlone, float version, [NotNull] params string[] namespaceURI)
		{
			XmlDocument document = new XmlDocument {PreserveWhitespace = !ignoreWhiteSpace};
			document.AppendDeclaration(encoding, standAlone, version);
			document.AppendNamespaces(namespaceURI);
			return document;
		}

		[NotNull]
		public static XmlDocument LoadFile([NotNull] string filename) { return LoadFile(filename, null, null); }

		[NotNull]
		public static XmlDocument LoadFile([NotNull] string filename, XmlReaderSettings settings) { return LoadFile(filename, settings, null); }

		[NotNull]
		public static XmlDocument LoadFile([NotNull] string filename, XmlReaderSettings settings, XmlParserContext context)
		{
			XmlDocument document = new XmlDocument();
			document.LoadFile(filename);
			return document;
		}
	}
}

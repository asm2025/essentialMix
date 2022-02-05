using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Data.Helpers;

public static class XDocumentHelper
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
	public static XDocument Create(params object[] content) { return Create(EncodingHelper.Default, true, 1.0f, content); }

	[NotNull]
	public static XDocument Create(Encoding encoding, params object[] content) { return Create(encoding, true, 1.0f, content); }

	[NotNull]
	public static XDocument Create(Encoding encoding, bool standAlone, params object[] content) { return Create(encoding, standAlone, 1.0f, content); }

	[NotNull]
	public static XDocument Create(Encoding encoding, bool standAlone, float version, params object[] content)
	{
		XDocument document = new XDocument();
		document.AppendDeclaration(encoding, standAlone, version);
		if (!content.IsNullOrEmpty()) document.Add(content);
		return document;
	}

	[NotNull]
	public static XDocument LoadFile([NotNull] string filename) { return LoadFile(filename, null, null); }

	[NotNull]
	public static XDocument LoadFile([NotNull] string filename, XmlReaderSettings settings) { return LoadFile(filename, settings, null); }

	[NotNull]
	public static XDocument LoadFile([NotNull] string filename, XmlReaderSettings settings, XmlParserContext context)
	{
		if (filename == null) throw new ArgumentNullException(nameof(filename));
		if (!File.Exists(filename)) throw new FileNotFoundException("File not found", filename);

		XDocument document;
		XmlReaderSettings options = settings ?? XmlReaderHelper.CreateSettings();

		using (XmlReader reader = context == null ? XmlReader.Create(filename, options) : XmlReader.Create(filename, options, context))
			document = XDocument.Load(reader, LoadOptions.SetBaseUri);

		return document;
	}

	[NotNull]
	public static XDocument LoadFile([NotNull] string filename, LoadOptions options)
	{
		if (!File.Exists(filename)) throw new FileNotFoundException("File not found", filename);

		XDocument document = XDocument.Load(filename, options);
		return document;
	}
}
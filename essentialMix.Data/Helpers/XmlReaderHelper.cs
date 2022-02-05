using System.Text;
using System.Xml;
using System.Xml.Schema;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Data.Helpers;

public static class XmlReaderHelper
{
	[NotNull]
	public static XmlReaderSettings CreateSettings()
	{
		return CreateSettings(false, false, true, false, ConformanceLevel.Auto, DtdProcessing.Parse, null, Constants.XML_SIMPLE_VALIDATION_FLAGS, ValidationType.None, null,
							null);
	}

	[NotNull]
	public static XmlReaderSettings CreateSettings(XmlSchemaValidationFlags validationFlags, ValidationType validationType)
	{
		return CreateSettings(false, false, true, false, ConformanceLevel.Auto, DtdProcessing.Parse, null, validationFlags, validationType, null, null);
	}

	[NotNull]
	public static XmlReaderSettings CreateSettings(bool ignoreWhitespace, bool ignoreComments, XmlSchemaValidationFlags validationFlags,
		ValidationType validationType)
	{
		return CreateSettings(ignoreWhitespace, ignoreComments, true, false, ConformanceLevel.Auto, DtdProcessing.Parse, null, validationFlags, validationType, null,
							null);
	}

	[NotNull]
	public static XmlReaderSettings CreateSettings(bool ignoreWhitespace, bool ignoreComments, bool checkCharacters, bool ignoreProcessingInstructions)
	{
		return CreateSettings(ignoreWhitespace, ignoreComments, checkCharacters, ignoreProcessingInstructions, ConformanceLevel.Auto, DtdProcessing.Parse, null,
							Constants.XML_SIMPLE_VALIDATION_FLAGS, ValidationType.None, null, null);
	}

	[NotNull]
	public static XmlReaderSettings CreateSettings(ConformanceLevel level, DtdProcessing dtdProcessing, XmlUrlResolver resolver,
		XmlSchemaValidationFlags validationFlags, ValidationType validationType)
	{
		return CreateSettings(false, false, true, false, level, dtdProcessing, resolver, validationFlags, validationType, null, null);
	}

	[NotNull]
	public static XmlReaderSettings CreateSettings(ConformanceLevel level, DtdProcessing dtdProcessing, XmlUrlResolver resolver,
		XmlSchemaValidationFlags validationFlags, ValidationType validationType, XmlSchemaSet schemaSet, ValidationEventHandler onErrorHandler)
	{
		return CreateSettings(false, false, true, false, level, dtdProcessing, resolver, validationFlags, validationType, schemaSet, onErrorHandler);
	}

	[NotNull]
	public static XmlReaderSettings CreateSettings(bool ignoreWhitespace, bool ignoreComments, bool checkCharacters, bool ignoreProcessingInstructions,
		ConformanceLevel level, DtdProcessing dtdProcessing, XmlUrlResolver resolver, XmlSchemaValidationFlags validationFlags, ValidationType validationType,
		XmlSchemaSet schemaSet, ValidationEventHandler onErrorHandler)
	{
		XmlReaderSettings settings = new XmlReaderSettings
		{
			CloseInput = true,
			IgnoreComments = ignoreComments,
			IgnoreWhitespace = ignoreWhitespace,
			CheckCharacters = checkCharacters,
			IgnoreProcessingInstructions = ignoreProcessingInstructions,
			DtdProcessing = dtdProcessing,
			ConformanceLevel = level,
			ValidationFlags = validationFlags,
			ValidationType = validationType,
			Schemas = schemaSet,
			XmlResolver = resolver ?? XmlUrlResolverHelper.CreateResolver()
		};

		if (onErrorHandler != null) settings.ValidationEventHandler += onErrorHandler;
		return settings;
	}

	[NotNull]
	public static XmlParserContext CreateParserContext([NotNull] XmlNameTable nt) { return CreateParserContext(true, nt, null, null); }

	[NotNull]
	public static XmlParserContext CreateParserContext([NotNull] XmlNameTable nt, bool ignoreWhitespace) { return CreateParserContext(ignoreWhitespace, nt, null, null); }

	[NotNull]
	public static XmlParserContext CreateParserContext([NotNull] XmlNameTable nt, bool ignoreWhitespace, Encoding encoding) { return CreateParserContext(ignoreWhitespace, nt, null, encoding); }

	[NotNull]
	public static XmlParserContext CreateParserContext(bool ignoreWhitespace, [NotNull] XmlNameTable nt, Encoding encoding)
	{
		return CreateParserContext(ignoreWhitespace, nt, null, encoding);
	}

	[NotNull]
	public static XmlParserContext CreateParserContext(bool ignoreWhitespace, [NotNull] XmlNameTable nt, XmlNamespaceManager manager, Encoding encoding)
	{
		return nt.CreateParserContext(ignoreWhitespace, manager, encoding ?? Encoding.UTF8);
	}
}
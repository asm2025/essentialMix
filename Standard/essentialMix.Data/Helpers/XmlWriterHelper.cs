using System;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Data.Helpers
{
	public static class XmlWriterHelper
	{
		[NotNull]
		public static XmlWriterSettings CreateSettings(bool? indent = null, bool? checkCharacters = null, bool? omitDeclaration = null, ConformanceLevel? level = null, Encoding encoding = null)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			SetDefaults(settings);
			if (indent.HasValue) settings.Indent = indent == true;
			if (checkCharacters.HasValue) settings.CheckCharacters = checkCharacters == true;
			if (level.HasValue) settings.ConformanceLevel = level.Value;
			if (omitDeclaration.HasValue) settings.OmitXmlDeclaration = omitDeclaration == true;
			if (encoding != null && !ReferenceEquals(encoding, EncodingHelper.Default)) settings.Encoding = encoding;
			return settings;
		}

		[NotNull]
		public static XmlWriterSettings SetDefaults([NotNull] XmlWriterSettings value)
		{
			value.Async = true;
			value.OmitXmlDeclaration = true;
			value.CheckCharacters = false;
			value.CloseOutput = false;
			value.NewLineOnAttributes = false;
			value.Indent = false;
			value.IndentChars = "\t";
			value.NewLineChars = Environment.NewLine;
			value.NamespaceHandling = NamespaceHandling.OmitDuplicates;
			value.NewLineHandling = NewLineHandling.Replace;
			value.ConformanceLevel = ConformanceLevel.Auto;
			value.Encoding = EncodingHelper.Default;
			return value;
		}
	}
}
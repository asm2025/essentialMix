using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using asm.Data.Extensions;
using asm.Data.Xml;
using asm.Extensions;

namespace asm.Data.Helpers
{
	public static class XmlHelper
	{
		private static readonly XmlSerializerFactory __factory = new XmlSerializerFactory();
		private static readonly ConcurrentDictionary<Type, XmlSerializer> __serializersCache = new ConcurrentDictionary<Type, XmlSerializer>();
		
		[NotNull]
		public static XmlSerializer CreateSerializer<T>(XmlSerializerSettings settings = null) { return CreateSerializer(typeof(T), settings); }
		
		[NotNull]
		public static XmlSerializer CreateSerializer([NotNull] Type type, XmlSerializerSettings settings = null)
		{
			XmlSerializer serializer = __serializersCache.GetOrAdd(type, e => settings == null
																				? __factory.CreateSerializer(e)
																				: __factory.CreateSerializer(e, settings.Overrides, settings.ExtraTypes, settings.Root, settings.DefaultNamespace, settings.Location));
			return serializer ?? throw new SerializationException($"Could not create a serializer for the type {type.FullName}.");
		}

		[NotNull]
		public static XmlSerializer CreateSerializer([NotNull] XmlTypeMapping mapping)
		{
			Type type = Type.GetType(mapping.TypeFullName) ?? throw new TypeLoadException();
			return __serializersCache.GetOrAdd(type, _ => __factory.CreateSerializer(mapping));
		}

		public static string Serialize<T>([NotNull] T value, XmlSerializerSettings settings = null, XmlWriterSettings writerSettings = null)
		{
			if (value.IsNull()) return null;
			XmlSerializer serializer = CreateSerializer<T>(settings);
			return Serialize(value, serializer, writerSettings ?? XmlWriterHelper.CreateSettings());
		}

		public static string Serialize<T>([NotNull] T value, [NotNull] XmlTypeMapping mapping, XmlWriterSettings writerSettings = null)
		{
			if (value.IsNull()) return null;
			XmlSerializer serializer = CreateSerializer(mapping);
			return Serialize(value, serializer, writerSettings ?? XmlWriterHelper.CreateSettings());
		}

		public static string Serialize<T>([NotNull] T value, [NotNull] XmlSerializer serializer, XmlWriterSettings writerSettings = null)
		{
			return value.IsNull()
						? null
						: Serialize((object)value, serializer, writerSettings ?? XmlWriterHelper.CreateSettings());
		}

		public static string Serialize(object value, XmlSerializerSettings settings = null, XmlWriterSettings writerSettings = null)
		{
			if (value.IsNull()) return null;
			XmlSerializer serializer = CreateSerializer(value.GetType(), settings);
			return Serialize(value, serializer, writerSettings ?? XmlWriterHelper.CreateSettings());
		}

		public static string Serialize(object value, [NotNull] XmlTypeMapping mapping, XmlWriterSettings writerSettings = null)
		{
			if (value.IsNull()) return null;
			XmlSerializer serializer = CreateSerializer(mapping);
			return Serialize(value, serializer, writerSettings ?? XmlWriterHelper.CreateSettings());
		}

		[NotNull]
		public static string Serialize([NotNull] object value, [NotNull] XmlSerializer serializer, [NotNull] XmlWriterSettings writerSettings)
		{
			StringBuilder sb = new StringBuilder();

			using (XmlWriter writer = XmlWriter.Create(sb, writerSettings))
			{
				if (value is string s)
				{
					using (TextReader reader = new StringReader(s))
					{
						serializer.Serialize(writer, serializer.Deserialize(reader));
					}
				}
				else
				{
					serializer.Serialize(writer, value);
				}
			}

			return sb.ToString();
		}

		public static T Deserialize<T>(string value, XmlSerializerSettings settings = null, T defaultValue = default(T))
		{
			if (string.IsNullOrWhiteSpace(value)) return defaultValue;
			XmlSerializer serializer = CreateSerializer(typeof(T), settings);
			return (T)Deserialize(value, serializer, (object)defaultValue);
		}

		public static T Deserialize<T>(string value, [NotNull] XmlTypeMapping mapping, T defaultValue = default(T))
		{
			if (string.IsNullOrWhiteSpace(value)) return defaultValue;
			XmlSerializer serializer = CreateSerializer(mapping);
			return (T)Deserialize(value, serializer, (object)defaultValue);
		}

		public static T Deserialize<T>(string value, [NotNull] XmlSerializer serializer, T defaultValue = default(T))
		{
			return string.IsNullOrWhiteSpace(value)
						? defaultValue
						: (T)Deserialize(value, serializer, (object)defaultValue);
		}

		public static object Deserialize(string value, [NotNull] Type type, XmlSerializerSettings settings = null, object defaultValue = null)
		{
			if (string.IsNullOrWhiteSpace(value)) return defaultValue;
			XmlSerializer serializer = CreateSerializer(type, settings);
			return Deserialize(value, serializer, defaultValue);
		}

		public static object Deserialize(string value, [NotNull] XmlTypeMapping mapping, object defaultValue = null)
		{
			if (string.IsNullOrWhiteSpace(value)) return defaultValue;
			XmlSerializer serializer = CreateSerializer(mapping);
			return Deserialize(value, serializer, defaultValue);
		}

		public static object Deserialize(string value, [NotNull] XmlSerializer serializer, object defaultValue = null)
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return defaultValue;

			try
			{
				using (TextReader reader = new StringReader(value))
				{
					return serializer.Deserialize(reader);
				}
			}
			catch
			{
				return defaultValue;
			}
		}

		public static XDocument XParse(string value) { return XParse(value, true); }

		public static XDocument XParse(string value, bool ignoreWhitespace)
		{
			if (string.IsNullOrEmpty(value)) return null;

			XDocument document;

			try
			{
				document = XDocument.Parse(value, ignoreWhitespace ? LoadOptions.None : LoadOptions.PreserveWhitespace);
			}
			catch
			{
				try
				{
					XElement element = ParseXElement(value, ignoreWhitespace);

					if (element != null)
					{
						document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
						document.Add(element);
					}
					else document = null;
				}
				catch
				{
					document = null;
				}
			}

			return document;
		}

		public static XElement ParseXElement(string value) { return ParseXElement(value, true); }

		public static XElement ParseXElement(string value, bool ignoreWhitespace)
		{
			if (string.IsNullOrEmpty(value)) return null;

			XElement element;

			try
			{
				element = XElement.Parse(value, ignoreWhitespace ? LoadOptions.None : LoadOptions.PreserveWhitespace);
			}
			catch
			{
				element = null;
			}

			return element;
		}

		public static XmlDocument ParseXml(string value, params string[] namespaceUri) { return ParseXml(value, false, namespaceUri); }

		public static XmlDocument ParseXml(string value, bool ignoreWhitespace, params string[] namespaceUri) { return ParseXml(value, ignoreWhitespace, false, namespaceUri); }

		public static XmlDocument ParseXml(string value, bool ignoreWhitespace, bool ignoreComments, params string[] namespacesUri)
		{
			if (string.IsNullOrEmpty(value)) return null;

			XmlReaderSettings settings = XmlReaderHelper.CreateSettings(ignoreWhitespace, ignoreComments, Constants.XML_VALIDATE_EVERYTHING_FLAGS, ValidationType.Schema);
			XmlDocument document = XmlDocumentHelper.Create(ignoreWhitespace, namespacesUri);
			XmlReader reader = null;

			try
			{
				using (TextReader textReader = new StringReader(value))
				{
					using (reader = XmlReader.Create(textReader, settings, document.NameTable.CreateParserContext(ignoreWhitespace)))
					{
						bool addedElements = false;

						while (reader.Read())
						{
							switch (reader.NodeType)
							{
								case XmlNodeType.DocumentType:
									if (addedElements) continue;
									string docTypeSystem = reader.GetAttribute("system");
									string docPublicId = reader.GetAttribute("public");
									XmlDocumentType docType = document.CreateDocumentType(reader.Name, docPublicId, docTypeSystem, reader.Value);
									document.AppendChild(docType);
									break;
								case XmlNodeType.XmlDeclaration:
									if (addedElements) continue;
									string docVersion = reader.GetAttribute("version") ?? "1.0";
									string docEncoding = reader.GetAttribute("encoding");
									string docStandAlone = reader.GetAttribute("standalone");
									XmlDeclaration declaration = document.CreateXmlDeclaration(docVersion, docEncoding, docStandAlone);
									document.AppendChild(declaration);
									break;
								case XmlNodeType.Element:
									XmlElement child = document.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
									reader.CollectAttributes(child);
									document.AppendChild(child);
									reader.CollectChildren(child, ignoreComments);
									addedElements = true;
									break;
								case XmlNodeType.Attribute:
									XmlAttribute attribute = document.CreateAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
									if (reader.HasValue) attribute.Value = reader.Value;
									document.AppendChild(attribute);
									break;
								case XmlNodeType.Text:
									document.AppendChild(document.CreateTextNode(reader.Value));
									break;
								case XmlNodeType.CDATA:
									document.AppendChild(document.CreateCDataSection(reader.Value));
									break;
								case XmlNodeType.ProcessingInstruction:
									document.AppendChild(document.CreateProcessingInstruction(reader.Name, reader.Value));
									break;
								case XmlNodeType.Comment:
									if (!ignoreComments) document.AppendChild(document.CreateComment(reader.Value));
									break;
								case XmlNodeType.EntityReference:
									document.AppendChild(document.CreateEntityReference(reader.Name));
									break;
							}
						}
					}
				}
			}
			catch
			{
				document = null;
			}
			finally
			{
				if (reader != null && reader.ReadState != ReadState.Closed) reader.Close();
			}

			return document;
		}

		public static XmlElement ParseXmlElement(string value, params string[] namespaceUri) { return ParseXmlElement(value, false, false, namespaceUri); }

		public static XmlElement ParseXmlElement(string value, bool ignoreWhitespace, params string[] namespaceUri)
		{
			return ParseXmlElement(value, ignoreWhitespace, false, namespaceUri);
		}

		public static XmlElement ParseXmlElement(string value, bool ignoreWhitespace, bool ignoreComments, params string[] namespaceUri) 
		{
			if (string.IsNullOrEmpty(value)) return null;
			StringBuilder sb = new StringBuilder();
			if (!value.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)) sb.AppendLine(XmlDocumentHelper.CreateDeclarationString());
			sb.Append(value);
			XmlDocument document = ParseXml(sb.ToString(), ignoreWhitespace, ignoreComments, namespaceUri);
			return document?.DocumentElement;
		}
	}
}
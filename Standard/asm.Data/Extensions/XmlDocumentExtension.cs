using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Data.Helpers;
using asm.Exceptions.Collections;
using asm.Helpers;

namespace asm.Data.Extensions
{
	public static class XmlDocumentExtension
	{
		[NotNull]
		public static XmlDeclaration CreateDeclaration([NotNull] this XmlDocument thisValue) { return CreateDeclaration(thisValue, EncodingHelper.Default, true, 1.0f); }

		[NotNull]
		public static XmlDeclaration CreateDeclaration([NotNull] this XmlDocument thisValue, Encoding encoding) { return CreateDeclaration(thisValue, encoding, true, 1.0f); }

		[NotNull]
		public static XmlDeclaration CreateDeclaration([NotNull] this XmlDocument thisValue, Encoding encoding, bool standAlone)
		{
			return CreateDeclaration(thisValue, encoding, standAlone, 1.0f);
		}

		[NotNull]
		public static XmlDeclaration CreateDeclaration([NotNull] this XmlDocument thisValue, Encoding encoding, bool standAlone, float version)
		{
			if (Math.Abs(version - 1.0f) > 0.001f) throw new ArgumentException("Currently the version is preserved and must be set to 1.0", nameof(version));
			return thisValue.CreateXmlDeclaration(version.ToString("F1"), (encoding ?? EncodingHelper.Default).WebName, standAlone ? "yes" : "no");
		}

		public static XmlDeclaration AppendDeclaration([NotNull] this XmlDocument thisValue, [NotNull] XDeclaration declaration)
		{
			if (declaration == null) throw new ArgumentNullException(nameof(declaration));
			return AppendDeclaration(thisValue, string.IsNullOrEmpty(declaration.Encoding) ? EncodingHelper.Default : Encoding.GetEncoding(declaration.Encoding), declaration.Standalone.IsSame("yes"), string.IsNullOrEmpty(declaration.Version) ? 1.0f : float.Parse(declaration.Version));
		}

		public static XmlDeclaration AppendDeclaration([NotNull] this XmlDocument thisValue) { return AppendDeclaration(thisValue, EncodingHelper.Default, true, 1.0f); }

		public static XmlDeclaration AppendDeclaration([NotNull] this XmlDocument thisValue, Encoding encoding) { return AppendDeclaration(thisValue, encoding, true, 1.0f); }

		public static XmlDeclaration AppendDeclaration([NotNull] this XmlDocument thisValue, Encoding encoding, bool standAlone)
		{
			return AppendDeclaration(thisValue, encoding, standAlone, 1.0f);
		}

		[NotNull]
		public static XmlDeclaration AppendDeclaration([NotNull] this XmlDocument thisValue, Encoding encoding, bool standAlone, float version)
		{
			return AppendDeclaration(thisValue, CreateDeclaration(thisValue, encoding, standAlone, version));
		}

		[NotNull]
		public static XmlDeclaration AppendDeclaration([NotNull] this XmlDocument thisValue, [NotNull] XmlDeclaration declaration)
		{
			AssertIsWritable(thisValue);
			if (declaration == null) throw new ArgumentNullException(nameof(declaration));
			
			if (thisValue.DocumentElement != null)
				thisValue.InsertBefore(declaration, thisValue.DocumentElement);
			else
				thisValue.AppendChild(declaration);

			return declaration;
		}

		public static int AppendNamespaces([NotNull] this XmlDocument thisValue, [NotNull] params string[] namespaceURI)
		{
			return namespaceURI.IsNullOrEmpty()
						? 0
						: thisValue.NameTable.Append(namespaceURI);
		}

		[NotNull]
		public static XmlNamespaceManager GetNamespaceManager([NotNull] this XmlDocument thisValue) { return new XmlNamespaceManager(thisValue.NameTable); }

		[NotNull]
		public static XDocument ToXDocument([NotNull] this XmlDocument thisValue)
		{
			XDocument document = new XDocument();
			
			using (XmlWriter xmlWriter = document.CreateWriter())
				thisValue.WriteTo(xmlWriter);

			XmlDeclaration decl = thisValue.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
			if (decl != null) document.AppendDeclaration(decl);
			return document;
		}

		[NotNull]
		public static XmlElement Append([NotNull] this XmlDocument thisValue, [NotNull] string name)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			XmlElement element = thisValue.CreateElement(name);

			if (thisValue.DocumentElement == null)
				thisValue.AppendChild(element);
			else
				thisValue.DocumentElement.AppendChild(element);

			return element;
		}

		[NotNull]
		public static XmlElement Append([NotNull] this XmlDocument thisValue, [NotNull] string localName, string namespaceURI)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

			XmlElement element = string.IsNullOrEmpty(namespaceURI) ? thisValue.CreateElement(localName) : thisValue.CreateElement(localName, namespaceURI);

			if (thisValue.DocumentElement == null)
				thisValue.AppendChild(element);
			else
				thisValue.DocumentElement.AppendChild(element);

			return element;
		}

		[NotNull]
		public static XmlElement Append([NotNull] this XmlDocument thisValue, string prefix, [NotNull] string localName, string namespaceURI)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

			XmlElement element;
			
			if (string.IsNullOrEmpty(namespaceURI))
				element = thisValue.CreateElement(localName);
			else
				element = string.IsNullOrEmpty(prefix) ? thisValue.CreateElement(localName, namespaceURI) : thisValue.CreateElement(prefix, localName, namespaceURI);

			if (thisValue.DocumentElement == null)
				thisValue.AppendChild(element);
			else
				thisValue.DocumentElement.AppendChild(element);

			return element;
		}

		[NotNull]
		public static XmlComment AppendComment([NotNull] this XmlDocument thisValue, [NotNull] string comment)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));

			XmlComment xmlComment = thisValue.CreateComment(comment);
			thisValue.AppendChild(xmlComment);
			return xmlComment;
		}

		public static void LoadFile([NotNull] this XmlDocument thisValue, [NotNull] string filename) { LoadFile(thisValue, filename, null, null); }
		public static void LoadFile([NotNull] this XmlDocument thisValue, [NotNull] string filename, XmlReaderSettings settings) { LoadFile(thisValue, filename, settings, null); }
		public static void LoadFile([NotNull] this XmlDocument thisValue, [NotNull] string filename, XmlReaderSettings settings, XmlParserContext context)
		{
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			if (!File.Exists(filename)) throw new FileNotFoundException("File not found", filename);

			XmlReaderSettings options = settings ?? XmlReaderHelper.CreateSettings();

			using (XmlReader reader = context == null ? XmlReader.Create(filename, options) : XmlReader.Create(filename, options, context))
				thisValue.Load(reader);
		}

		public static void SaveFile([NotNull] this XmlDocument thisValue, string filename) { SaveFile(thisValue, filename, EncodingHelper.Default); }
		public static void SaveFile([NotNull] this XmlDocument thisValue, [NotNull] string filename, Encoding encoding) { SaveFile(thisValue, filename, XmlWriterHelper.CreateSettings(encoding: encoding)); }
		public static void SaveFile([NotNull] this XmlDocument thisValue, [NotNull] string filename, XmlWriterSettings settings)
		{
			XmlWriterSettings options = settings ?? XmlWriterHelper.CreateSettings();

			using (XmlWriter writer = XmlWriter.Create(filename, options))
				thisValue.Save(writer);
		}

		public static bool IsValid(this XmlDocument thisValue) { return thisValue != null; }

		public static bool IsValid([NotNull] this XmlDocument thisValue, bool mustHaveItems) { return IsValid(thisValue) && (!mustHaveItems || thisValue.DocumentElement != null); }

		public static bool IsWritable([NotNull] this XmlDocument thisValue) { return IsValid(thisValue) && !thisValue.IsReadOnly; }

		private static void AssertIsWritable([NotNull] XmlDocument value)
		{
			if (IsWritable(value)) return;
			throw new ReadOnlyException();
		}
	}
}
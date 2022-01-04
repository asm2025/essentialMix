using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;
using essentialMix.Data.Helpers;
using essentialMix.Helpers;
using essentialMix.Exceptions.Collections;
using essentialMix.Patterns.Sorting;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class XDocumentExtension
{
	[NotNull]
	public static XDeclaration CreateDeclaration([NotNull] this XDocument thisValue) { return CreateDeclaration(thisValue, EncodingHelper.Default, true, 1.0f); }

	[NotNull]
	public static XDeclaration CreateDeclaration([NotNull] this XDocument thisValue, Encoding encoding) { return CreateDeclaration(thisValue, encoding, true, 1.0f); }

	[NotNull]
	public static XDeclaration CreateDeclaration([NotNull] this XDocument thisValue, Encoding encoding, bool standAlone)
	{
		return CreateDeclaration(thisValue, encoding, standAlone, 1.0f);
	}

	[NotNull]
	public static XDeclaration CreateDeclaration([NotNull] this XDocument thisValue, Encoding encoding, bool standAlone, float version)
	{
		if (Math.Abs(version - 1.0f) > 0.001f) throw new ArgumentException("Currently the version is preserved and must be set to 1.0", nameof(version));
		return new XDeclaration(version.ToString("F1"), (encoding ?? EncodingHelper.Default).WebName, standAlone ? "yes" : "no");
	}

	[NotNull]
	public static XDeclaration AppendDeclaration([NotNull] this XDocument thisValue, [NotNull] XmlDeclaration declaration)
	{
		if (declaration == null) throw new ArgumentNullException(nameof(declaration));
		return AppendDeclaration(thisValue, string.IsNullOrEmpty(declaration.Encoding) ? EncodingHelper.Default : Encoding.GetEncoding(declaration.Encoding), declaration.Standalone.IsSame("yes"), string.IsNullOrEmpty(declaration.Version) ? 1.0f : float.Parse(declaration.Version));
	}

	[NotNull]
	public static XDeclaration AppendDeclaration([NotNull] this XDocument thisValue) { return AppendDeclaration(thisValue, EncodingHelper.Default, true, 1.0f); }

	[NotNull]
	public static XDeclaration AppendDeclaration([NotNull] this XDocument thisValue, Encoding encoding) { return AppendDeclaration(thisValue, encoding, true, 1.0f); }

	[NotNull]
	public static XDeclaration AppendDeclaration([NotNull] this XDocument thisValue, Encoding encoding, bool standAlone)
	{
		return AppendDeclaration(thisValue, encoding, standAlone, 1.0f);
	}

	[NotNull]
	public static XDeclaration AppendDeclaration([NotNull] this XDocument thisValue, Encoding encoding, bool standAlone, float version)
	{
		return AppendDeclaration(thisValue, CreateDeclaration(thisValue, encoding, standAlone, version));
	}

	[NotNull]
	public static XDeclaration AppendDeclaration([NotNull] this XDocument thisValue, [NotNull] XDeclaration declaration)
	{
		AssertIsWritable(thisValue);
		thisValue.Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
		return declaration;
	}

	public static int AppendNamespaces([NotNull] this XDocument thisValue, [NotNull] params string[] namespaceURI)
	{
		if (namespaceURI.IsNullOrEmpty()) return 0;
		if (thisValue.Root == null) throw new ArgumentException("A root element to hold the namespace(s)");
		return thisValue.Root.AppendNamespaces(namespaceURI);
	}

	[NotNull]
	public static XmlNamespaceManager GetNamespaceManager([NotNull] this XDocument thisValue, bool appendDocumentNamespaces = true)
	{
		XmlNameTable nameTable;

		try
		{
			using (XmlReader reader = thisValue.CreateReader())
				nameTable = reader.NameTable;
		}
		catch
		{
			nameTable = null;
		}

		XmlNamespaceManager manager = new XmlNamespaceManager(nameTable ?? new NameTable());
		if (!appendDocumentNamespaces || thisValue.Root == null) return manager;

		XPathNavigator navigator = thisValue.Root.CreateNavigator();
		XPathNodeIterator namespaces = navigator.Select("//namespace::*[not(. = ../../namespace::*)]");

		while (namespaces.MoveNext())
		{
			XPathNavigator node = namespaces.Current;
			if (node == null) continue;

			string prefix = node.Name;

			if (prefix == "xmlns")
			{
				manager.AddNamespace(string.Empty, node.Value);
				prefix = XmlHelper.NS_DEFAULT;
			}
			manager.AddNamespace(prefix, node.Value);
		}

		return manager;
	}

	[NotNull]
	public static XmlDocument ToXmlDocument([NotNull] this XDocument thisValue)
	{
		XmlDocument document = new XmlDocument();

		using (XmlReader xmlReader = thisValue.CreateReader())
			document.Load(xmlReader);

		if (thisValue.Declaration != null) document.AppendDeclaration(thisValue.Declaration);
		return document;
	}

	[NotNull]
	public static XElement Append([NotNull] this XDocument thisValue, [NotNull] string name)
	{
		AssertIsWritable(thisValue);
		if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

		XElement element = new XElement(name);

		if (thisValue.Root == null)
			thisValue.Add(element);
		else
			thisValue.Root.Add(element);

		return element;
	}

	[NotNull]
	public static XElement Append([NotNull] this XDocument thisValue, [NotNull] string localName, string namespaceURI)
	{
		AssertIsWritable(thisValue);
		if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

		XElement element;

		if (string.IsNullOrEmpty(namespaceURI))
			element = new XElement(localName);
		else
		{
			XNamespace ns = namespaceURI;
			element = new XElement(ns + localName);
		}
			 

		if (thisValue.Root == null)
			thisValue.Add(element);
		else
			thisValue.Root.Add(element);

		return element;
	}

	[NotNull]
	public static XElement Append([NotNull] this XDocument thisValue, string prefix, [NotNull] string localName, string namespaceURI)
	{
		AssertIsWritable(thisValue);
		if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

		XElement element;

		if (string.IsNullOrEmpty(namespaceURI)) 
			element = new XElement(localName);
		else
		{
			XNamespace ns = namespaceURI;
			element = new XElement(ns + localName);
			if (!string.IsNullOrEmpty(prefix) && ns != null) element.Add(new XAttribute(XNamespace.Xmlns + prefix, ns.NamespaceName));
		}


		if (thisValue.Root == null)
			thisValue.Add(element);
		else
			thisValue.Root.Add(element);

		return element;
	}

	[NotNull]
	public static XComment AppendComment([NotNull] this XDocument thisValue, [NotNull] string comment)
	{
		AssertIsWritable(thisValue);
		if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));

		XComment xComment = new XComment(comment);
		thisValue.Add(xComment);
		return xComment;
	}

	public static void SaveFile([NotNull] this XDocument thisValue, [NotNull] string filename) { SaveFile(thisValue, filename, EncodingHelper.Default); }
	public static void SaveFile([NotNull] this XDocument thisValue, [NotNull] string filename, Encoding encoding) { SaveFile(thisValue, filename, XmlWriterHelper.CreateSettings(encoding: encoding)); }
	public static void SaveFile([NotNull] this XDocument thisValue, [NotNull] string filename, XmlWriterSettings settings)
	{
		XmlWriterSettings options = settings ?? XmlWriterHelper.CreateSettings();

		using (XmlWriter writer = XmlWriter.Create(filename, options))
			thisValue.Save(writer);
	}

	public static void SaveFile([NotNull] this XDocument thisValue, [NotNull] string filename, SaveOptions options) { thisValue.Save(filename, options); }

	public static bool IsValid(this XDocument thisValue, bool mustHaveItems = false) { return thisValue != null && (!mustHaveItems || thisValue.Root != null); }

	public static bool IsWritable([NotNull] this XDocument thisValue) { return IsValid(thisValue); }

	[NotNull]
	public static XDocument Sort([NotNull] this XDocument thisValue, string attribute = null, SortType sortAttributes = SortType.None)
	{
		return thisValue.Root == null
					? thisValue
					: new XDocument(thisValue.Root.Sort(attribute, sortAttributes));
	}

	[NotNull]
	public static XDocument SortNumerically([NotNull] this XDocument thisValue, [NotNull] string attribute, SortType sortAttributes = SortType.None)
	{
		return thisValue.Root == null
					? thisValue
					: new XDocument(thisValue.Root.SortNumerically(attribute, sortAttributes));
	}

	private static void AssertIsWritable([NotNull] XDocument thisValue)
	{
		if (IsWritable(thisValue)) return;
		throw new ReadOnlyException();
	}
}
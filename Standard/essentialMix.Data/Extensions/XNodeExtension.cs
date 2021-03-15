using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Web;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XNodeExtension
	{
		private static readonly Lazy<XmlDocument> __document = new Lazy<XmlDocument>(() => new XmlDocument(), LazyThreadSafetyMode.PublicationOnly);

		public static int GetIndex(this XNode thisValue, XmlIndexMatchType matchType)
		{
			if (thisValue?.Parent == null) return 0;

			int index = 0;

			if (matchType == XmlIndexMatchType.None)
			{
				for (XNode n = thisValue; n != null; n = n.PreviousNode) ++index;

				return index;
			}

			bool matchT = matchType.HasFlag(XmlIndexMatchType.Type);
			string thisName = thisValue.NodeType == XmlNodeType.Element && matchType.HasFlag(XmlIndexMatchType.Name) ? ((XElement)thisValue).Name.LocalName : null;

			if (thisName != null)
			{
				for (XNode n = thisValue; n != null; n = n.PreviousNode)
				{
					if (matchT && n.NodeType != thisValue.NodeType) continue;
					if (!(n is XElement element) || !element.Name.LocalName.IsSame(thisName)) continue;
					++index;
				}
			}
			else
			{
				for (XNode n = thisValue; n != null; n = n.PreviousNode)
				{
					if (matchT && n.NodeType != thisValue.NodeType) continue;
					++index;
				}
			}

			return index;
		}

		public static XmlNode ToXmlNode([NotNull] this XNode thisValue) { return ToXmlNode(thisValue, false); }

		public static XmlNode ToXmlNode([NotNull] this XNode thisValue, bool preserveWhitespace)
		{
			XmlNode node;

			using (XmlReader reader = thisValue.CreateReader())
				node = LoadNode(reader, preserveWhitespace);

			return node;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsElement(this XNode thisValue, string name)
		{
			return thisValue != null && thisValue.NodeType == XmlNodeType.Element && ((XElement)thisValue).Name.LocalName.IsSame(name);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsElement(this XNode thisValue, string localName, string namespaceURI)
		{
			return thisValue is XElement element && element.Name.LocalName.IsSame(localName) && element.Name.NamespaceName.IsSame(namespaceURI);
		}

		private static XmlNode LoadNode(XmlReader reader, bool preserveWhitespace)
		{
			if (reader == null) return null;

			XmlNode node = null;

			while (reader.Read())
			{
				XmlNode newChild;

				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						bool isEmptyElement = reader.IsEmptyElement;
						XmlElement element = __document.Value.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
						element.IsEmpty = isEmptyElement;

						if (reader.MoveToFirstAttribute())
						{
							XmlAttributeCollection attributes = element.Attributes;

							do
							{
								XmlAttribute attribute = LoadAttributeNode(reader);
								attributes.Append(attribute);
							}
							while (reader.MoveToNextAttribute());

							reader.MoveToElement();
						}

						if (!isEmptyElement)
						{
							node?.AppendChild(element);
							node = element;
							continue;
						}

						newChild = element;
						break;
					case XmlNodeType.Attribute:
						newChild = LoadAttributeNode(reader);
						break;
					case XmlNodeType.Text:
						newChild = __document.Value.CreateTextNode(reader.Value);
						break;
					case XmlNodeType.CDATA:
						newChild = __document.Value.CreateCDataSection(reader.Value);
						break;
					case XmlNodeType.EntityReference:
						newChild = LoadEntityReferenceNode(reader, preserveWhitespace);
						break;
					case XmlNodeType.ProcessingInstruction:
						newChild = __document.Value.CreateProcessingInstruction(reader.Name, reader.Value);
						break;
					case XmlNodeType.Comment:
						newChild = __document.Value.CreateComment(reader.Value);
						break;
					case XmlNodeType.Whitespace:
						if (preserveWhitespace)
						{
							newChild = __document.Value.CreateWhitespace(reader.Value);
							break;
						}

						continue;
					case XmlNodeType.SignificantWhitespace:
						newChild = __document.Value.CreateSignificantWhitespace(reader.Value);
						break;
					case XmlNodeType.EndElement:
						if (node == null) return null;
						if (node.ParentNode == null) return node;
						node = node.ParentNode;
						continue;
					case XmlNodeType.EndEntity:
						return null;
					case XmlNodeType.XmlDeclaration:
						newChild = LoadDeclarationNode(reader);
						break;
					default:
						throw new InvalidOperationException(string.Concat("Unexpected node type", reader.NodeType));
				}

				if (node == null) return newChild;
				node.AppendChild(newChild);
			}

			if (node != null)
			{
				while (node.ParentNode != null)
					node = node.ParentNode;
			}

			return node;
		}

		private static XmlAttribute LoadAttributeNode(XmlReader reader)
		{
			if (reader == null) return null;

			XmlAttribute attribute = __document.Value.CreateAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);

			while (reader.ReadAttributeValue())
			{
				XmlNode xmlNode;

				switch (reader.NodeType)
				{
					case XmlNodeType.Text:
						xmlNode = __document.Value.CreateTextNode(reader.Value);
						break;
					case XmlNodeType.EntityReference:
						xmlNode = __document.Value.CreateEntityReference(reader.LocalName);

						if (reader.CanResolveEntity)
						{
							reader.ResolveEntity();
							LoadAttributeValue(reader, xmlNode);

							if (xmlNode.FirstChild == null)
								xmlNode.AppendChild(__document.Value.CreateTextNode(string.Empty));
						}

						break;
					default:
						throw new InvalidOperationException(string.Concat("Unexpected node type", reader.NodeType));
				}

				attribute.AppendChild(xmlNode);
			}

			return attribute;
		}

		private static void LoadAttributeValue(XmlReader reader, XmlNode parent)
		{
			if (reader == null) return;

			while (reader.ReadAttributeValue())
			{
				XmlNode xmlNode;

				switch (reader.NodeType)
				{
					case XmlNodeType.Text:
						xmlNode = __document.Value.CreateTextNode(reader.Value);
						break;
					case XmlNodeType.EntityReference:
						xmlNode = __document.Value.CreateEntityReference(reader.LocalName);

						if (reader.CanResolveEntity)
						{
							reader.ResolveEntity();
							LoadAttributeValue(reader, xmlNode);
							if (xmlNode.FirstChild == null) xmlNode.AppendChild(__document.Value.CreateTextNode(string.Empty));
						}

						break;
					case XmlNodeType.EndEntity:
						return;
					default:
						throw new InvalidOperationException(string.Concat("Unexpected node type", reader.NodeType));
				}

				parent.AppendChild(xmlNode);
			}
		}

		private static XmlEntityReference LoadEntityReferenceNode(XmlReader reader, bool preserveWhitespace)
		{
			if (reader == null) return null;

			XmlEntityReference xmlEntityReference = __document.Value.CreateEntityReference(reader.Name);

			if (reader.CanResolveEntity)
			{
				reader.ResolveEntity();

				while (reader.Read() && reader.NodeType != XmlNodeType.EndEntity)
				{
					XmlNode newChild = LoadNode(reader, preserveWhitespace);
					if (newChild != null)
						xmlEntityReference.AppendChild(newChild);
				}

				if (xmlEntityReference.LastChild == null) xmlEntityReference.AppendChild(__document.Value.CreateTextNode(string.Empty));
			}

			return xmlEntityReference;
		}

		private static XmlDeclaration LoadDeclarationNode(XmlReader reader)
		{
			if (reader == null) return null;

			string version = null;
			string encoding = null;
			string standalone = null;

			while (reader.MoveToNextAttribute())
			{
				switch (reader.Name)
				{
					case "version":
						version = reader.Value;
						continue;
					case "encoding":
						encoding = reader.Value;
						continue;
					case "standalone":
						standalone = reader.Value;
						continue;
					default:
						continue;
				}
			}

			if (version == null) ParseXmlDeclarationValue(reader.Value, out version, out encoding, out standalone);
			return __document.Value.CreateXmlDeclaration(version, encoding, standalone);
		}

		private static void ParseXmlDeclarationValue(string strValue, out string version, out string encoding, out string standalone)
		{
			version = null;
			encoding = null;
			standalone = null;
			if (string.IsNullOrEmpty(strValue)) return;
			
			Match match = Regex.Match(strValue, "version=\"(\\d+(\\.\\d+)?)\"", RegexHelper.OPTIONS_I);
			if (match.Success && match.Groups.Count > 0) version = match.Groups[0].Value;
			
			match = Regex.Match(strValue, "encoding=\"([^\"]+)\"", RegexHelper.OPTIONS_I);
			if (match.Success && match.Groups.Count > 0) encoding = match.Groups[0].Value;
			
			match = Regex.Match(strValue, "standalone=\"([^\"]+)\"", RegexHelper.OPTIONS_I);
			if (match.Success && match.Groups.Count > 0) standalone = match.Groups[0].Value;
		}
	}
}
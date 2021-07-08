using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using essentialMix.Data.Xml;
using essentialMix.Web;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XmlNodeExtension
	{
		public static int GetIndex(this XmlNode thisValue, XmlIndexMatchType matchType)
		{
			if (thisValue?.ParentNode == null) return -1;

			int index = 0;

			if (matchType == XmlIndexMatchType.None)
			{
				for (XmlNode n = thisValue; n != null; n = n.PreviousSibling)
					++index;

				return index;
			}

			bool matchT = matchType.FastHasFlag(XmlIndexMatchType.Type);
			bool matchN = thisValue.NodeType == XmlNodeType.Element && matchType.FastHasFlag(XmlIndexMatchType.Name);

			if (matchN)
			{
				for (XmlNode n = thisValue; n != null; n = n.PreviousSibling)
				{
					if (matchT && n.NodeType != thisValue.NodeType) continue;
					if (!n.Name.IsSame(thisValue.Name)) continue;
					++index;
				}
			}
			else
			{
				for (XmlNode n = thisValue; n != null; n = n.PreviousSibling)
				{
					if (matchT && n.NodeType != thisValue.NodeType) continue;
					++index;
				}
			}

			return index;
		}

		public static XNode ToXNode(this XmlNode thisValue, XmlWriterSettings settings)
		{
			if (thisValue == null) return null;

			XNode node;
			
			using (XNodeBuilder builder = new XNodeBuilder())
			{
				using (XmlWriter writer = XmlWriter.Create(builder, settings))
					thisValue.WriteTo(writer);

				node = builder.Root;
			}

			return node;
		}

		public static string GetXPath([NotNull] this XmlNode thisValue)
		{
			StringBuilder sb = new StringBuilder();
			XmlNode node = thisValue;
			
			while (node != null)
			{
				int index;
			
				switch (node.NodeType)
				{
					case XmlNodeType.Attribute:
						// attributes have an OwnerElement, not a ParentNode; also they have
						// to be matched by name, not found by position
						XmlAttribute attribute = (XmlAttribute)node;
						if (attribute.OwnerElement == null) return null;
						sb.Insert(0, string.Concat("/@", node.Name));
						node = attribute.OwnerElement;
						break;
					case XmlNodeType.Element:
						// the only node with no parent is the root node
						index = node.GetIndex(XmlIndexMatchType.Type | XmlIndexMatchType.Name);
						sb.Insert(0, index < 1 ? string.Concat("/", node.Name) : $"/{node.Name}[{index}]");
						node = node.ParentNode;
						break;
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.CDATA:
						index = node.GetIndex(XmlIndexMatchType.None);
						sb.Insert(0, index < 1 ? "/text()" : $"/node()[{index}]");
						node = node.ParentNode;
						break;
					case XmlNodeType.Comment:
						index = node.GetIndex(XmlIndexMatchType.Type);
						sb.Insert(0, index < 1 ? "/comment()" : $"/comment()[{index}]");
						node = node.ParentNode;
						break;
					case XmlNodeType.EndElement:
						node = node.ParentNode;
						break;
					case XmlNodeType.Document:
						sb.Insert(0, '/');
						node = null;
						break;
					default:
						node = null;
						break;
				}
			}

			return sb.Length == 0 ? null : sb.ToString();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsElement(this XmlNode thisValue, string name) { return thisValue is { NodeType: XmlNodeType.Element } && thisValue.Name.IsSame(name); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsElement(this XmlNode thisValue, string localName, string namespaceURI)
		{
			return thisValue is { NodeType: XmlNodeType.Element } && thisValue.Name.IsSame(localName) && thisValue.NamespaceURI.IsSame(namespaceURI);
		}

		public static T Select<T>([NotNull] this XmlNode thisValue, [NotNull] string path)
			where T : XmlNode
		{
			return Select<T>(thisValue, path, null);
		}

		public static T Select<T>([NotNull] this XmlNode thisValue, [NotNull] string path, XmlNamespaceManager manager)
			where T : XmlNode
		{
			return manager == null
				? (T)thisValue.SelectSingleNode(path)
				: (T)thisValue.SelectSingleNode(path, manager);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasAttribute([NotNull] this XmlNode thisValue, [NotNull] string name)
		{
			return thisValue.Attributes.Has(name);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasAttribute([NotNull] this XmlNode thisValue, [NotNull] string localName, string namespaceURI)
		{
			return thisValue.Attributes.Has(localName, namespaceURI);
		}

		public static bool HasChild([NotNull] this XmlNode thisValue, [NotNull] string name)
		{
			if (!thisValue.HasChildNodes) return false;

			foreach (XmlNode node in thisValue.ChildNodes)
			{
				if (node.Name != name) continue;
				return true;
			}

			return false;
		}

		public static bool HasChild([NotNull] this XmlNode thisValue, [NotNull] string localName, string namespaceURI)
		{
			if (!thisValue.HasChildNodes) return false;
			
			foreach (XmlNode node in thisValue.ChildNodes)
			{
				if (node.LocalName != localName || node.NamespaceURI != namespaceURI) continue;
				return true;
			}

			return false;
		}

		public static bool HasAttributeOrChild([NotNull] this XmlNode thisValue, [NotNull] string name)
		{
			return HasAttribute(thisValue, name) || HasChild(thisValue, name);
		}

		public static bool HasAttributeOrChild([NotNull] this XmlNode thisValue, [NotNull] string localName, string namespaceURI)
		{
			return HasAttribute(thisValue, localName, namespaceURI) || HasChild(thisValue, localName, namespaceURI);
		}
	}
}
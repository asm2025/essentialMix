using System.Text;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using essentialMix.Web;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XObjectExtension
	{
		public static string GetXPath([NotNull] this XObject thisValue)
		{
			StringBuilder sb = new StringBuilder();
			XObject node = thisValue;
			
			while (node != null)
			{
				int index;
			
				switch (node.NodeType)
				{
					case XmlNodeType.Attribute:
						// attributes have an OwnerElement, not a ParentNode; also they have
						// to be matched by name, not found by position
						XAttribute attribute = (XAttribute)node;
						if (attribute.Parent == null) return null;
						sb.Insert(0, string.Concat("/@", attribute.Name));
						node = attribute.Parent;
						break;
					case XmlNodeType.Element:
						// the only node with no parent is the root node
						XElement element = (XElement)node;
						index = element.GetIndex(XmlIndexMatchType.Type | XmlIndexMatchType.Name);
						sb.Insert(0, index < 1 ? string.Concat("/", element.Name) : $"/{element.Name}[{index}]");
						node = node.Parent;
						break;
					//comment, text node
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.CDATA:
						index = ((XNode)node).GetIndex(XmlIndexMatchType.None);
						sb.Insert(0, index < 1 ? "/text()" : $"/node()[{index}]");
						node = node.Parent;
						break;
					case XmlNodeType.Comment:
						index = ((XNode)node).GetIndex(XmlIndexMatchType.Type);
						sb.Insert(0, index < 1 ? "/comment()" : $"/comment()[{index}]");
						node = node.Parent;
						break;
					case XmlNodeType.EndElement:
						node = node.Parent;
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
	}
}
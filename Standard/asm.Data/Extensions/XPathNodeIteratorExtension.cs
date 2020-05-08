using System.Xml.XPath;
using JetBrains.Annotations;
using asm.Data.Xml;

namespace asm.Data.Extensions
{
	public static class XPathNodeIteratorExtension
	{
		[NotNull] public static XmlNodeEnumerator XmlEnumerate([NotNull] this XPathNodeIterator thisValue) { return new XmlNodeEnumerator(thisValue); }

		[NotNull] public static XmlNodeLister XmlList([NotNull] this XPathNodeIterator thisValue) { return new XmlNodeLister(thisValue); }

		[NotNull] public static XNodeEnumerator XEnumerate([NotNull] this XPathNodeIterator thisValue) { return new XNodeEnumerator(thisValue); }

		[NotNull] public static XNodeLister XList([NotNull] this XPathNodeIterator thisValue) { return new XNodeLister(thisValue); }
	}
}
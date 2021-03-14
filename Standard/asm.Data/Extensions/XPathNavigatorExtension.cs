using System.Xml;
using System.Xml.XPath;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XPathNavigatorExtension
	{
		public static string GetChildNodeValue([NotNull] this XPathNavigator thisValue, string nodePath, IXmlNamespaceResolver resolver = null)
		{
			if (string.IsNullOrEmpty(nodePath)) return null;
			XPathNavigator navigator = resolver == null ? thisValue.SelectSingleNode(nodePath) : thisValue.SelectSingleNode(nodePath, resolver);
			return navigator?.Value;
		}
	}
}
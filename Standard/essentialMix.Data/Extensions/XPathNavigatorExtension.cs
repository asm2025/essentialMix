using System.Xml;
using System.Xml.XPath;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XPathNavigatorExtension
	{
		public static string GetChildNodeValue([NotNull] this XPathNavigator thisValue, string nodePath, IXmlNamespaceResolver resolver = null)
		{
			if (string.IsNullOrEmpty(nodePath)) return null;
			XPathNavigator navigator = resolver == null ? thisValue.SelectSingleNode(nodePath) : thisValue.SelectSingleNode(nodePath, resolver);
			return navigator?.Value;
		}

		public static string Get([NotNull] this XPathNavigator thisValue, string name, IXmlNamespaceResolver resolver = null) { return Get(thisValue, name, null, resolver); }
		public static string Get([NotNull] this XPathNavigator thisValue, string name, string defaultValue, IXmlNamespaceResolver resolver = null)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			XPathNavigator attribute = thisValue.SelectSingleNode("@" + name, resolver);
			return attribute?.Value ?? defaultValue;
		}

		public static T Get<T>([NotNull] this XPathNavigator thisValue, string name, IXmlNamespaceResolver resolver = null) { return Get(thisValue, name, default(T), resolver); }
		public static T Get<T>([NotNull] this XPathNavigator thisValue, string name, T defaultValue, IXmlNamespaceResolver resolver = null)
		{
			string value = Get(thisValue, name, null, resolver);
			return string.IsNullOrEmpty(value)
						? defaultValue
						: value.To(defaultValue);
		}

		public static bool TryGet([NotNull] this XPathNavigator thisValue, string name, out string result, IXmlNamespaceResolver resolver = null)
		{
			result = null;
			if (string.IsNullOrEmpty(name)) return false;
			XPathNavigator attribute = thisValue.SelectSingleNode("@" + name, resolver);
			if (attribute == null) return false;
			result = attribute.Value;
			return true;
		}

		public static bool TryGet<T>([NotNull] this XPathNavigator thisValue, string name, out T result, IXmlNamespaceResolver resolver = null)
		{
			if (TryGet(thisValue, name, out string value, resolver)) return value.TryConvert(out result);
			result = default(T);
			return false;

		}
	}
}
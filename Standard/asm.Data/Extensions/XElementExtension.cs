using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Data.Xml;
using asm.Exceptions.Collections;
using asm.Patterns.Sorting;
using asm.Web;

namespace asm.Data.Extensions
{
	public static class XElementExtension
	{
		public static int GetIndex([NotNull] this XElement thisValue) { return GetIndex(thisValue, true); }

		public static int GetIndex([NotNull] this XElement thisValue, bool matchName)
		{
			XmlIndexMatchType flags = XmlIndexMatchType.Type;
			if (matchName) flags |= XmlIndexMatchType.Name;
			return thisValue.GetIndex(flags);
		}

		public static XmlNamespaceManager GetNamespaceManager([NotNull] this XElement thisValue)
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

			return nameTable == null ? null : new XmlNamespaceManager(nameTable);
		}

		public static int AppendNamespaces([NotNull] this XElement thisValue, [NotNull] params string[] namespaceURI)
		{
			if (namespaceURI.IsNullOrEmpty()) return 0;

			int n = 0;

			foreach (string s in namespaceURI)
			{
				if (string.IsNullOrEmpty(s)) continue;

				int p = s.IndexOf('|');

				if (p > 0 && p < s.Length)
				{
					string[] names = s.Split(2, '|');
					int p2 = names[1].IndexOf('|');
					thisValue.Add(new XAttribute(XNamespace.Xmlns + names[0], p2 > -1 ? names[1].Replace("|", string.Empty) : names[1]));
					++n;
				}
				else
				{
					thisValue.Add(new XAttribute("xmlns", p > -1 ? s.Replace("|", string.Empty) : s));
					++n;
				}
			}
			
			return n;
		}

		public static XNode SelectSingleNode([NotNull] this XElement thisValue, [NotNull] string xpath)
		{
			XNodeLister list = SelectNodes(thisValue, xpath);
			if (list == null) return null;
			return list.Count == 0 ? null : list[0];
		}

		public static XNode SelectSingleNode([NotNull] this XElement thisValue, [NotNull] string xpath, XmlNamespaceManager nsmgr)
		{
			XNodeLister list = SelectNodes(thisValue, xpath);
			if (list == null) return null;
			return list.Count == 0 ? null : list[0];
		}

		[NotNull]
		public static XNodeLister SelectNodes([NotNull] this XElement thisValue, [NotNull] string xpath)
		{
			XPathNavigator navigator = thisValue.CreateNavigator();
			return new XNodeLister(navigator.Select(xpath));
		}

		[NotNull]
		public static XNodeLister SelectNodes([NotNull] this XElement thisValue, [NotNull] string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator navigator = thisValue.CreateNavigator();
			XPathExpression expr = navigator.Compile(xpath);
			expr.SetContext(nsmgr);
			return new XNodeLister(navigator.Select(expr));
		}

		public static XNodeLister Elements([NotNull] this XElement thisValue, string name)
		{
			if (!thisValue.HasElements) return null;

			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string expr = name ?? "*";
			return nsmgr == null ? thisValue.SelectNodes(expr) : thisValue.SelectNodes(expr, nsmgr);
		}

		public static XNodeLister Elements([NotNull] this XElement thisValue, string name, string attribute, string value)
		{
			if (!thisValue.HasElements) return null;

			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string nm = name ?? "*";
			string expr = attribute == null
				? nm
				: $"{nm}[@{attribute}='{value ?? string.Empty}']";
			return nsmgr == null ? thisValue.SelectNodes(expr) : thisValue.SelectNodes(expr, nsmgr);
		}

		public static XNode First([NotNull] this XElement thisValue, string name) { return First(thisValue, name, null, null); }

		public static XNode First([NotNull] this XElement thisValue, string name, string attribute, string value)
		{
			if (!thisValue.HasElements) return null;
			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string nm = name ?? "*";
			string expr = attribute == null
				? nm
				: $"{nm}[@{attribute}='{value ?? string.Empty}']";
			return nsmgr == null ? thisValue.SelectSingleNode(expr) : thisValue.SelectSingleNode(expr, nsmgr);
		}

		public static XNode Last([NotNull] this XElement thisValue, string name) { return Last(thisValue, name, null, null); }

		public static XNode Last([NotNull] this XElement thisValue, string name, string attribute, string value)
		{
			if (!thisValue.HasElements) return null;
			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string nm = name ?? "*";
			
			string expr = attribute == null
				? $"{nm}[last()]"
				: $"{nm}[@{attribute}='{value ?? string.Empty}'][last()]";
			return nsmgr == null ? thisValue.SelectSingleNode(expr) : thisValue.SelectSingleNode(expr, nsmgr);
		}

		public static T Evaluate<T>([NotNull] this XElement thisValue, string expression) { return Evaluate(thisValue, expression, default(T)); }

		public static T Evaluate<T>([NotNull] this XElement thisValue, string expression, T defaultValue)
		{
			if (!IsValid(thisValue)) return defaultValue;

			XPathNavigator navigator = thisValue.CreateNavigator();
			T result;

			try { result = navigator.Evaluate(expression ?? ".").To(defaultValue); }
			catch { result = defaultValue; }

			return result;
		}

		public static T Evaluate<T>([NotNull] this XElement thisValue, XPathExpression expression) { return Evaluate(thisValue, expression, default(T)); }

		public static T Evaluate<T>([NotNull] this XElement thisValue, XPathExpression expression, T defaultValue)
		{
			if (!IsValid(thisValue) || expression == null) return defaultValue;
			
			XPathNavigator navigator = thisValue.CreateNavigator();
			T result;

			try { result = navigator.Evaluate(expression).To(defaultValue); }
			catch { result = defaultValue; }
			
			return result;
		}

		public static bool IsElement(this XElement thisValue, string name) { return IsValid(thisValue) && thisValue.Name.LocalName.IsSame(name); }

		public static bool IsElement(this XElement thisValue, string localName, string namespaceURI)
		{
			return IsValid(thisValue) && thisValue.Name.LocalName.IsSame(localName) && thisValue.Name.NamespaceName.IsSame(namespaceURI);
		}

		public static bool IsValid(this XElement thisValue) { return thisValue != null; }

		public static bool IsWritable(this XElement thisValue) { return IsValid(thisValue); }

		public static T Get<T>(this XElement thisValue, [NotNull] string name) { return Get(thisValue, name, default(T)); }

		public static T Get<T>(this XElement thisValue, [NotNull] string name, T defaultValue)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (!IsValid(thisValue) || !thisValue.HasAttributes) return defaultValue;

			XAttribute attribute = thisValue.Attribute(name);
			return attribute == null 
				? defaultValue
				: attribute.Get(defaultValue);
		}

		public static T Get<T>(this XElement thisValue, [NotNull] string localName, string namespaceUri) { return Get(thisValue, localName, namespaceUri, default(T)); }

		public static T Get<T>(this XElement thisValue, [NotNull] string localName, string namespaceUri, T defaultValue)
		{
			if (string.IsNullOrEmpty(namespaceUri)) return Get(thisValue, localName, defaultValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));
			if (!IsValid(thisValue) || !thisValue.HasAttributes) return defaultValue;
			
			XNamespace ns = namespaceUri;
			XAttribute attribute = thisValue.Attribute(ns + localName);
			return attribute == null
				? defaultValue
				: attribute.Get(defaultValue);
		}

		public static void Set<T>([NotNull] this XElement thisValue, [NotNull] string name, T value)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			XAttribute attr = thisValue.Attribute(name);

			if (attr != null)
			{
				attr.Value = Convert.ToString(value);
				return;
			}

			attr = new XAttribute(name, value);
			thisValue.Add(attr);
		}

		public static void Set<T>([NotNull] this XElement thisValue, [NotNull] string localName, string namespaceUri, T value)
		{
			if (string.IsNullOrEmpty(namespaceUri))
			{
				Set(thisValue, localName, value);
				return;
			}

			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

			XNamespace ns = namespaceUri;
			XAttribute attr = thisValue.Attribute(ns + localName);

			if (attr != null)
			{
				attr.Value = Convert.ToString(value);
				return;
			}
			
			attr = new XAttribute(ns + localName, value);
			thisValue.Add(attr);
		}

		public static void Remove([NotNull] this XElement thisValue, [NotNull] string name)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			XAttribute attr = thisValue.Attribute(name);
			attr?.Remove();
		}

		public static void Remove([NotNull] this XElement thisValue, [NotNull] string localName, string namespaceUri)
		{
			if (string.IsNullOrEmpty(namespaceUri))
			{
				Remove(thisValue, localName);
				return;
			}

			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

			XAttribute attr = thisValue.Attribute(XNamespace.Get(namespaceUri) + localName);
			attr?.Remove();
		}

		[NotNull]
		public static XElement Append([NotNull] this XElement thisValue, [NotNull] string name)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			XElement element = new XElement(name);
			thisValue.Add(element);
			return element;
		}

		[NotNull]
		public static XElement Append([NotNull] this XElement thisValue, [NotNull] string localName, [NotNull] string namespaceURI)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

			XElement element = new XElement(XNamespace.Get(namespaceURI) + localName);
			thisValue.Add(element);
			return element;
		}

		[NotNull]
		public static XElement Append([NotNull] this XElement thisValue, string prefix, [NotNull] string localName, string namespaceURI)
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

			thisValue.Add(element);
			return element;
		}

		[NotNull]
		public static XComment AppendComment([NotNull] this XElement thisValue, [NotNull] string comment)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));

			XComment xmlComment = new XComment(comment);
			thisValue.Add(xmlComment);
			return xmlComment;
		}

		[NotNull]
		public static XCData AppendCData([NotNull] this XElement thisValue, [NotNull] string data)
		{
			AssertIsWritable(thisValue);
			if (data == null) throw new ArgumentNullException(nameof(data));
			
			XCData section = new XCData(data);
			thisValue.Add(section);
			return section;
		}

		[NotNull]
		public static XElement StripNamespaces([NotNull] this XElement thisValue)
		{
			XElement result = new XElement(thisValue);

			foreach (XElement e in result.DescendantsAndSelf())
			{
				e.Name = XNamespace.None.GetName(e.Name.LocalName);

				IEnumerable<XAttribute> attributes = e.Attributes()
					.Where(a => !a.IsNamespaceDeclaration)
					.Where(a => a.Name.Namespace != XNamespace.Xml && a.Name.Namespace != XNamespace.Xmlns)
					.Select(a => new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value));
				e.ReplaceAttributes(attributes);
			}

			return result;
		}

		[NotNull]
		public static XElement ElementStrict([NotNull] this XElement thisValue, XName name)
		{
			return thisValue.Element(name) ?? throw new KeyNotFoundException($"Could not find element [{name}]");
		}

		[NotNull]
		public static XAttribute AttributeStrict([NotNull] this XElement thisValue, XName name)
		{
			return thisValue.Attribute(name) ?? throw new KeyNotFoundException($"Could not find attribute [{name}]");
		}

		[NotNull]
		public static XElement Sort([NotNull] this XElement thisValue, string attribute = null, SortType sortAttributes = SortType.None)
		{
			return Sort(thisValue, 0, attribute, sortAttributes);
		}

		[NotNull]
		public static XElement SortNumerically([NotNull] this XElement thisValue, [NotNull] string attribute, SortType sortAttributes = SortType.None)
		{
			return SortNumerically(thisValue, 0, attribute, sortAttributes);
		}

		[NotNull]
		private static XElement Sort([NotNull] XElement element, uint level, string attribute, SortType sortAttributes)
		{
			XElement newElement = new XElement(element.Name,
				element.Elements()
					.OrderBy(child =>
					{
						if (child.Ancestors().Count() <= level) return string.Empty;
						if (!child.HasAttributes || string.IsNullOrEmpty(attribute)) return child.Name;

						XAttribute xAttribute = child.Attribute(attribute);
						return xAttribute?.Value ?? child.Name;
					})
					.Select(child => Sort(child, level, attribute, sortAttributes)));
			if (!element.HasAttributes) return newElement;

			IEnumerable<XAttribute> attributes = element.Attributes();

			switch (sortAttributes)
			{
				case SortType.Ascending:
					attributes = attributes.OrderBy(e => e.Name);
					break;
				case SortType.Descending:
					attributes = attributes.OrderByDescending(e => e.Name);
					break;
			}

			foreach (XAttribute xAttribute in attributes)
				newElement.SetAttributeValue(xAttribute.Name, xAttribute.Value);

			return newElement;
		}

		[NotNull]
		private static XElement SortNumerically([NotNull] XElement element, uint level, string attribute, SortType sortAttributes)
		{
			XElement newElement = new XElement(element.Name,
				element.Elements()
					.OrderBy(child =>
					{
						if (child.Ancestors().Count() <= level) return 0UL;
						if (!child.HasAttributes || string.IsNullOrEmpty(attribute)) return (ulong)(child.Name.GetHashCode() + long.MaxValue);

						XAttribute xAttribute = child.Attribute(attribute);
						if (xAttribute == null) return (ulong)(child.Name.GetHashCode() + long.MaxValue);
						return xAttribute.Value.To(0UL);
					})
					.Select(child => Sort(child, level, attribute, sortAttributes)));
			if (!element.HasAttributes) return newElement;

			IEnumerable<XAttribute> attributes = element.Attributes();

			switch (sortAttributes)
			{
				case SortType.Ascending:
					attributes = attributes.OrderBy(e => e.Name);
					break;
				case SortType.Descending:
					attributes = attributes.OrderByDescending(e => e.Name);
					break;
			}

			foreach (XAttribute xAttribute in attributes)
				newElement.SetAttributeValue(xAttribute.Name, xAttribute.Value);

			return newElement;
		}

		private static void AssertIsWritable(XElement value)
		{
			if (IsWritable(value)) return;
			throw new ReadOnlyException();
		}
	}
}

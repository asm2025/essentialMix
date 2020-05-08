using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Web;

namespace asm.Data.Extensions
{
	public static class XmlElementExtension
	{
		public static int GetIndex([NotNull] this XmlElement thisValue) { return GetIndex(thisValue, true); }

		public static int GetIndex([NotNull] this XmlElement thisValue, bool matchName)
		{
			XmlIndexMatchType flags = XmlIndexMatchType.Type;
			if (matchName) flags |= XmlIndexMatchType.Name;
			return thisValue.GetIndex(flags);
		}

		public static XmlNamespaceManager GetNamespaceManager([NotNull] this XmlElement thisValue)
		{
			return thisValue.OwnerDocument == null
						? null
						: new XmlNamespaceManager(thisValue.OwnerDocument.NameTable);
		}

		public static int AppendNamespaces([NotNull] this XmlElement thisValue, [NotNull] params string[] namespaceURI)
		{
			if (thisValue.OwnerDocument == null || namespaceURI.IsNullOrEmpty()) return 0;

			int n = 0;

			foreach (string s in namespaceURI)
			{
				if (string.IsNullOrEmpty(s)) continue;

				int p = s.IndexOf('|');

				if (p > 0 && p < s.Length)
				{
					string[] names = s.Split(2, '|');
					int p2 = names[1].IndexOf('|');
					thisValue.SetAttribute(string.Concat("xmlns:", names[0]), p2 > -1 ? names[1].Replace("|", string.Empty) : names[1]);
					++n;
				}
				else
				{
					thisValue.SetAttribute("xmlns:", p > -1 ? s.Replace("|", string.Empty) : s);
					++n;
				}
			}

			return n;
		}

		public static IEnumerable<XmlElement> Ancestors([NotNull] this XmlElement thisValue) { return Ancestors(thisValue, null); }

		[ItemNotNull]
		public static IEnumerable<XmlElement> Ancestors([NotNull] this XmlElement thisValue, string name)
		{
			XmlElement e;
			
			if (name == null)
			{
				while ((e = thisValue.ParentNode as XmlElement) != null)
					yield return e;
			}
			else
			{
				while ((e = thisValue.ParentNode as XmlElement) != null)
					if (name == e.Name) yield return e;
			}
		}

		public static XmlNodeList Elements([NotNull] this XmlElement thisValue) { return Elements(thisValue, null); }

		public static XmlNodeList Elements([NotNull] this XmlElement thisValue, string name)
		{
			if (!thisValue.HasChildNodes) return null;
			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string expr = name ?? "*";
			return nsmgr == null ? thisValue.SelectNodes(expr) : thisValue.SelectNodes(expr, nsmgr);
		}

		public static XmlNodeList Elements([NotNull] this XmlElement thisValue, string name, string attribute, string value)
		{
			if (!thisValue.HasChildNodes) return null;

			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string nm = name ?? "*";
			string expr = attribute == null
				? nm
				: $"{nm}[@{attribute}='{value ?? string.Empty}']";
			return nsmgr == null ? thisValue.SelectNodes(expr) : thisValue.SelectNodes(expr, nsmgr);
		}

		public static XmlNode First([NotNull] this XmlElement thisValue, string name) { return First(thisValue, name, null, null); }

		public static XmlNode First([NotNull] this XmlElement thisValue, string name, string attribute, string value)
		{
			if (!thisValue.HasChildNodes) return null;
			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string nm = name ?? "*";
			string expr = attribute == null
				? nm
				: $"{nm}[@{attribute}='{value ?? string.Empty}']";
			return nsmgr == null ? thisValue.SelectSingleNode(expr) : thisValue.SelectSingleNode(expr, nsmgr);
		}

		public static XmlNode Last([NotNull] this XmlElement thisValue, string name) { return Last(thisValue, name, null, null); }

		public static XmlNode Last([NotNull] this XmlElement thisValue, string name, string attribute, string value)
		{
			if (!thisValue.HasChildNodes) return null;
			XmlNamespaceManager nsmgr = thisValue.GetNamespaceManager();
			string nm = name ?? "*";
			
			string expr = attribute == null
				? $"{nm}[last()]"
				: $"{nm}[@{attribute}='{value ?? string.Empty}'][last()]";
			return nsmgr == null ? thisValue.SelectSingleNode(expr) : thisValue.SelectSingleNode(expr, nsmgr);
		}

		public static T Evaluate<T>([NotNull] this XmlElement thisValue, string expression) { return Evaluate(thisValue, expression, default(T)); }

		public static T Evaluate<T>([NotNull] this XmlElement thisValue, string expression, T defaultValue)
		{
			if (!IsValid(thisValue)) return defaultValue;
			XPathNavigator navigator = thisValue.CreateNavigator();
			T result;

			try { result = navigator.Evaluate(expression ?? ".").To(defaultValue); }
			catch { result = defaultValue; }

			return result;
		}

		public static T Evaluate<T>([NotNull] this XmlElement thisValue, XPathExpression expression) { return Evaluate(thisValue, expression, default(T)); }

		public static T Evaluate<T>([NotNull] this XmlElement thisValue, XPathExpression expression, T defaultValue)
		{
			if (!IsValid(thisValue) || expression == null) return defaultValue;
			
			XPathNavigator navigator = thisValue.CreateNavigator();
			T result;

			try { result = navigator.Evaluate(expression).To(defaultValue); }
			catch { result = defaultValue; }
			
			return result;
		}

		public static bool IsElement([NotNull] this XmlElement thisValue, string name) { return IsValid(thisValue) && thisValue.Name.IsSame(name); }

		public static bool IsElement([NotNull] this XmlElement thisValue, string localName, string namespaceURI)
		{
			return IsValid(thisValue) && thisValue.LocalName.IsSame(localName) && thisValue.NamespaceURI.IsSame(namespaceURI);
		}

		public static bool IsValid(this XmlElement thisValue) { return thisValue != null; }

		public static bool IsWritable([NotNull] this XmlElement thisValue) { return IsValid(thisValue) && !thisValue.IsReadOnly; }

		public static T Get<T>([NotNull] this XmlElement thisValue, [NotNull] string name) { return Get(thisValue, name, default(T)); }

		public static T Get<T>([NotNull] this XmlElement thisValue, [NotNull] string name, T defaultValue)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (!IsValid(thisValue) || !thisValue.HasAttributes) return defaultValue;
			return thisValue.Attributes[name].Get(defaultValue);
		}

		public static T Get<T>([NotNull] this XmlElement thisValue, [NotNull] string localName, string namespaceUri) { return Get(thisValue, localName, namespaceUri, default(T)); }

		public static T Get<T>([NotNull] this XmlElement thisValue, [NotNull] string localName, string namespaceUri, T defaultValue)
		{
			if (string.IsNullOrEmpty(namespaceUri)) return Get(thisValue, localName, defaultValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));
			if (!IsValid(thisValue) || !thisValue.HasAttributes) return defaultValue;
			return thisValue.Attributes[localName, namespaceUri].Get(defaultValue);
		}

		public static void Set<T>([NotNull] this XmlElement thisValue, [NotNull] string name, T value)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

			XmlAttribute attr = thisValue.Attributes[name];

			if (attr != null)
			{
				attr.Value = Convert.ToString(value);
				return;
			}

			attr = thisValue.OwnerDocument?.CreateAttribute(name) ?? throw new ArgumentException("The element has no document attached to it");
			attr.Value = Convert.ToString(value);
			thisValue.Attributes.Append(attr);
		}

		public static void Set<T>([NotNull] this XmlElement thisValue, [NotNull] string localName, string namespaceUri, T value) { Set(thisValue, null, localName, namespaceUri, value); }
		public static void Set<T>([NotNull] this XmlElement thisValue, string prefix, [NotNull] string localName, string namespaceUri, T value)
		{
			if (string.IsNullOrEmpty(namespaceUri))
			{
				Set(thisValue, localName, value);
				return;
			}

			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));

			XmlAttribute attr = thisValue.Attributes[localName, namespaceUri];

			if (attr != null)
			{
				attr.Value = Convert.ToString(value);
				return;
			}

			if (thisValue.OwnerDocument == null) throw new ArgumentException("The element has no document attached to it");
			attr = string.IsNullOrEmpty(prefix) ? thisValue.OwnerDocument.CreateAttribute(localName, namespaceUri) : thisValue.OwnerDocument.CreateAttribute(prefix, localName, namespaceUri);
			attr.Value = Convert.ToString(value);
			thisValue.Attributes.Append(attr);
		}

		public static void Remove([NotNull] this XmlElement thisValue, [NotNull] string name)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			thisValue.RemoveAttribute(name);
		}

		public static void Remove([NotNull] this XmlElement thisValue, [NotNull] string localName, string namespaceUri)
		{
			if (string.IsNullOrEmpty(namespaceUri))
			{
				Remove(thisValue, localName);
				return;
			}

			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));
			thisValue.RemoveAttribute(localName, namespaceUri);
		}

		[NotNull]
		public static XmlElement Append([NotNull] this XmlElement thisValue, [NotNull] string name)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (thisValue.OwnerDocument == null) throw new ArgumentException("The element has no document attached to it");

			XmlElement element = thisValue.OwnerDocument.CreateElement(name);
			thisValue.AppendChild(element);
			return element;
		}

		[NotNull]
		public static XmlElement Append([NotNull] this XmlElement thisValue, [NotNull] string localName, string namespaceURI)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));
			if (thisValue.OwnerDocument == null) throw new ArgumentException("The element has no document attached to it");

			XmlElement element = thisValue.OwnerDocument.CreateElement(localName, namespaceURI);
			thisValue.AppendChild(element);
			return element;
		}

		[NotNull]
		public static XmlElement Append([NotNull] this XmlElement thisValue, string prefix, [NotNull] string localName, string namespaceURI)
		{
			AssertIsWritable(thisValue);
			if (string.IsNullOrEmpty(localName)) throw new ArgumentNullException(nameof(localName));
			if (thisValue.OwnerDocument == null) throw new ArgumentException("The element has no document attached to it");

			XmlElement element = thisValue.OwnerDocument.CreateElement(prefix, localName, namespaceURI);
			thisValue.AppendChild(element);
			return element;
		}

		[NotNull]
		public static XmlComment AppendComment([NotNull] this XmlElement thisValue, [NotNull] string comment)
		{
			AssertIsWritable(thisValue);
			if (thisValue.OwnerDocument == null) throw new ArgumentException("The element has no document attached to it");
			if (string.IsNullOrEmpty(comment)) throw new ArgumentNullException(nameof(comment));

			XmlComment xmlComment = thisValue.OwnerDocument.CreateComment(comment);
			thisValue.AppendChild(xmlComment);
			return xmlComment;
		}

		[NotNull]
		public static XmlCDataSection AppendCData([NotNull] this XmlElement thisValue, [NotNull] string data)
		{
			AssertIsWritable(thisValue);
			if (thisValue.OwnerDocument == null) throw new ArgumentException("The element has no document attached to it");
			if (data == null) throw new ArgumentNullException(nameof(data));

			XmlCDataSection section = thisValue.OwnerDocument.CreateCDataSection(data);
			thisValue.AppendChild(section);
			return section;
		}

		private static void AssertIsWritable([NotNull] XmlElement value)
		{
			if (IsWritable(value)) return;
			throw new ReadOnlyException();
		}
	}
}
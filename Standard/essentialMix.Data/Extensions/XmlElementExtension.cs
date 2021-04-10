using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.XPath;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;
using essentialMix.Web;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XmlElementExtension
	{
		public static int GetIndex(this XmlElement thisValue) { return GetIndex(thisValue, true); }

		public static int GetIndex(this XmlElement thisValue, bool matchName)
		{
			if (thisValue == null) return -1;
			XmlIndexMatchType flags = XmlIndexMatchType.Type;
			if (matchName) flags |= XmlIndexMatchType.Name;
			return thisValue.GetIndex(flags);
		}

		public static XmlNamespaceManager GetNamespaceManager(this XmlElement thisValue)
		{
			return thisValue?.OwnerDocument?.GetNamespaceManager();
		}

		public static int AppendNamespaces(this XmlElement thisValue, [NotNull] params string[] namespaceURI)
		{
			if (thisValue?.OwnerDocument == null || namespaceURI.IsNullOrEmpty()) return -1;

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

		public static IEnumerable<XmlElement> Ancestors(this XmlElement thisValue) { return Ancestors(thisValue, null); }

		[ItemNotNull]
		public static IEnumerable<XmlElement> Ancestors(this XmlElement thisValue, string name)
		{
			if (thisValue == null) yield break;

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

		public static XmlNodeList Elements(this XmlElement thisValue) { return Elements(thisValue, null, null); }
		public static XmlNodeList Elements(this XmlElement thisValue, XmlNamespaceManager manager) { return Elements(thisValue, null, manager); }
		public static XmlNodeList Elements(this XmlElement thisValue, string name) { return Elements(thisValue, name, null); }
		public static XmlNodeList Elements(this XmlElement thisValue, string name, XmlNamespaceManager manager)
		{
			if (thisValue == null || !thisValue.HasChildNodes) return null;
			string expr = name ?? "*";
			return manager == null
						? thisValue.SelectNodes(expr)
						: thisValue.SelectNodes(expr, manager);
		}

		public static XmlNodeList Elements(this XmlElement thisValue, string name, string attribute, string value) { return Elements(thisValue, name, attribute, value, null); }
		public static XmlNodeList Elements(this XmlElement thisValue, string name, string attribute, string value, XmlNamespaceManager manager)
		{
			if (thisValue == null || !thisValue.HasChildNodes) return null;
			string nm = name ?? "*";
			string expr = attribute == null
				? nm
				: $"{nm}[@{attribute}='{value ?? string.Empty}']";
			return manager == null
						? thisValue.SelectNodes(expr)
						: thisValue.SelectNodes(expr, manager);
		}

		public static XmlElement First(this XmlElement thisValue) { return First(thisValue, null, null, null, null); }
		public static XmlElement First(this XmlElement thisValue, string name) { return First(thisValue, name, null, null, null); }
		public static XmlElement First(this XmlElement thisValue, XmlNamespaceManager manager) { return First(thisValue, null, null, null, manager); }
		public static XmlElement First(this XmlElement thisValue, string name, XmlNamespaceManager manager) { return First(thisValue, name, null, null, manager); }
		public static XmlElement First(this XmlElement thisValue, string name, string attribute, string value) { return First(thisValue, name, attribute, value, null); }
		public static XmlElement First(this XmlElement thisValue, string name, string attribute, string value, XmlNamespaceManager manager)
		{
			if (thisValue == null || !thisValue.HasChildNodes) return null;
			string nm = name ?? "*";
			string expr = attribute == null
				? nm
				: $"{nm}[@{attribute}='{value ?? string.Empty}']";
			return thisValue.Select<XmlElement>(expr, manager);
		}

		public static XmlElement Last(this XmlElement thisValue) { return Last(thisValue, null, null, null, null); }
		public static XmlElement Last(this XmlElement thisValue, string name) { return Last(thisValue, name, null, null, null); }
		public static XmlElement Last(this XmlElement thisValue, XmlNamespaceManager manager) { return Last(thisValue, null, null, null, manager); }
		public static XmlElement Last(this XmlElement thisValue, string name, XmlNamespaceManager manager) { return Last(thisValue, name, null, null, manager); }
		public static XmlElement Last(this XmlElement thisValue, string name, string attribute, string value) { return Last(thisValue, name, attribute, value, null); }
		public static XmlElement Last(this XmlElement thisValue, string name, string attribute, string value, XmlNamespaceManager manager)
		{
			if (thisValue == null || !thisValue.HasChildNodes) return null;
			string nm = name ?? "*";
			string expr = attribute == null
				? $"{nm}[last()]"
				: $"{nm}[@{attribute}='{value ?? string.Empty}'][last()]";
			return thisValue.Select<XmlElement>(expr, manager);
		}

		public static T Evaluate<T>(this XmlElement thisValue, string expression) { return Evaluate(thisValue, expression, default(T)); }

		public static T Evaluate<T>(this XmlElement thisValue, string expression, T defaultValue)
		{
			if (!IsValid(thisValue)) return defaultValue;
			XPathNavigator navigator = thisValue.CreateNavigator();
			T result;

			try { result = navigator.Evaluate(expression ?? ".").To(defaultValue); }
			catch { result = defaultValue; }

			return result;
		}

		public static T Evaluate<T>(this XmlElement thisValue, XPathExpression expression) { return Evaluate(thisValue, expression, default(T)); }

		public static T Evaluate<T>(this XmlElement thisValue, XPathExpression expression, T defaultValue)
		{
			if (!IsValid(thisValue) || expression == null) return defaultValue;
			
			XPathNavigator navigator = thisValue.CreateNavigator();
			T result;

			try { result = navigator.Evaluate(expression).To(defaultValue); }
			catch { result = defaultValue; }
			
			return result;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsElement(this XmlElement thisValue, string name) { return IsValid(thisValue) && thisValue.Name.IsSame(name); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsElement(this XmlElement thisValue, string localName, string namespaceURI)
		{
			return IsValid(thisValue) && thisValue.LocalName.IsSame(localName) && thisValue.NamespaceURI.IsSame(namespaceURI);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsValid(this XmlElement thisValue) { return thisValue != null; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsWritable(this XmlElement thisValue) { return IsValid(thisValue) && !thisValue.IsReadOnly; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this XmlElement thisValue, [NotNull] string name) { return Get(thisValue, name, default(T)); }

		public static T Get<T>(this XmlElement thisValue, [NotNull] string name, T defaultValue)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (!IsValid(thisValue)) return defaultValue;

			if (thisValue.HasAttributes)
			{
				XmlAttribute attribute = thisValue.Attributes[name];
				if (attribute != null) return attribute.Get(defaultValue);
			}

			XmlElement child = thisValue.Select<XmlElement>(name);
			return child == null
						? defaultValue
						: child.InnerText.To(defaultValue);
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

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static void AssertIsWritable([NotNull] XmlElement value)
		{
			if (IsWritable(value)) return;
			throw new ReadOnlyException();
		}
	}
}
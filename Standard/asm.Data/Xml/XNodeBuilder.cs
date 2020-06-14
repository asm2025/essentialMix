using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using asm.Extensions;
using asm.Data.Helpers;
using JetBrains.Annotations;

namespace asm.Data.Xml
{
	public class XNodeBuilder : XmlWriter
	{
		private List<object> _content;
		private XmlWriterSettings _settings;
		private XContainer _root;
		private XContainer _parent;
		private XName _attrName;
		private string _attrValue;

		public XNodeBuilder()
			: this(null)
		{
		}

		public XNodeBuilder(XContainer container)
		{
			_root = container;
		}

		[NotNull]
		public override XmlWriterSettings Settings => _settings ??= XmlWriterHelper.CreateSettings();

		public override WriteState WriteState => throw new NotSupportedException();

		public override void Close()
		{
			if (_content.IsNullOrEmpty()) return;
			if (_root == null) throw new InvalidOperationException("No container node was found to hold the contents");
			_root.Add(_content);
		}

		public override void Flush() { }

		public override string LookupPrefix(string namespaceName) { throw new NotSupportedException(); }

		public override void WriteBase64(byte[] buffer, int index, int count) { throw new NotSupportedException(); }

		public override void WriteCData(string text) { AddNode(new XCData(text)); }

		public override void WriteCharEntity(char ch) { AddString(new string(ch, 1)); }

		public override void WriteChars(char[] buffer, int index, int count) { AddString(new string(buffer, index, count)); }

		public override void WriteComment(string text) { AddNode(new XComment(text)); }

		public override void WriteDocType(string name, string pubid, string sysid, string subset) { AddNode(new XDocumentType(name, pubid, sysid, subset)); }

		public override void WriteEndAttribute()
		{
			XAttribute xattribute = new XAttribute(_attrName, _attrValue);
			_attrName = null;
			_attrValue = null;

			if (_parent != null) _parent.Add(xattribute);
			else Add(xattribute);
		}

		public override void WriteEndDocument() { }

		public override void WriteEndElement() { _parent = _parent.Parent; }

		public override void WriteEntityRef(string name)
		{
			switch (name)
			{
				case "amp":
					AddString("&");
					break;
				case "apos":
					AddString("'");
					break;
				case "gt":
					AddString(">");
					break;
				case "lt":
					AddString("<");
					break;
				case "quot":
					AddString("\"");
					break;
				default:
					throw new NotSupportedException();
			}
		}

		public override void WriteFullEndElement()
		{
			XElement xelement = (XElement)_parent;
			if (xelement.IsEmpty) xelement.Add(string.Empty);
			_parent = xelement.Parent;
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			if (name == "xml") return;
			AddNode(new XProcessingInstruction(name, text));
		}

		public override void WriteRaw(char[] buffer, int index, int count) { AddString(new string(buffer, index, count)); }

		public override void WriteRaw(string data) { AddString(data); }

		public override void WriteStartAttribute(string prefix, string localName, string namespaceName)
		{
			if (prefix == null) throw new ArgumentNullException(nameof(prefix));
			_attrName = XNamespace.Get(prefix.Length == 0 ? string.Empty : namespaceName).GetName(localName);
			_attrValue = string.Empty;
		}

		public override void WriteStartDocument() { }

		public override void WriteStartDocument(bool standalone) { }

		public override void WriteStartElement(string prefix, string localName, string namespaceName)
		{
			AddNode(new XElement(XNamespace.Get(namespaceName).GetName(localName)));
		}

		public override void WriteString(string text) { AddString(text); }

		public override void WriteSurrogateCharEntity(char lowCh, char highCh) { AddString(new string(new[] {highCh, lowCh})); }

		public override void WriteWhitespace(string ws) { AddString(ws); }

		public XNode Root => _root;

		private void Add(object o)
		{
			_content ??= new List<object>();
			_content.Add(o);
		}

		private void AddNode(XNode n)
		{
			XContainer xcontainer = n as XContainer;
			bool isContainer = xcontainer != null;

			if (_root == null && isContainer)
			{
				_root = xcontainer;
				return;
			}

			if (_parent != null) 
				_parent.Add(n);
			else 
				Add(n);

			if (!isContainer) return;
			_parent = xcontainer;
		}

		private void AddString(string s)
		{
			if (s == null) return;

			if (_attrValue != null)
			{
				XNodeBuilder xnodeBuilder = this;
				string str = xnodeBuilder._attrValue + s;
				xnodeBuilder._attrValue = str;
			}
			else if (_parent != null) 
				_parent.Add(s);
			else 
				Add(s);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XmlReaderExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsValid(this XmlReader thisValue) { return thisValue != null; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool CanCollectAttributes(this XmlReader thisValue, XmlElement element)
		{
			return CanRead(thisValue) &&
					thisValue.NodeType == XmlNodeType.Element &&
					element?.OwnerDocument != null &&
					thisValue.LocalName.IsSame(element.LocalName) &&
					thisValue.NamespaceURI.IsSame(element.NamespaceURI);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool CanCollectChildren(this XmlReader thisValue, XmlElement element)
		{
			return CanRead(thisValue) &&
					thisValue.NodeType == XmlNodeType.Element &&
					element?.OwnerDocument != null &&
					thisValue.LocalName.IsSame(element.LocalName) &&
					thisValue.NamespaceURI.IsSame(element.NamespaceURI);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool CanRead(this XmlReader thisValue)
		{
			return IsValid(thisValue) && (thisValue.ReadState == ReadState.Initial || thisValue.ReadState == ReadState.Interactive);
		}

		public static void CollectAttributes(this XmlReader thisValue, XmlElement element) { CollectAttributes(thisValue, element, true); }
		public static void CollectAttributes(this XmlReader thisValue, XmlElement element, bool stopOnErrors)
		{
			if (!CanCollectAttributes(thisValue, element)) return;
			element.IsEmpty = thisValue.IsEmptyElement;
			if (!thisValue.HasAttributes) return;

			XmlDocument document = element.OwnerDocument;
			if (document == null) return;

			try
			{
				while(thisValue.MoveToNextAttribute())
				{
					XmlAttribute attribute = document.CreateAttribute(thisValue.Prefix, thisValue.LocalName, thisValue.NamespaceURI);
					if (thisValue.HasValue) attribute.Value = thisValue.Value;
					element.Attributes.Append(attribute);
				}
			}
			catch (XmlException)
			{
				if (stopOnErrors) throw;
			}

			thisValue.MoveToElement();
		}

		public static void CollectChildren(this XmlReader thisValue, XmlElement element, bool ignoreComments) { CollectChildren(thisValue, element, ignoreComments, true); }
		public static void CollectChildren(this XmlReader thisValue, XmlElement element, bool ignoreComments, bool stopOnErrors)
		{
			if (!CanCollectChildren(thisValue, element)) return;

			XmlDocument document = element.OwnerDocument;
			if (document == null) return;

			Stack<XmlElement> openElements = new Stack<XmlElement>();
			XmlElement current = element;
			openElements.Push(current);

			try
			{
				while(current != null && thisValue.Read())
				{
					switch (thisValue.NodeType)
					{
						case XmlNodeType.Element:
							XmlElement child = document.CreateElement(thisValue.Prefix, thisValue.LocalName, thisValue.NamespaceURI);
							CollectAttributes(thisValue, child);
							current.AppendChild(child);
							if (child.IsEmpty) continue;
							openElements.Push(child);
							current = child;
							break;
						case XmlNodeType.EndElement:
							openElements.Pop();
							current = openElements.Count > 0 ? openElements.Peek() : null;
							break;
						case XmlNodeType.Attribute:
							XmlAttribute attribute = document.CreateAttribute(thisValue.Prefix, thisValue.LocalName, thisValue.NamespaceURI);
							if (thisValue.HasValue) attribute.Value = thisValue.Value;
							current.AppendChild(attribute);
							break;
						case XmlNodeType.Text:
							current.AppendChild(document.CreateTextNode(thisValue.Value));
							break;
						case XmlNodeType.CDATA:
							current.AppendChild(document.CreateCDataSection(thisValue.Value));
							break;
						case XmlNodeType.ProcessingInstruction:
							current.AppendChild(document.CreateProcessingInstruction(thisValue.Name, thisValue.Value));
							break;
						case XmlNodeType.Comment:
							if (!ignoreComments) current.AppendChild(document.CreateComment(thisValue.Value));
							break;
						case XmlNodeType.EntityReference:
							current.AppendChild(document.CreateEntityReference(thisValue.Name));
							break;
					}
				}
			}
			catch (XmlException)
			{
				if (stopOnErrors) throw;
			}
		}

		public static object DeserializeXml([NotNull] this XmlReader thisValue, XmlDeserializationEvents? events = null, params Type[] extraTypes)
		{
			return DeserializeXml(thisValue, (object)null, events, extraTypes);
		}

		public static T DeserializeXml<T>([NotNull] this XmlReader thisValue, T defaultValue, XmlDeserializationEvents? events = null, params Type[] extraTypes)
		{
			if (thisValue == null) throw new ArgumentNullException(nameof(thisValue));

			XmlSerializer serializer = extraTypes.IsNullOrEmpty() ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes);

			try
			{
				return (T)(events.HasValue
								? serializer.Deserialize(thisValue, events.Value)
								: serializer.Deserialize(thisValue));
			}
			catch
			{
				return defaultValue;
			}
		}

		public static object DeserializeDataContract([NotNull] this XmlReader thisValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(thisValue, (object)null, verifyObjectName, settings);
		}

		public static T DeserializeDataContract<T>([NotNull] this XmlReader thisValue, T defaultValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(T), settings ?? new DataContractSerializerSettings());

			try
			{
				return (T)dcs.ReadObject(thisValue, verifyObjectName);
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}
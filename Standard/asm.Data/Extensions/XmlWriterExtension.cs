using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XmlWriterExtension
	{
		public static bool IsValid(this XmlWriter thisValue) { return thisValue != null; }

		public static bool CanWrite([NotNull] this XmlWriter thisValue) { return IsValid(thisValue) && thisValue.WriteState == WriteState.Closed; }

		public static bool SerializeXml<T>([NotNull] this XmlWriter thisValue, T value, XmlSerializerNamespaces namespaces = null, params Type[] extraTypes)
		{
			if (value.IsNull()) return false;

			XmlSerializer serializer = extraTypes.IsNullOrEmpty() ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes);

			try
			{
				serializer.Serialize(thisValue, value, namespaces);
				thisValue.Flush();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool SerializeDataContract<T>([NotNull] this XmlWriter thisValue, T value, DataContractSerializerSettings settings = null)
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(T), settings ?? new DataContractSerializerSettings());

			try
			{
				dcs.WriteObject(thisValue, thisValue);
				thisValue.Flush();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Data.Helpers;

namespace asm.Data.Extensions
{
	public static class TextWriterExtension
	{
		public static bool SerializeXml<T>([NotNull] this TextWriter thisValue, T value, XmlSerializerNamespaces namespaces = null, params Type[] extraTypes)
		{
			XmlSerializer serializer = extraTypes.IsNullOrEmpty() ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes);

			try
			{
				serializer.Serialize(thisValue, value, namespaces);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool SerializeDataContract<T>([NotNull] this TextWriter thisValue, T value, DataContractSerializerSettings settings = null, XmlWriterSettings xmlOptions = null)
		{
			XmlWriterSettings opt = xmlOptions ?? XmlWriterHelper.CreateSettings();

			using (XmlWriter writer = XmlWriter.Create(thisValue, opt))
				return writer.SerializeDataContract(value, settings);
		}
	}
}
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Data.Extensions
{
	public static class StreamExtension
	{
		public static bool SerializeXml<T>([NotNull] this Stream thisValue, T value, XmlSerializerNamespaces namespaces = null, params Type[] extraTypes)
		{
			if (value.IsNull()) return false;

			using (StreamWriter writer = new StreamWriter(thisValue))
				return writer.SerializeXml(value, namespaces, extraTypes);
		}

		public static object DeserializeXml([NotNull] this Stream thisValue, params Type[] extraTypes) { return DeserializeXml(thisValue, (object)null, null, extraTypes); }

		public static T DeserializeXml<T>([NotNull] this Stream thisValue, T defaultValue, params Type[] extraTypes)
		{
			return DeserializeXml(thisValue, defaultValue, null, extraTypes);
		}

		public static object DeserializeXml([NotNull] this Stream thisValue, Encoding encoding, params Type[] extraTypes)
		{
			return DeserializeXml(thisValue, (object)null, encoding, extraTypes);
		}

		public static T DeserializeXml<T>([NotNull] this Stream thisValue, T defaultValue, Encoding encoding, params Type[] extraTypes)
		{
			using (TextReader reader = new StreamReader(thisValue, encoding ?? EncodingHelper.Default, true))
				return reader.DeserializeXml(defaultValue, extraTypes);
		}

		public static bool SerializeDataContract<T>([NotNull] this Stream thisValue, T value, DataContractSerializerSettings settings = null)
		{
			if (value.IsNull()) return false;

			using (StreamWriter writer = new StreamWriter(thisValue))
				return writer.SerializeDataContract(value, settings);
		}

		public static object DeserializeDataContract([NotNull] this Stream thisValue, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(thisValue, (object)null, false, settings);
		}

		public static T DeserializeDataContract<T>([NotNull] this Stream thisValue, T defaultValue, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(thisValue, defaultValue, null, false, settings);
		}

		public static object DeserializeDataContract([NotNull] this Stream thisValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(thisValue, (object)null, verifyObjectName, settings);
		}

		public static T DeserializeDataContract<T>([NotNull] this Stream thisValue, T defaultValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(thisValue, defaultValue, null, verifyObjectName, settings);
		}

		public static object DeserializeDataContract([NotNull] this Stream thisValue, Encoding encoding, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(thisValue, (object)null, encoding, verifyObjectName, settings);
		}

		public static T DeserializeDataContract<T>([NotNull] this Stream thisValue, T defaultValue, Encoding encoding, bool verifyObjectName = false, DataContractSerializerSettings settings = null, XmlReaderSettings xmlOptions = null, XmlParserContext xmlContext = null)
		{
			using (TextReader reader = new StreamReader(thisValue, encoding ?? EncodingHelper.Default, true))
				return reader.DeserializeDataContract(defaultValue, verifyObjectName, settings, xmlOptions, xmlContext);
		}
	}
}
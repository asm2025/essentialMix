using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using asm.Data.Extensions;
using JetBrains.Annotations;

namespace asm.Data.Helpers
{
	public static class StringHelper
	{
		public static object DeserializeXml(string value, params Type[] extraTypes) { return DeserializeXml(value, (object)null, extraTypes); }

		public static T DeserializeXml<T>(string value, T defaultValue, params Type[] extraTypes)
		{
			if (string.IsNullOrEmpty(value)) return defaultValue;

			using (TextReader reader = new StringReader(value))
				return reader.DeserializeXml(defaultValue, extraTypes);
		}

		public static object DeserializeDataContract(string value, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
		{
			return DeserializeDataContract(value, (object)null, verifyObjectName, settings);
		}

		public static T DeserializeDataContract<T>(string value, T defaultValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null, XmlReaderSettings xmlOptions = null, XmlParserContext xmlContext = null)
		{
			if (string.IsNullOrEmpty(value)) return defaultValue;

			using (TextReader reader = new StringReader(value))
				return reader.DeserializeDataContract(defaultValue, verifyObjectName, settings, xmlOptions, xmlContext);
		}
	}
}
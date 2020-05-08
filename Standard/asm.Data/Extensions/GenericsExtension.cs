using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class GenericsExtension
	{
		public static bool SerializeXml<T>([NotNull] this T thisValue, [NotNull] Stream stream, XmlSerializerNamespaces namespaces = null, params Type[] extraTypes)
		{
			return stream.SerializeXml(thisValue, namespaces, extraTypes);
		}

		public static bool SerializeXml<T>([NotNull] this T thisValue, [NotNull] TextWriter writer, XmlSerializerNamespaces namespaces = null, params Type[] extraTypes)
		{
			return writer.SerializeXml(thisValue, namespaces, extraTypes);
		}

		public static bool SerializeXml<T>([NotNull] this T thisValue, [NotNull] XmlWriter writer, XmlSerializerNamespaces namespaces = null, params Type[] extraTypes)
		{
			return writer.SerializeXml(thisValue, namespaces, extraTypes);
		}

		public static bool SerializeDataContract<T>([NotNull] this T thisValue, [NotNull] Stream stream, DataContractSerializerSettings settings = null)
		{
			return stream.SerializeDataContract(thisValue, settings);
		}

		public static bool SerializeDataContract<T>([NotNull] this T thisValue, [NotNull] TextWriter writer, DataContractSerializerSettings settings = null, XmlWriterSettings xmlOptions = null)
		{
			return writer.SerializeDataContract(thisValue, settings, xmlOptions);
		}

		public static bool SerializeDataContract<T>([NotNull] this T thisValue, [NotNull] XmlWriter writer, DataContractSerializerSettings settings = null)
		{
			return writer.SerializeDataContract(thisValue, settings);
		}
	}
}
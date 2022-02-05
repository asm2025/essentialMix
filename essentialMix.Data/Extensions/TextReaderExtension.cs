using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using essentialMix.Data.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class TextReaderExtension
{
	public static object DeserializeXml([NotNull] this TextReader thisValue, params Type[] extraTypes) { return DeserializeXml(thisValue, (object)null, extraTypes); }

	public static T DeserializeXml<T>([NotNull] this TextReader thisValue, T defaultValue, params Type[] extraTypes)
	{
		XmlSerializer serializer = extraTypes.IsNullOrEmpty() ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), extraTypes);

		try
		{
			return (T)serializer.Deserialize(thisValue);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static object DeserializeDataContract([NotNull] this TextReader thisValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null)
	{
		return DeserializeDataContract(thisValue, (object)null, verifyObjectName, settings);
	}

	public static T DeserializeDataContract<T>([NotNull] this TextReader thisValue, T defaultValue, bool verifyObjectName = false, DataContractSerializerSettings settings = null, XmlReaderSettings xmlOptions = null, XmlParserContext xmlContext = null)
	{
		XmlReaderSettings opt = xmlOptions ?? XmlReaderHelper.CreateSettings();
		XmlParserContext context = xmlContext ?? XmlReaderHelper.CreateParserContext(opt.NameTable);

		using (XmlReader reader = XmlReader.Create(thisValue, opt, context))
		{
			try
			{
				return reader.DeserializeDataContract(defaultValue, verifyObjectName, settings);
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}
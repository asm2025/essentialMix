using System.Collections.Generic;
using essentialMix.Exceptions.Collections;
using essentialMix.Helpers;
using essentialMix.Newtonsoft.Converters;
using essentialMix.Newtonsoft.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class JsonSerializerSettingsExtension
	{
		[NotNull]
		public static JsonSerializerSettings AddConverters([NotNull] this JsonSerializerSettings thisValue, JsonSerializerSettingsConverters convertersToAdd = JsonSerializerSettingsConverters.Default)
		{
			IList<JsonConverter> converters = thisValue.Converters;
			if (converters.IsReadOnly) throw new ReadOnlyException();
			if (convertersToAdd == JsonSerializerSettingsConverters.Default) convertersToAdd = EnumHelper<JsonSerializerSettingsConverters>.GetAllFlags();

			if (EnumHelper<JsonSerializerSettingsConverters>.HasFlag(convertersToAdd, JsonSerializerSettingsConverters.StringEnum | JsonSerializerSettingsConverters.StringEnumTranslation))
				convertersToAdd &= ~JsonSerializerSettingsConverters.StringEnumTranslation;
			
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.Binary)) converters.Add(new BinaryConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.DataSet)) converters.Add(new DataSetConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.DataTable)) converters.Add(new DataTableConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.EntityKeyMember)) converters.Add(new EntityKeyMemberConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.ExpandoObject)) converters.Add(new ExpandoObjectConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.IsoDateTime)) converters.Add(new IsoDateTimeConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.JavaScriptDateTime)) converters.Add(new JavaScriptDateTimeConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.UnixDateTime)) converters.Add(new UnixDateTimeConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.KeyValuePair)) converters.Add(new KeyValuePairConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.Regex)) converters.Add(new RegexConverter());

			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.StringEnumTranslation)) converters.Add(new StringEnumTranslationConverter { Culture = thisValue.Culture });
			else if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.StringEnum)) converters.Add(new StringEnumConverter());
			
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.Version)) converters.Add(new VersionConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.XmlNode)) converters.Add(new XmlNodeConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.Uri)) converters.Add(new UriConverter());
			if (convertersToAdd.FastHasFlag(JsonSerializerSettingsConverters.Keys)) converters.Add(new KeysConverter());
			return thisValue;
		}
	}
}
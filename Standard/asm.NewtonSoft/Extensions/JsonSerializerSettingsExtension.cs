using System.Collections.Generic;
using asm.Exceptions.Collections;
using asm.Helpers;
using asm.Newtonsoft.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace asm.Newtonsoft.Extensions
{
	public static class JsonSerializerSettingsExtension
	{
		public static JsonSerializerSettings AddConverters([NotNull] this JsonSerializerSettings thisValue, JsonSerializerSettingsConverters convertersToAdd = JsonSerializerSettingsConverters.Default)
		{
			IList<JsonConverter> converters = thisValue.Converters;
			if (converters.IsReadOnly) throw new ReadOnlyException();
			if (convertersToAdd == JsonSerializerSettingsConverters.Default) convertersToAdd = EnumHelper<JsonSerializerSettingsConverters>.GetAllFlags();

			if (EnumHelper<JsonSerializerSettingsConverters>.HasAllFlags(convertersToAdd, JsonSerializerSettingsConverters.StringEnum | JsonSerializerSettingsConverters.StringEnumTranslation))
				convertersToAdd &= ~JsonSerializerSettingsConverters.StringEnumTranslation;
			
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.Binary)) converters.Add(new BinaryConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.DataSet)) converters.Add(new DataSetConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.DataTable)) converters.Add(new DataTableConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.EntityKeyMember)) converters.Add(new EntityKeyMemberConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.ExpandoObject)) converters.Add(new ExpandoObjectConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.IsoDateTime)) converters.Add(new IsoDateTimeConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.JavaScriptDateTime)) converters.Add(new JavaScriptDateTimeConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.UnixDateTime)) converters.Add(new UnixDateTimeConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.KeyValuePair)) converters.Add(new KeyValuePairConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.Regex)) converters.Add(new RegexConverter());

			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.StringEnumTranslation)) converters.Add(new StringEnumTranslationConverter { Culture = thisValue.Culture ?? CultureInfoHelper.Default });
			else if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.StringEnum)) converters.Add(new StringEnumConverter());
			
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.Version)) converters.Add(new VersionConverter());
			if (convertersToAdd.HasFlag(JsonSerializerSettingsConverters.XmlNode)) converters.Add(new XmlNodeConverter());
			return thisValue;
		}
	}
}
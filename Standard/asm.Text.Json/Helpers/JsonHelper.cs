using System.Text.Encodings.Web;
using System.Text.Json;
using JetBrains.Annotations;

namespace asm.Text.Json.Helpers
{
	public static class JsonHelper
	{
		[NotNull]
		public static JsonSerializerOptions CreateSettings(bool? indent = null, bool? propertyNameCaseInsensitive = null, JsonNamingPolicy propertyNamingPolicy = null)
		{
			JsonSerializerOptions settings = new JsonSerializerOptions();
			SetDefaults(settings, indent, propertyNameCaseInsensitive, propertyNamingPolicy);
			return settings;
		}

		[NotNull]
		public static JsonSerializerOptions SetDefaults([NotNull] JsonSerializerOptions value, bool? indent = null, bool? propertyNameCaseInsensitive = null, JsonNamingPolicy propertyNamingPolicy = null, JsonNamingPolicy dictionaryKeyPolicy = null, JavaScriptEncoder encoder = null)
		{
			if (indent.HasValue) value.WriteIndented = indent == true;
			if (propertyNameCaseInsensitive.HasValue) value.PropertyNameCaseInsensitive = propertyNameCaseInsensitive == true;
			value.AllowTrailingCommas = true;
			value.PropertyNamingPolicy = propertyNamingPolicy;
			value.DictionaryKeyPolicy = dictionaryKeyPolicy;
			if (encoder != null) value.Encoder = encoder;
			value.IgnoreNullValues = false;
			value.IgnoreReadOnlyProperties = false;
			value.ReadCommentHandling = JsonCommentHandling.Skip;
			return value;
		}
	}
}
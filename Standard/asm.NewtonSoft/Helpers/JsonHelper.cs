using System;
using System.Globalization;
using System.Runtime.Serialization;
using asm.Comparers;
using asm.Extensions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace asm.Newtonsoft.Helpers
{
	public static class JsonHelper
	{
		[NotNull]
		public static JsonSerializerSettings CreateSettings(bool? indent = null, PreserveReferencesHandling referencesHandling = PreserveReferencesHandling.None, ReferenceLoopHandling loopHandling = ReferenceLoopHandling.Ignore, IContractResolver contractResolver = null, CultureInfo culture = null)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			SetDefaults(settings, indent, referencesHandling, loopHandling, contractResolver, culture);
			return settings;
		}

		[NotNull]
		public static JsonSerializerSettings SetDefaults([NotNull] JsonSerializerSettings value, bool? indent = null, PreserveReferencesHandling referencesHandling = PreserveReferencesHandling.None, ReferenceLoopHandling loopHandling = ReferenceLoopHandling.Ignore, IContractResolver contractResolver = null, CultureInfo culture = null)
		{
			value.ObjectCreationHandling = ObjectCreationHandling.Replace;
			value.Context = new StreamingContext(StreamingContextStates.Clone);
			value.EqualityComparer = ReferenceComparer.Default;
			value.CheckAdditionalContent = true;
			value.NullValueHandling = NullValueHandling.Ignore;
			value.PreserveReferencesHandling = referencesHandling;
			value.ReferenceLoopHandling = loopHandling;
			value.DateFormatString = "O";

			if (indent.HasValue)
			{
				value.Formatting = indent == true
										? Formatting.Indented
										: Formatting.None;
			}

			if (contractResolver != null) value.ContractResolver = contractResolver;
			if (culture != null) value.Culture = culture;
			return value;
		}

		[NotNull]
		public static JsonLoadSettings CreateLoadSettings()
		{
			return new JsonLoadSettings
					{
						CommentHandling = CommentHandling.Ignore,
						LineInfoHandling = LineInfoHandling.Ignore
					};
		}

		[NotNull]
		public static JObject Deserialize(string value, JsonLoadSettings settings = null)
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
			return JObject.Parse(value, settings ?? CreateLoadSettings());
		}

		public static T Deserialize<T>(string value, T defaultValue, JsonSerializerSettings settings = null) { return (T)Deserialize(value, typeof(T), defaultValue, settings); }
		public static object Deserialize(string value, [NotNull] Type type, JsonSerializerSettings settings = null) { return Deserialize(value, type, type.Default(), settings); }
		public static object Deserialize(string value, [NotNull] Type type, object defaultValue = null, JsonSerializerSettings settings = null)
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return defaultValue;

			try
			{
				return JsonConvert.DeserializeObject(value, type, settings ?? CreateSettings());
			}
			catch
			{
				return defaultValue;
			}
		}
	}
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Newtonsoft.Converters
{
	public class KeysConverter : JsonConverter
	{
		private static readonly ConcurrentDictionary<Type, bool> __types = new ConcurrentDictionary<Type, bool>();

		/// <inheritdoc />
		public KeysConverter() 
		{
		}

		/// <inheritdoc />
		public KeysConverter([NotNull] NamingStrategy namingStrategy, bool allowIntegerValues = true)
		{
			NamingStrategy = namingStrategy;
			AllowIntegerValues = allowIntegerValues;
		}

		/// <inheritdoc />
		public override bool CanRead { get; } = false;

		/// <summary>
		/// Gets or sets the naming strategy used to resolve how enum text is written.
		/// </summary>
		/// <value>The naming strategy used to resolve how enum text is written.</value>
		public NamingStrategy NamingStrategy { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether integer values are allowed when serializing and deserializing.
		/// The default value is <c>true</c>.
		/// </summary>
		/// <value><c>true</c> if integers are allowed when serializing and deserializing; otherwise, <c>false</c>.</value>
		public bool AllowIntegerValues { get; set; } = true;

		[NotNull]
		public static ICollection<Type> RegisteredTypes => __types.Keys;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType) { return objectType != null && IsTypeIncluded(objectType); }

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, global::Newtonsoft.Json.JsonSerializer jsonSerializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			
			Type type = value.AsType();

			if (!IsTypeIncluded(type))
			{
				jsonSerializer.Serialize(writer, value);
				return;
			}

			JToken token = JToken.FromObject(value);

			if (token.Type != JTokenType.Object)
			{
				token.WriteTo(writer);
			}
			else
			{
				JObject jObject = (JObject)token;
				jObject.AddFirst(new JProperty("keys", new JArray(jObject.Properties().Select(e => e.Name))));
				jObject.WriteTo(writer);
			}
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, global::Newtonsoft.Json.JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}

		public static void Register([NotNull] Type type)
		{
			if (!type.IsEnum) throw new InvalidEnumArgumentException();
			__types.AddOrUpdate(type, _ => true, (_, _) => true);
		}

		public static void Unregister([NotNull] Type type)
		{
			if (!type.IsEnum) throw new InvalidEnumArgumentException();
			__types.TryRemove(type, out _);
		}

		public static void Clear()
		{
			__types.Clear();
		}

		public static bool IsTypeRegistered([NotNull] Type type) { return type.IsEnum && __types.ContainsKey(type); }
		public static bool IsTypeIncluded([NotNull] Type type) { return type.IsEnum && __types.TryGetValue(type, out bool enabled) && enabled; }
	}
}
using System;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace asm.Newtonsoft.Converters
{
	public class UriConverter : JsonConverter
	{
		/// <inheritdoc />
		public UriConverter()
			: this(true)
		{
		}

		/// <inheritdoc />
		public UriConverter(bool ensureSchema)
		{
			EnsureSchema = ensureSchema;
		}

		public bool EnsureSchema { get; set; }

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return objectType != null && (typeof(Uri).IsAssignableFrom(objectType) || typeof(UriBuilder).IsAssignableFrom(objectType));
		}

		/// <inheritdoc />
		public override void WriteJson([NotNull] JsonWriter writer, object value, global::Newtonsoft.Json.JsonSerializer jsonSerializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			Uri uri = value switch
			{
				Uri uriValue => uriValue,
				UriBuilder builder => builder.Uri,
				string urlStr => UriHelper.ToUri(urlStr),
				_ => throw new InvalidOperationException("Value is not in the correct format.")
			};

			writer.WriteValue(uri.String());
		}

		/// <inheritdoc />
		public override object ReadJson([NotNull] JsonReader reader, Type objectType, object existingValue, global::Newtonsoft.Json.JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Null:
				case JsonToken.None:
				case JsonToken.Undefined:
					return null;
				case JsonToken.String:
					string value = reader.Value?.ToString();
					return value == null
								? null
								: UriHelper.ToUri(value);
				default:
					throw new InvalidOperationException("Value is not in the correct format.");
			}
		}
	}
}
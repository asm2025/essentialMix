using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using asm.Helpers;
using asm.Json.Abstraction;
using asm.Text.Json.Helpers;
using TextJsonSerializer = System.Text.Json.JsonSerializer;

namespace asm.Text.Json
{
	public class JsonSerializer : JsonSerializer<JsonSerializerOptions>
	{
		/// <inheritdoc />
		public JsonSerializer() 
		{
		}

		/// <inheritdoc />
		public override Func<JsonSerializerOptions> DefaultSettings { get; set; } = () => JsonHelper.CreateSettings();

		/// <inheritdoc />
		public override string Serialize(object value, Type type, JsonSerializerOptions settings)
		{
			return TextJsonSerializer.Serialize(value, type, settings ?? DefaultSettings());
		}

		/// <inheritdoc />
		public override object Deserialize(string value, Type type, JsonSerializerOptions settings)
		{
			return TextJsonSerializer.Deserialize(value, type, settings ?? DefaultSettings());
		}

		/// <inheritdoc />
		public override ValueTask<object> DeserializeAsync(Stream stream, Type type, JsonSerializerOptions settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return TextJsonSerializer.DeserializeAsync(stream, type, settings ?? DefaultSettings(), token);
		}

		/// <inheritdoc />
		public override void Populate(string value, object target, JsonSerializerOptions settings)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc />
		public override void Populate(string value, IDictionary<string, string> dictionary)
		{
			JsonDocument document = null;

			try
			{
				document = JsonDocument.Parse(value, new JsonDocumentOptions
				{
					AllowTrailingCommas = true,
					CommentHandling = JsonCommentHandling.Skip
				});

				JsonElement jObject = document.RootElement;
				// short circuit for axios
				if (jObject.TryGetProperty("params", out JsonElement jParams)) jObject = jParams;

				foreach (JsonProperty property in jObject.EnumerateObject())
				{
					AddJsonProperties(dictionary, null, property);
				}
			}
			finally
			{
				ObjectHelper.Dispose(ref document);
			}

			void AddJsonProperties(IDictionary<string, string> values, string path, JsonProperty root)
			{
				path = string.Join(".", path, root.Name);

				switch (root.Value.ValueKind)
				{
					case JsonValueKind.Array:
					case JsonValueKind.Object:
						foreach (JsonProperty property in root.Value.EnumerateObject())
						{
							AddJsonProperties(dictionary, path, property);
						}
						break;
					default:
						values[path] = Convert.ToString(root.Value);
						break;
				}
			}
		}

		/// <inheritdoc />
		public override string ToJson(XmlNode node, bool omitRootObject = false)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc />
		public override string ToJson(XNode node, bool omitRootObject = false)
		{
			throw new NotSupportedException();
		}
	}
}

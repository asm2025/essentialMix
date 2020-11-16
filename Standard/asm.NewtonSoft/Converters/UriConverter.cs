using System;
using System.IO;
using System.Text;
using asm.Extensions;
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
		public override void WriteJson(JsonWriter writer, object value, global::Newtonsoft.Json.JsonSerializer jsonSerializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			Uri uri = (Uri)value;
			base.WriteJson(writer, value, jsonSerializer);
		}
	}
}
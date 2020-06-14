using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using asm.Helpers;
using asm.Newtonsoft.Helpers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace asm.Newtonsoft
{
	public class JsonSerializer : Json.Abstraction.JsonSerializer<JsonSerializerSettings>
	{
		/// <inheritdoc />
		public JsonSerializer() 
		{
		}

		/// <inheritdoc />
		public override Func<JsonSerializerSettings> DefaultSettings { get; set; } = () => JsonHelper.CreateSettings();

		/// <inheritdoc />
		[NotNull]
		public override string Serialize(object value, Type type, JsonSerializerSettings settings)
		{
			return JsonConvert.SerializeObject(value, type, settings ?? DefaultSettings());
		}

		/// <inheritdoc />
		public override object Deserialize(string value, Type type, JsonSerializerSettings settings)
		{
			return JsonConvert.DeserializeObject(value, type, settings ?? DefaultSettings());
		}

		/// <inheritdoc />
		public override async ValueTask<object> DeserializeAsync(Stream stream, Type type, JsonSerializerSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			StreamReader reader = null;

			try
			{
				reader = new StreamReader(stream, Encoding.UTF8);
				string value = await reader.ReadToEndAsync();
				token.ThrowIfCancellationRequested();
				return Deserialize(value, type, settings);
			}
			finally
			{
				ObjectHelper.Dispose(ref reader);
			}
		}

		/// <inheritdoc />
		public override void Populate(string value, object target, JsonSerializerSettings settings)
		{
			JsonConvert.PopulateObject(value, target, settings ?? DefaultSettings());
		}

		/// <inheritdoc />
		public override void Populate(string value, IDictionary<string, string> dictionary)
		{
			JObject jObject = JObject.Parse(value, JsonHelper.CreateLoadSettings());
			// Can do it also using jObject.SelectTokens("$..*") but AddJsonProperties() has more control
			// short circuit for axios
			if (jObject["params"] is JObject jParams) jObject = new JObject(jParams.Children());
			AddJsonProperties(dictionary, jObject);

			static void AddJsonProperties(IDictionary<string, string> values, JToken root)
			{
				if (root.Type == JTokenType.Property)
				{
					JProperty property = (JProperty)root;
					if (property.Name != "$id") values[property.Path] = Convert.ToString(property.Value);
				}

				foreach (JToken child in root.Where(e => e.HasValues))
					AddJsonProperties(values, child);
			}
		}

		/// <inheritdoc />
		[NotNull]
		public override string ToJson(XmlNode node, bool omitRootObject = false)
		{
			return ToJson(node, Formatting.None, omitRootObject);
		}

		[NotNull]
		public string ToJson([NotNull] XmlNode node, Formatting formatting, bool omitRootObject = false)
		{
			return JsonConvert.SerializeXmlNode(node, formatting, omitRootObject);
		}

		/// <inheritdoc />
		[NotNull]
		public override string ToJson(XNode node, bool omitRootObject = false)
		{
			return ToJson(node, Formatting.None, omitRootObject);
		}

		[NotNull]
		public string ToJson([NotNull] XNode node, Formatting formatting, bool omitRootObject = false)
		{
			return JsonConvert.SerializeXNode(node, formatting, omitRootObject);
		}
	}
}

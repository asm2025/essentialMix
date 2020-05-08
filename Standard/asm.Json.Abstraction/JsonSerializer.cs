using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace asm.Json.Abstraction
{
	public abstract class JsonSerializer : IJsonSerializer
	{
		/// <inheritdoc />
		protected JsonSerializer() 
		{
		}

		/// <inheritdoc />
		public string Serialize(object value) { return Serialize(value, value.GetType()); }

		/// <inheritdoc />
		public abstract string Serialize(object value, Type type);

		/// <inheritdoc />
		public T Deserialize<T>(string value) { return (T)Deserialize(value, typeof(T)); }

		/// <inheritdoc />
		public abstract object Deserialize(string value, Type type);

		/// <inheritdoc />
		public async ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return (T)await DeserializeAsync(stream, typeof(T), token);
		}

		/// <inheritdoc />
		public abstract ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public abstract void Populate(string value, object target);

		/// <inheritdoc />
		public abstract void Populate(string value, IDictionary<string, string> dictionary);

		/// <inheritdoc />
		public abstract string ToJson(XmlNode node, bool omitRootObject = false);

		/// <inheritdoc />
		public abstract string ToJson(XNode node, bool omitRootObject = false);
	}

	public abstract class JsonSerializer<TSettings> : JsonSerializer, IJsonSerializer<TSettings>
		where TSettings : class, new()
	{
		/// <inheritdoc />
		protected JsonSerializer() 
		{
		}

		/// <inheritdoc />
		public abstract Func<TSettings> DefaultSettings { get; set; }

		/// <inheritdoc />
		public override string Serialize(object value, Type type) { return Serialize(value, type, null); }

		/// <inheritdoc />
		public string Serialize(object value, TSettings settings) { return Serialize(value, value.GetType(), null); }

		/// <inheritdoc />
		public abstract string Serialize(object value, Type type, TSettings settings);

		/// <inheritdoc />
		public T Deserialize<T>(string value, TSettings settings) { return (T)Deserialize(value, typeof(T), settings); }

		/// <inheritdoc />
		public override object Deserialize(string value, Type type) { return Deserialize(value, type, null); }

		/// <inheritdoc />
		public abstract object Deserialize(string value, Type type, TSettings settings);

		/// <inheritdoc />
		public async ValueTask<T> DeserializeAsync<T>(Stream stream, TSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return (T)await DeserializeAsync(stream, typeof(T), settings, token);
		}

		/// <inheritdoc />
		public override ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return DeserializeAsync(stream, type, null, token);
		}

		/// <inheritdoc />
		public abstract ValueTask<object> DeserializeAsync(Stream stream, Type type, TSettings settings, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public override void Populate(string value, object target) { Populate(value, target, null); }

		/// <inheritdoc />
		public abstract void Populate(string value, object target, TSettings settings);
	}
}

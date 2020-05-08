using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace asm.Json.Abstraction
{
	public interface IJsonSerializer
	{
		string Serialize([NotNull] object value);
		string Serialize([NotNull] object value, [NotNull] Type type);
		T Deserialize<T>([NotNull] string value);
		object Deserialize([NotNull] string value, [NotNull] Type type);
		ValueTask<T> DeserializeAsync<T>([NotNull] Stream stream, CancellationToken token);
		ValueTask<object> DeserializeAsync([NotNull] Stream stream, [NotNull] Type type, CancellationToken token);
		void Populate([NotNull] string value, [NotNull] object target);
		void Populate([NotNull] string value, [NotNull] IDictionary<string, string> dictionary);
		string ToJson([NotNull] XmlNode node, bool omitRootObject);
		string ToJson([NotNull] XNode node, bool omitRootObject);
	}

	public interface IJsonSerializer<TSettings> : IJsonSerializer
		where TSettings : class, new()
	{
		[NotNull]
		Func<TSettings> DefaultSettings { get; set; }
		string Serialize([NotNull] object value, TSettings settings);
		string Serialize([NotNull] object value, [NotNull] Type type, TSettings settings);
		T Deserialize<T>([NotNull] string value, TSettings settings);
		object Deserialize([NotNull] string value, [NotNull] Type type, TSettings settings);
		ValueTask<T> DeserializeAsync<T>([NotNull] Stream stream, TSettings settings, CancellationToken token);
		ValueTask<object> DeserializeAsync([NotNull] Stream stream, [NotNull] Type type, TSettings settings, CancellationToken token);
		void Populate([NotNull] string value, [NotNull] object target, TSettings settings);
	}
}
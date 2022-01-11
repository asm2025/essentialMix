using JetBrains.Annotations;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class JsonSerializerExtension
{
	[NotNull]
	public static T Apply<T>([NotNull] this T thisValue, [NotNull] JsonSerializerSettings settings)
		where T : JsonSerializer
	{
		thisValue.ObjectCreationHandling = settings.ObjectCreationHandling;
		thisValue.Context = settings.Context;
		thisValue.EqualityComparer = settings.EqualityComparer;
		thisValue.CheckAdditionalContent = settings.CheckAdditionalContent;
		thisValue.NullValueHandling = settings.NullValueHandling;
		thisValue.PreserveReferencesHandling = settings.PreserveReferencesHandling;
		thisValue.ReferenceLoopHandling = settings.ReferenceLoopHandling;
		thisValue.DateFormatString = settings.DateFormatString;
		thisValue.Formatting = settings.Formatting;
		if (settings.ContractResolver != null) thisValue.ContractResolver = settings.ContractResolver;
		thisValue.Culture = settings.Culture;
		if (settings.Converters.Count == 0) return thisValue;

		JsonConverterCollection converters = thisValue.Converters;

		lock (converters)
		{
			foreach (JsonConverter converter in settings.Converters)
			{
				if (converters.Contains(converter)) continue;
				converters.Add(converter);
			}
		}

		return thisValue;
	}
}
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class KeyValuePairExtension
	{
		[NotNull]
		public static string Format<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue, [NotNull] string format, char separator = '=')
		{
			return !HasKeyAndValue(thisValue)
						? string.Empty
						: $"{thisValue.Key}{separator}{FormatValue(format, thisValue.Value)}";
		}

		[NotNull]
		public static string Format<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue, [NotNull] string format, string separator)
		{
			return !HasKeyAndValue(thisValue)
						? string.Empty
						: string.Concat(thisValue.Key, separator, FormatValue(format, thisValue.Value));
		}

		[NotNull]
		public static string ToString<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue, char separator)
		{
			return string.Concat(thisValue.Key, separator, (object)thisValue.Value ?? string.Empty);
		}

		[NotNull]
		public static string ToString<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue, [NotNull] string separator)
		{
			return string.Concat(thisValue.Key, separator, (object)thisValue.Value ?? string.Empty);
		}

		public static bool HasKey<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue) { return thisValue.Key != null; }

		public static bool HasValue<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue) { return thisValue.Value != null; }

		public static bool HasKeyAndValue<TKey, TValue>(this KeyValuePair<TKey, TValue> thisValue) { return thisValue.Key != null && thisValue.Value != null; }

		[NotNull]
		public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue)
		{
			IDictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
			return ToDictionary(thisValue, dictionary);
		}

		[NotNull]
		public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue, [NotNull] IDictionary<TKey, TValue> dictionary)
		{
			foreach (KeyValuePair<TKey, TValue> pair in thisValue) 
				dictionary.Add(pair);

			return dictionary;
		}

		[NotNull]
		private static string FormatValue<TValue>(string format, TValue value)
		{
			if (string.IsNullOrEmpty(format) || value.IsNull()) return string.Empty;

			if (!(value is IEnumerable enumerable)) return string.Format(format, value);

			StringBuilder sb = new StringBuilder();

			foreach (object item in enumerable)
			{
				if (item.IsNull()) continue;
				sb.Separator(',');
				sb.AppendFormat(format, item);
			}

			return sb.ToString();
		}
	}
}
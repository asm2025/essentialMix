using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class IReadOnlyDictionaryExtension
	{
		[NotNull]
		public static DictionaryEnumerator<TKey, TValue> Enumerate<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue)
		{
			return new DictionaryEnumerator<TKey, TValue>(thisValue);
		}

		public static void CloneTo<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, [NotNull] IDictionary<TKey, TValue> destination)
		{
			if (thisValue.Count == 0) return;

			bool keyClonable = typeof(TKey) is ICloneable;
			bool valueClonable = typeof(TValue) is ICloneable;

			if (keyClonable && valueClonable)
			{
				foreach (KeyValuePair<TKey, TValue> pair in thisValue)
					destination.Add((TKey)((ICloneable)pair.Key).Clone(), (TValue)((ICloneable)pair.Value)?.Clone());
			}
			else if (keyClonable)
			{
				foreach (KeyValuePair<TKey, TValue> pair in thisValue)
					destination.Add((TKey)((ICloneable)pair.Key).Clone(), pair.Value);
			}
			else if (valueClonable)
			{
				foreach (KeyValuePair<TKey, TValue> pair in thisValue)
					destination.Add(pair.Key, (TValue)((ICloneable)pair.Value)?.Clone());
			}
			else
			{
				thisValue.CopyTo(destination);
			}
		}

		[NotNull]
		public static string ToString<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, char group) { return ToString(thisValue, "=", group.ToString()); }

		[NotNull]
		public static string ToString<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, char separator, char group)
		{
			return ToString(thisValue, separator.ToString(), group.ToString());
		}

		[NotNull]
		public static string ToString<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, [NotNull] string group) { return ToString(thisValue, "=", group); }

		[NotNull]
		public static string ToString<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, [NotNull] string separator, [NotNull] string group)
		{
			if (separator.Length == 0) separator = "=";
			return group.Length == 0 ? string.Concat(thisValue.Select(p => p.ToString(separator))) : string.Join(group, thisValue.Select(p => p.ToString(separator)));
		}

		public static TValue Get<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, [NotNull] TKey key, TValue defaultValue = default(TValue))
		{
			return thisValue.TryGetValue(key, out TValue value)
						? value
						: defaultValue;
		}

		public static TKey GetKeyByIndex<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, int index) { return GetKeyByIndex(thisValue, index, default); }

		public static TKey GetKeyByIndex<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, int index, TKey defaultKey)
		{
			if (!index.InRangeRx(0, thisValue.Count)) throw new ArgumentOutOfRangeException(nameof(index));
			return index == 0 ? thisValue.Keys.First() : thisValue.Keys.Skip(index + 1).First();
		}

		public static TValue GetValueByIndex<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, int index)
		{
			return GetValueByIndex(thisValue, index, default(TValue));
		}

		public static TValue GetValueByIndex<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, int index, TValue defaultValue)
		{
			TKey key = GetKeyByIndex(thisValue, index);
			return thisValue.ContainsKey(key) ? thisValue[key] : defaultValue;
		}

		[ItemNotNull]
		public static IEnumerable<IDictionary<TKey, TValue>> Partition<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, int size, PartitionSize type = PartitionSize.PerPartition)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0 || thisValue.Count == 0) yield break;

			int sz;

			switch (type)
			{
				case PartitionSize.TotalCount:
					sz = (int)Math.Ceiling(thisValue.Count / (double)size);
					break;
				default:
					sz = size;
					break;
			}

			int n = 0;
			IDictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(sz);

			foreach (KeyValuePair<TKey, TValue> item in thisValue)
			{
				dictionary.Add(item);
				n++;

				if (n == thisValue.Count || dictionary.Count == sz)
				{
					yield return dictionary;
					dictionary.Clear();
				}
			}
		}

		public static IEnumerable<TValue> Match<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, [NotNull] TKey key, [NotNull] IEqualityComparer<TKey> comparer)
		{
			if (thisValue.Count == 0) yield break;

			foreach (TKey k in thisValue.Keys.Where(k => comparer.Equals(k, key)))
				yield return thisValue[k];
		}

		public static TValue GetOrDefault<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> thisValue, [NotNull] TKey key, TValue defaultValue = default(TValue))
		{
			return thisValue.TryGetValue(key, out TValue result)
						? result
						: defaultValue;
		}
	}
}

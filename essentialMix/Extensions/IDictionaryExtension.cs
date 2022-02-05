using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using essentialMix.Collections;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class IDictionaryExtension
{
	[NotNull]
	public static DictionaryEnumerator Enumerate([NotNull] this IDictionary thisValue) { return new DictionaryEnumerator(thisValue); }

	[NotNull]
	public static DictionaryEnumerator<TKey, TValue> Enumerate<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue)
	{
		return new DictionaryEnumerator<TKey, TValue>(thisValue);
	}

	[NotNull]
	public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue)
	{
		return new ReadOnlyDictionary<TKey, TValue>(thisValue);
	}

	public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] TKey key)
		where TValue : new()
	{
		lock(thisValue)
		{
			if (thisValue.TryGetValue(key, out TValue value)) return value;
			value = new TValue();
			thisValue[key] = value;
			return value;
		}
	}

	public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] TKey key, TValue missingValue)
		where TValue : struct, IComparable
	{
		lock(thisValue)
		{
			if (thisValue.TryGetValue(key, out TValue value)) return value;
			thisValue[key] = missingValue;
			return missingValue;
		}
	}

	public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] TKey key, [NotNull] Func<TKey, TValue> onNotFound)
	{
		lock(thisValue)
		{
			if (thisValue.TryGetValue(key, out TValue value)) return value;
			value = onNotFound(key);
			thisValue[key] = value;
			return value;
		}
	}

	public static bool TryAdd([NotNull] this IDictionary thisValue, DictionaryEntry entry) { return TryAdd(thisValue, entry.Key, entry.Value); }

	public static bool TryAdd([NotNull] this IDictionary thisValue, [NotNull] object key, object value)
	{
		lock(thisValue.SyncRoot)
		{
			if (thisValue.Contains(key)) return false;
			thisValue.Add(key, value);
			return true;
		}
	}

	public static bool TryAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, KeyValuePair<TKey, TValue> pair)
	{
		return TryAdd(thisValue, pair.Key, pair.Value);
	}

	public static bool TryAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, TKey key, TValue value)
	{
		if (key == null) return false;

		lock(thisValue)
		{
			if (thisValue.ContainsKey(key)) return false;
			thisValue.Add(key, value);
			return true;
		}
	}

	public static int Add([NotNull] this IDictionary thisValue, [NotNull] params DictionaryEntry[] entry)
	{
		if (entry.Length == 0) return 0;

		lock(thisValue.SyncRoot)
		{
			int n = 0;

			foreach (DictionaryEntry e in entry)
			{
				thisValue.Add(e.Key, e.Value);
				n++;
			}

			return n;
		}
	}

	public static int Add<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] params KeyValuePair<TKey, TValue>[] pair)
	{
		if (pair.Length == 0) return 0;

		lock(thisValue)
		{
			foreach (KeyValuePair<TKey, TValue> p in pair)
				thisValue.Add(p);

			return pair.Length;
		}
	}

	public static int AddRange([NotNull] this IDictionary thisValue, [NotNull] IEnumerable<DictionaryEntry> source, bool skipExisting = false)
	{
		lock(thisValue.SyncRoot)
		{
			int added = 0;

			if (skipExisting)
			{
				foreach (DictionaryEntry entry in source)
				{
					thisValue.Add(entry.Key, entry.Value);
					added++;
				}
			}
			else
			{
				foreach (DictionaryEntry entry in source)
				{
					thisValue[entry.Key] = entry.Value;
					added++;
				}
			}

			return added;
		}
	}

	public static int AddRange<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> source, bool skipExisting = false)
	{
		int added = 0;

		if (skipExisting)
		{
			foreach (KeyValuePair<TKey, TValue> pair in source)
			{
				thisValue.Add(pair.Key, pair.Value);
				added++;
			}
		}
		else
		{
			foreach (KeyValuePair<TKey, TValue> pair in source)
			{
				thisValue[pair.Key] = pair.Value;
				added++;
			}
		}

		return added;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static void AddSkipNull<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, TKey key, TValue value)
		where TValue : class
	{
		if (value.IsNull()) return;
		thisValue.Add(key, value);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static void AddSkipNullOrEmpty<TKey>([NotNull] this IDictionary<TKey, string> thisValue, TKey key, string value)
	{
		if (string.IsNullOrEmpty(value)) return;
		thisValue.Add(key, value);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static void AddSkipNullOrWhiteSpace<TKey>([NotNull] this IDictionary<TKey, string> thisValue, TKey key, string value)
	{
		if (string.IsNullOrWhiteSpace(value)) return;
		thisValue.Add(key, value);
	}

	public static void Initialize([NotNull] this IDictionary thisValue, object value = null)
	{
		lock(thisValue.SyncRoot)
		{
			foreach (object key in thisValue.Keys)
				thisValue[key] = value;
		}
	}

	public static void Initialize<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, TValue value = default(TValue))
	{
		foreach (TKey key in thisValue.Keys)
			thisValue[key] = value;
	}

	[NotNull]
	public static string ToString<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, char group) { return ToString(thisValue, "=", group.ToString()); }

	[NotNull]
	public static string ToString<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, char separator, char group)
	{
		return ToString(thisValue, separator.ToString(), group.ToString());
	}

	[NotNull]
	public static string ToString<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] string group) { return ToString(thisValue, "=", group); }

	[NotNull]
	public static string ToString<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] string separator, [NotNull] string group)
	{
		if (separator.Length == 0) separator = "=";
		return group.Length == 0 ? string.Concat(thisValue.Select(p => p.ToString(separator))) : string.Join(group, thisValue.Select(p => p.ToString(separator)));
	}

	public static TValue Get<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] TKey key, TValue defaultValue = default(TValue))
	{
		return thisValue.TryGetValue(key, out TValue value)
					? value
					: defaultValue;
	}

	public static TKey GetKeyByIndex<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, int index) { return GetKeyByIndex(thisValue, index, default(TKey)); }

	public static TKey GetKeyByIndex<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, int index, TKey defaultKey)
	{
		if (!index.InRangeRx(0, thisValue.Count)) throw new ArgumentOutOfRangeException(nameof(index));
		return index == 0 ? thisValue.Keys.First() : thisValue.Keys.Skip(index + 1).First();
	}

	public static TValue GetValueByIndex<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, int index)
	{
		return GetValueByIndex(thisValue, index, default(TValue));
	}

	public static TValue GetValueByIndex<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, int index, TValue defaultValue)
	{
		TKey key = GetKeyByIndex(thisValue, index);
		return thisValue.ContainsKey(key) ? thisValue[key] : defaultValue;
	}

	[ItemNotNull]
	public static IEnumerable<IDictionary<TKey, TValue>> Partition<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, int size, PartitionSize type = PartitionSize.PerPartition)
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

	public static IEnumerable<TValue> Match<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] TKey key, [NotNull] IEqualityComparer<TKey> comparer)
	{
		if (thisValue.Count == 0) yield break;

		foreach (TKey k in thisValue.Keys.Where(k => comparer.Equals(k, key)))
			yield return thisValue[k];
	}

	public static TValue GetOrDefault<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] TKey key, TValue defaultValue = default(TValue))
	{
		return thisValue.TryGetValue(key, out TValue result)
					? result
					: defaultValue;
	}

	public static void Merge<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] IDictionary<TKey, TValue> other)
	{
		if (other.Count == 0) return;

		foreach (KeyValuePair<TKey, TValue> pair in other) 
			thisValue[pair.Key] = pair.Value;
	}

	public static void Merge<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] IDictionary<TKey, TValue> other1, [NotNull] IDictionary<TKey, TValue> other2)
	{
		if (other1.Count == 0) return;

		foreach (KeyValuePair<TKey, TValue> pair in other1) 
			thisValue[pair.Key] = pair.Value;

		foreach (KeyValuePair<TKey, TValue> pair in other2) 
			thisValue[pair.Key] = pair.Value;
	}

	public static void Merge<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] IDictionary<TKey, TValue> other1, [NotNull] IDictionary<TKey, TValue> other2, [NotNull] IDictionary<TKey, TValue> other3)
	{
		if (other1.Count == 0) return;

		foreach (KeyValuePair<TKey, TValue> pair in other1) 
			thisValue[pair.Key] = pair.Value;

		foreach (KeyValuePair<TKey, TValue> pair in other2) 
			thisValue[pair.Key] = pair.Value;

		foreach (KeyValuePair<TKey, TValue> pair in other3) 
			thisValue[pair.Key] = pair.Value;
	}

	public static void Merge<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> thisValue, [NotNull] params IDictionary<TKey, TValue>[] dictionaries)
	{
		if (dictionaries.Length == 0) return;

		foreach (IDictionary<TKey, TValue> dictionary in dictionaries)
		{
			if (dictionary == null || dictionary.Count == 0) continue;

			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				thisValue[pair.Key] = pair.Value;
			}
		}
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool TryGetValue<TKey, TValue>(this IDictionary<TKey, object> thisValue, TKey key, out TValue value) { return TryGetValue(thisValue, key, out value, default(TValue)); }
	public static bool TryGetValue<TKey, TValue>(this IDictionary<TKey, object> thisValue, TKey key, out TValue value, TValue defaultValue)
	{
		if (thisValue != null && thisValue.TryGetValue(key, out object obj))
		{
			value = obj.To(defaultValue);
			return true;
		}

		value = defaultValue;
		return false;
	}
}
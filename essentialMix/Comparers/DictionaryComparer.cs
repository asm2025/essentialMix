using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Comparers;

public class DictionaryComparer<TKey, TValue>(IKeyValueComparer<TKey, TValue> comparer)
	: IDictionaryComparer<TKey, TValue>
{
	public static DictionaryComparer<TKey, TValue> Default { get; } = new DictionaryComparer<TKey, TValue>();

	public DictionaryComparer()
		: this(null)
	{
	}

	public DictionaryComparer(IGenericComparer<TKey> keyComparer, IGenericComparer<TValue> valueComparer)
		: this(new KeyValueComparer<TKey, TValue>(keyComparer, valueComparer))
	{
	}

	[NotNull]
	public IKeyValueComparer<TKey, TValue> Comparer { get; } = comparer ?? KeyValueComparer<TKey, TValue>.Default;

	public int Compare(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		if (x.Count != y.Count) return y.Count - x.Count;

		int compare = 0;
		IEnumerable<TKey> keys = x.Keys.Union(y.Keys, Comparer.KeyComparer);

		foreach (TKey key in keys)
		{
			if (!x.TryGetValue(key, out TValue xv)) compare++;
			if (!y.TryGetValue(key, out TValue yv)) compare--;
			compare += Comparer.ValueComparer.Compare(xv, yv);
		}

		return compare;
	}

	public bool Equals(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y) { return Equals(x, y, EqualityComparer<TValue>.Default, null); }

	public bool Equals(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y, [NotNull] params TKey[] keysToExclude)
	{
		return Equals(x, y, EqualityComparer<TValue>.Default, keysToExclude);
	}

	public bool Equals(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y, IEqualityComparer<TValue> equalityComparer, params TKey[] keysToExclude)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		if (x.Count != y.Count) return false;
		keysToExclude ??= [];

		IEnumerable<TKey> keys = x.Keys.Union(y.Keys, Comparer.KeyComparer);
			
		foreach (TKey key in keys.Where(e => !keysToExclude.Contains(e)))
		{
			if (!x.TryGetValue(key, out TValue xv)) return false;
			if (!y.TryGetValue(key, out TValue yv)) return false;
			if (!Comparer.ValueComparer.Equals(xv, yv)) return false;
		}

		return true;
	}

	public int GetHashCode(IDictionary<TKey, TValue> obj) { return obj.GetHashCode(); }

	public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

	public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

	public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
}
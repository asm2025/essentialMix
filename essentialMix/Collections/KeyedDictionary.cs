using System;
using System.Collections.Generic;
using Other.Microsoft.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
public class KeyedDictionary<TKey, TValue> : KeyedDictionaryBase<TKey, TValue>
{
	public KeyedDictionary([NotNull] Func<TValue, TKey> getKey)
		: this(getKey, (IEqualityComparer<TKey>)null)
	{
	}

	public KeyedDictionary([NotNull] Func<TValue, TKey> getKey, IEqualityComparer<TKey> comparer)
		: base(comparer ?? EqualityComparer<TKey>.Default)
	{
		GetKey = getKey;
	}

	public KeyedDictionary([NotNull] Func<TValue, TKey> getKey, [NotNull] IEnumerable<TValue> collection)
		: this(getKey, collection, null)
	{
	}

	public KeyedDictionary([NotNull] Func<TValue, TKey> getKey, [NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer)
		: base(comparer ?? EqualityComparer<TKey>.Default)
	{
		GetKey = getKey;
		
		foreach (TValue value in collection) 
			Add(value);
	}

	[NotNull]
	public Func<TValue, TKey> GetKey { get; }

	/// <inheritdoc />
	protected sealed override TKey GetKeyForItem(TValue item) { return GetKey(item); }
}
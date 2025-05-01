using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
public abstract class ReadOnlyKeyedCollectionBase<TKey, TValue>([NotNull] KeyedCollectionBase<TKey, TValue> collection)
	: IReadOnlyKeyedCollection<TKey, TValue>, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>
{
	[NotNull]
	protected KeyedCollectionBase<TKey, TValue> Collection { get; } = collection;

	/// <inheritdoc />
	public IEnumerator<TValue> GetEnumerator() { return Collection.GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() { return ((IReadOnlyKeyedCollection<TKey, TValue>)Collection).GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Collection.Count;

	/// <inheritdoc />
	public bool ContainsKey([NotNull] TKey key) { return Collection.ContainsKey(key); }

	/// <inheritdoc />
	public bool TryGetValue(TKey key, out TValue value) { return Collection.TryGetValue(key, out value); }

	/// <inheritdoc />
	public TValue this[[NotNull] TKey key] => Collection[key];

	/// <inheritdoc />
	public IEnumerable<TKey> Keys => Collection.Keys;

	/// <inheritdoc />
	public IEnumerable<TValue> Values => Collection.Values;

	/// <inheritdoc />
	public IEqualityComparer<TKey> Comparer => Collection.Comparer;

	/// <inheritdoc />
	public int IndexOfKey(TKey key) { return Collection.IndexOfKey(key); }

	/// <inheritdoc />
	public bool Contains([NotNull] TKey key) { return Collection.Contains(key); }

	/// <inheritdoc />
	int IReadOnlyCollection<TValue>.Count => Collection.Count;

	/// <inheritdoc />
	public TValue this[int index] => Collection[index];
}
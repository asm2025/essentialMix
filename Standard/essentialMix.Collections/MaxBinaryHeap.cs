using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
public class MaxBinaryHeap<TKey, TValue> : BinaryHeap<TKey, TValue>
{
	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem)
		: base(getKeyForItem)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
		: base(getKeyForItem, capacity)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer)
		: base(getKeyForItem, keyComparer)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
		: base(getKeyForItem, capacity, keyComparer, comparer)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
		: base(getKeyForItem, enumerable)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
		: base(getKeyForItem, enumerable, keyComparer, comparer)
	{
	}

	/// <inheritdoc />
	protected sealed override int Compare(TValue x, TValue y) { return Comparer.Compare(x, y) * -1; }

	/// <inheritdoc />
	protected sealed override int KeyCompare(TKey x, TKey y) { return KeyComparer.Compare(x, y) * -1; }
}

[Serializable]
public class MaxBinaryHeap<T> : BinaryHeap<T>
{
	/// <inheritdoc />
	public MaxBinaryHeap() 
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap(int capacity)
		: base(capacity)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap(IComparer<T> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap(int capacity, IComparer<T> comparer)
		: base(capacity, comparer)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] IEnumerable<T> enumerable)
		: base(enumerable)
	{
	}

	/// <inheritdoc />
	public MaxBinaryHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
		: base(enumerable, comparer)
	{
	}

	/// <inheritdoc />
	protected override int Compare(T x, T y) { return Comparer.Compare(x, y) * -1; }
}
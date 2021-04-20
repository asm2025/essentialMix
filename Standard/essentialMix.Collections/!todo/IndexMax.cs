using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class IndexMax<TKey, TValue> : IndexHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public IndexMax([NotNull] Func<TValue, TKey> getKeyForItem)
			: base(getKeyForItem)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
			: base(getKeyForItem, capacity)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(getKeyForItem, comparer)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> comparer)
			: base(getKeyForItem, capacity, comparer)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: base(getKeyForItem, enumerable)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(getKeyForItem, enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(KeyedBinaryNode<TKey, TValue> x, KeyedBinaryNode<TKey, TValue> y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			return Compare(x.Key, y.Key);
		}

		/// <inheritdoc />
		protected sealed override int Compare(TKey x, TKey y)
		{
			return Comparer.Compare(x, y) * -1;
		}
	}

	[Serializable]
	public class IndexMax<T> : IndexHeap<T>
	{
		/// <inheritdoc />
		public IndexMax() 
		{
		}

		/// <inheritdoc />
		public IndexMax(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public IndexMax(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public IndexMax(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public IndexMax([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(KeyedBinaryNode<T> x, KeyedBinaryNode<T> y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			return Compare(x.Key, y.Key);
		}

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y) * -1;
		}
	}
}

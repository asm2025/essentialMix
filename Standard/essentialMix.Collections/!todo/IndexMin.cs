using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class IndexMin<TKey, TValue> : IndexHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public IndexMin([NotNull] Func<TValue, TKey> getKeyForItem)
			: base(getKeyForItem)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
			: base(getKeyForItem, capacity)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(getKeyForItem, comparer)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> comparer)
			: base(getKeyForItem, capacity, comparer)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: base(getKeyForItem, enumerable)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
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
			return Comparer.Compare(x, y);
		}
	}

	[Serializable]
	public class IndexMin<T> : IndexHeap<T>
	{
		/// <inheritdoc />
		public IndexMin() 
		{
		}

		/// <inheritdoc />
		public IndexMin(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public IndexMin(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public IndexMin(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public IndexMin([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
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
			return Comparer.Compare(x, y);
		}
	}
}
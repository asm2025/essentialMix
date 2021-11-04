using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class MinBinaryHeap<TKey, TValue> : BinaryHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public MinBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: base(getKeyForItem)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
			: base(getKeyForItem, capacity)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer)
			: base(getKeyForItem, keyComparer)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, capacity, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: base(getKeyForItem, enumerable)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, enumerable, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(TValue x, TValue y) { return Comparer.Compare(x, y); }

		/// <inheritdoc />
		protected sealed override int KeyCompare(TKey x, TKey y) { return KeyComparer.Compare(x, y); }
	}

	[Serializable]
	public class MinBinaryHeap<T> : BinaryHeap<T>
	{
		/// <inheritdoc />
		public MinBinaryHeap()
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y) { return Comparer.Compare(x, y); }
	}
}
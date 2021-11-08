using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class MinBinomialHeap<TKey, TValue> : BinomialHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public MinBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: base(getKeyForItem)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: base(getKeyForItem, enumerable)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, enumerable, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(TKey x, TKey y)
		{
			return KeyComparer.Compare(x, y);
		}

		/// <inheritdoc />
		protected sealed override int Compare(TValue x, TValue y)
		{
			return Comparer.Compare(x, y);
		}
	}

	[Serializable]
	public class MinBinomialHeap<T> : BinomialHeap<T>
	{
		/// <inheritdoc />
		public MinBinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y);
		}
	}
}
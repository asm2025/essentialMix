using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class MaxPairingHeap<TKey, TValue> : PairingHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public MaxPairingHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: base(getKeyForItem)
		{
		}

		/// <inheritdoc />
		public MaxPairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		public MaxPairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: base(getKeyForItem, enumerable)
		{
		}

		/// <inheritdoc />
		public MaxPairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, enumerable, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(TKey x, TKey y)
		{
			return KeyComparer.Compare(x, y) * - 1;
		}

		/// <inheritdoc />
		protected sealed override int Compare(TValue x, TValue y)
		{
			return Comparer.Compare(x, y) * - 1;
		}
	}

	[Serializable]
	public class MaxPairingHeap<T> : PairingHeap<T>
	{
		/// <inheritdoc />
		public MaxPairingHeap()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MaxPairingHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MaxPairingHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		public MaxPairingHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y) * - 1;
		}
	}
}
﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MaxBinomialHeap<TKey, TValue> : BinomialHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, (IComparer<TKey>)null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(getKeyForItem, comparer)
		{
		}

		/// <inheritdoc />
		internal MaxBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] BinomialNode<TKey, TValue> head)
			: this(getKeyForItem, head, null)
		{
		}

		/// <inheritdoc />
		internal MaxBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] BinomialNode<TKey, TValue> head, IComparer<TKey> comparer)
			: base(getKeyForItem, head, comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(getKeyForItem, enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override int Compare(TKey x, TKey y)
		{
			return Comparer.Compare(x, y) * - 1;
		}
	}

	[Serializable]
	public class MaxBinomialHeap<T> : BinomialHeap<T>
	{
		/// <inheritdoc />
		public MaxBinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y) * - 1;
		}
	}
}
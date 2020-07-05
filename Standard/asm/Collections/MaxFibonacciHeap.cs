using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MaxFibonacciHeap<TKey, TValue> : FibonacciHeap<TKey, TValue>
	{
		/// <inheritdoc />
		public MaxFibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, (IComparer<TKey>)null)
		{
		}

		/// <inheritdoc />
		public MaxFibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(getKeyForItem, comparer)
		{
		}

		/// <inheritdoc />
		protected internal MaxFibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] FibonacciNode<TKey, TValue> head)
			: this(getKeyForItem, head, null)
		{
		}

		/// <inheritdoc />
		protected internal MaxFibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] FibonacciNode<TKey, TValue> head, IComparer<TKey> comparer)
			: base(getKeyForItem, head, comparer)
		{
		}

		/// <inheritdoc />
		public MaxFibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		/// <inheritdoc />
		public MaxFibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(getKeyForItem, enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override int Compare(TKey x, TKey y)
		{
			return Comparer.Compare(x, y) * -1;
		}
	}

	[Serializable]
	public class MaxFibonacciHeap<T> : FibonacciHeap<T>
	{
		/// <inheritdoc />
		public MaxFibonacciHeap()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MaxFibonacciHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected internal MaxFibonacciHeap([NotNull] FibonacciNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		protected internal MaxFibonacciHeap([NotNull] FibonacciNode<T> head, IComparer<T> comparer)
			: base(head, comparer)
		{
		}

		/// <inheritdoc />
		public MaxFibonacciHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		public MaxFibonacciHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y) * -1;
		}
	}
}
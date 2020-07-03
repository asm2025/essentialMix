using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	// IMPORTANT: this is just a proof of concept and not for production. the count property will need more work todo in the base class
	public class MaxBinomialPriorityQueue<T> : BinomialPriorityQueue<T>
	{
		/// <inheritdoc />
		public MaxBinomialPriorityQueue()
			: this((IComparer<T>)null, null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialPriorityQueue(IComparer<T> priorityComparer)
			: this((IComparer<T>)null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialPriorityQueue(IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		internal MaxBinomialPriorityQueue([NotNull] BinomialNode<T> head, IComparer<T> priorityComparer)
			: this(head, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		internal MaxBinomialPriorityQueue([NotNull] BinomialNode<T> head, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(head, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialPriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> priorityComparer)
			: base(enumerable, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialPriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(enumerable, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override BinomialHeap<T> MakeHeap(BinomialNode<T> head) { return new MaxBinomialPriorityQueue<T>(head, Comparer, PriorityComparer); }

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			int cmp = PriorityComparer.Compare(x, y);
			return (cmp != 0 ? cmp : Comparer.Compare(x, y)) * - 1;
		}
	}
}
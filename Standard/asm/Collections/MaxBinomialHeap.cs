using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
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
		internal MaxBinomialHeap([NotNull] BinomialNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		internal MaxBinomialHeap([NotNull] BinomialNode<T> head, IComparer<T> comparer)
			: base(head, comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override BinomialHeap<T> MakeHeap(BinomialNode<T> head) { return new MaxBinomialHeap<T>(head, Comparer); }

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y) * -1;
		}
	}
}
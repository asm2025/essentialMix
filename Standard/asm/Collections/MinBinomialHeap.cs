using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
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
		internal MinBinomialHeap([NotNull] BinomialNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		internal MinBinomialHeap([NotNull] BinomialNode<T> head, IComparer<T> comparer)
			: base(head, comparer)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override BinomialHeap<T> MakeHeap(BinomialNode<T> head) { return new MinBinomialHeap<T>(head, Comparer); }

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			return Comparer.Compare(x, y);
		}
	}
}
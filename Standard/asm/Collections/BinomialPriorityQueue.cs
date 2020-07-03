using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	// IMPORTANT: this is just a proof of concept and not for production. the count property will need more work todo in the base class
	public abstract class BinomialPriorityQueue<T> : BinomialHeap<T>
	{
		/// <inheritdoc />
		protected BinomialPriorityQueue()
			: this((IComparer<T>)null, null)
		{
		}

		/// <inheritdoc />
		protected BinomialPriorityQueue(IComparer<T> priorityComparer)
			: this((IComparer<T>)null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected BinomialPriorityQueue(IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(comparer)
		{
			PriorityComparer = priorityComparer ?? Comparer;
		}

		/// <inheritdoc />
		internal BinomialPriorityQueue([NotNull] BinomialNode<T> head, IComparer<T> priorityComparer)
			: this(head, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		internal BinomialPriorityQueue([NotNull] BinomialNode<T> head, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(head, comparer)
		{
			PriorityComparer = priorityComparer ?? Comparer;
		}

		/// <inheritdoc />
		protected BinomialPriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> priorityComparer)
			: this(enumerable, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected BinomialPriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(comparer, priorityComparer)
		{
			Add(enumerable);
		}

		[NotNull]
		public IComparer<T> PriorityComparer { get; }
	}
}

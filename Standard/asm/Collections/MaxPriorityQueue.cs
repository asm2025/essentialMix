using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MaxPriorityQueue<T> : PriorityQueue<T>
	{
		/// <inheritdoc />
		public MaxPriorityQueue()
			: this(0, null, null)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity)
			: this(capacity, null, null)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(IComparer<T> priorityComparer)
			: this(0, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity, IComparer<T> priorityComparer)
			: this(capacity, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(0, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(capacity, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> priorityComparer)
			: this(enumerable, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(enumerable, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected sealed override int Compare(T x, T y)
		{
			int cmp = PriorityComparer.Compare(x, y);
			return (cmp != 0 ? cmp : Comparer.Compare(x, y)) * -1;
		}
	}
}
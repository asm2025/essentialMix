using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public abstract class PriorityQueue<T> : Heap<T>
	{
		/// <inheritdoc />
		protected PriorityQueue()
			: this(0, null, null)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity)
			: this(capacity, null, null)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(IComparer<T> priorityComparer)
			: this(0, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity, IComparer<T> priorityComparer)
			: this(capacity, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(0, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(capacity, comparer)
		{
			PriorityComparer = priorityComparer ?? Comparer;
		}

		/// <inheritdoc />
		protected PriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> priorityComparer)
			: this(enumerable, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(0, comparer, priorityComparer)
		{
			Add(enumerable);
		}

		[NotNull]
		public IComparer<T> PriorityComparer { get; private set; }
	}
}
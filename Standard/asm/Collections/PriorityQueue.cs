using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public abstract class PriorityQueue<TPriority, T> : Heap<T>
		where TPriority : struct, IComparable
	{
		/// <inheritdoc />
		protected PriorityQueue([NotNull] Func<T, TPriority> priority)
			: this(0, null, priority)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity, [NotNull] Func<T, TPriority> priority)
			: this(capacity, null, priority)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: base(capacity, comparer)
		{
			GetPriority = priority;
		}

		/// <inheritdoc />
		protected PriorityQueue([NotNull] IEnumerable<T> collection, [NotNull] Func<T, TPriority> priority)
			: this(collection, null, priority)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
			Add(collection);
		}

		[NotNull]
		protected Func<T, TPriority> GetPriority { get; }
	}
}
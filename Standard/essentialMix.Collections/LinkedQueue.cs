using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class LinkedQueue<T> : LinkedList<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	{
		private readonly Action<T> _enqueue;

		public LinkedQueue(DequeuePriority priority)
		{
			Priority = priority;

			_enqueue = priority switch
			{
				DequeuePriority.FIFO => e => AddFirst(e),
				DequeuePriority.LIFO => e => AddLast(e),
				_ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
			};
		}

		/// <inheritdoc />
		public LinkedQueue(DequeuePriority priority, [NotNull] IEnumerable<T> enumerable)
			: this(priority)
		{
			foreach (T item in enumerable)
			{
				Enqueue(item);
			}
		}

		public DequeuePriority Priority { get; }

		public void Enqueue(T item) { _enqueue(item); }

		public T Dequeue()
		{
			AssertNotEmpty();
			T value = Last.Value;
			RemoveLast();
			return value;
		}

		public T Peek()
		{
			AssertNotEmpty();
			return Last.Value;
		}

		private void AssertNotEmpty()
		{
			if (Count > 0) return;
			throw new InvalidOperationException("Queue is empty.");
		}
	}
}
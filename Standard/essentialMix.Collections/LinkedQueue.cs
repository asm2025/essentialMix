using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class LinkedQueue<T> : LinkedList<T>, IQueue<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	{
		private readonly Func<T> _dequeue;

		public LinkedQueue(DequeuePriority priority)
		{
			Priority = priority;
			_dequeue = priority switch
			{
				DequeuePriority.FIFO => () =>
				{
					AssertNotEmpty();
					T value = First.Value;
					RemoveFirst();
					return value;
				},
				DequeuePriority.LIFO => () =>
				{
					AssertNotEmpty();
					T value = Last.Value;
					RemoveLast();
					return value;
				},
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

		public void Enqueue(T item) { AddLast(item); }

		public T Dequeue() { return _dequeue(); }

		public T Peek()
		{
			AssertNotEmpty();
			LinkedListNode<T> node = Priority == DequeuePriority.LIFO
										? Last
										: First;
			return node.Value;
		}

		public T PeekTail()
		{
			AssertNotEmpty();
			LinkedListNode<T> node = Priority == DequeuePriority.LIFO
										? First
										: Last;
			return node.Value;
		}

		private void AssertNotEmpty()
		{
			if (Count > 0) return;
			throw new InvalidOperationException("Queue is empty.");
		}
	}
}
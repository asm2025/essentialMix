using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class LinkedQueue<T> : LinkedList<T>, IDeque<T>, IReadOnlyCollection<T>, ICollection, IEnumerable<T>, IEnumerable
	{
		public LinkedQueue(DequeuePriority priority)
		{
			Priority = priority;
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

		public T Dequeue()
		{
			AssertNotEmpty();
			
			T value;

			if (Priority == DequeuePriority.LIFO)
			{
				value = Last.Value;
				RemoveLast();
			}
			else
			{
				value = First.Value;
				RemoveFirst();
			}

			return value;
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}

			if (Priority == DequeuePriority.LIFO)
			{
				item = Last.Value;
				RemoveLast();
			}
			else
			{
				item = First.Value;
				RemoveFirst();
			}

			return true;
		}

		public void Push(T item) { AddFirst(item); }

		public T Pop()
		{
			AssertNotEmpty();

			T value;

			if (Priority == DequeuePriority.LIFO)
			{
				value = First.Value;
				RemoveFirst();
			}
			else
			{
				value = Last.Value;
				RemoveLast();
			}

			return value;
		}

		/// <inheritdoc />
		public bool TryPop(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}

			if (Priority == DequeuePriority.LIFO)
			{
				item = First.Value;
				RemoveFirst();
			}
			else
			{
				item = Last.Value;
				RemoveLast();
			}

			return true;
		}

		public T Peek()
		{
			AssertNotEmpty();
			LinkedListNode<T> node = Priority == DequeuePriority.LIFO
										? Last
										: First;
			return node.Value;
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			LinkedListNode<T> node = Priority == DequeuePriority.LIFO
										? Last
										: First;
			item = node.Value;
			return true;
		}

		public T PeekTail()
		{
			AssertNotEmpty();
			LinkedListNode<T> node = Priority == DequeuePriority.LIFO
										? First
										: Last;
			return node.Value;
		}

		/// <inheritdoc />
		public bool TryPeekTail(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			LinkedListNode<T> node = Priority == DequeuePriority.LIFO
										? First
										: Last;
			item = node.Value;
			return true;
		}

		private void AssertNotEmpty()
		{
			if (Count > 0) return;
			throw new InvalidOperationException("Queue is empty.");
		}
	}
}
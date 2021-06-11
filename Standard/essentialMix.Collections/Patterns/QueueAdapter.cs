using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections.Patterns
{
	public class QueueAdapter<T> : IQueue<T>, IReadOnlyCollection<T>, ICollection, IEnumerable<T>, IEnumerable
	{
		private readonly ICollection _collection;
		private readonly IReadOnlyCollection<T> _readOnlyCollection;
		private readonly Action<T> _enqueue;
		private readonly Func<T> _dequeue;
		private readonly Action _clear;

		internal QueueAdapter([NotNull] ICollection collection, [NotNull] Action<T> enqueue, [NotNull] Func<T> dequeue, [NotNull] Action clear)
		{
			_collection = collection;
			_readOnlyCollection = collection as IReadOnlyCollection<T> ?? throw new NotSupportedException();
			_enqueue = enqueue;
			_dequeue = dequeue;
			_clear = clear;
		}

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc cref="ICollection.Count" />
		public int Count => _collection.Count;

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return _readOnlyCollection.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }

		/// <inheritdoc />
		public void Enqueue(T item) { _enqueue(item); }

		/// <inheritdoc />
		public T Dequeue() { return _dequeue(); }

		/// <inheritdoc />
		public void Clear() { _clear(); }
	}

	public static class CollectionAdapter
	{
		[NotNull]
		public static QueueAdapter<T> Create<T>([NotNull] Queue<T> queue)
		{
			return new QueueAdapter<T>(queue, queue.Enqueue, queue.Dequeue, queue.Clear);
		}

		[NotNull]
		public static QueueAdapter<T> Create<T>([NotNull] Stack<T> stack)
		{
			return new QueueAdapter<T>(stack, stack.Push, stack.Pop, stack.Clear);
		}

		[NotNull]
		public static QueueAdapter<T> Create<T>([NotNull] Deque<T> deque)
		{
			return new QueueAdapter<T>(deque, deque.Enqueue, deque.Dequeue, deque.Clear);
		}

		[NotNull]
		public static QueueAdapter<T> Create<T>([NotNull] CircularBuffer<T> circularBuffer)
		{
			return new QueueAdapter<T>(circularBuffer, circularBuffer.Add, circularBuffer.Dequeue, circularBuffer.Clear);
		}

		[NotNull]
		public static QueueAdapter<T> Create<T>([NotNull] IList<T> list)
		{
			ICollection collection = list as ICollection ?? throw new NotSupportedException();
			return new QueueAdapter<T>(collection, list.Add, () =>
			{
				if (collection.Count == 0) throw new Exception("Collection is empty.");
				T item = list[0];
				list.RemoveAt(0);
				return item;
			}, list.Clear);
		}
	}
}

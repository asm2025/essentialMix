using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class QueueWrapper<T> : IQueue<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly Queue<T> _queue;
		private readonly ICollection _collection;

		public QueueWrapper()
			: this(new Queue<T>())
		{
		}

		public QueueWrapper(IEnumerable<T> collection)
			: this(new Queue<T>(collection))
		{
		}

		public QueueWrapper(int capacity)
			: this(new Queue<T>(capacity))
		{
		}

		public QueueWrapper([NotNull] Queue<T> queue)
		{
			_queue = queue;
			_collection = queue;
		}

		/// <inheritdoc cref="ICollection" />
		public int Count => _queue.Count;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		object ICollection.SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return _queue.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { _queue.Enqueue(item); }

		/// <inheritdoc />
		public T Dequeue() { return _queue.Dequeue(); }

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (_queue.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = _queue.Dequeue();
			return true;
		}

		/// <inheritdoc />
		public T Peek() { return _queue.Peek(); }

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			item = _queue.Peek();
			return true;
		}

		/// <inheritdoc />
		public void Clear() { _queue.Clear(); }

		public bool Contains(T item) { return _queue.Contains(item); }

		public void CopyTo(T[] array, int index) { _collection.CopyTo(array, index); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }

		[NotNull]
		public T[] ToArray() { return _queue.ToArray(); }
		public void TrimExcess() { _queue.TrimExcess(); }
	}
}
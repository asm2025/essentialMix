using System;
using System.Collections;
using System.Collections.Generic;
using essentialMix.Collections;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Collections
{
	public class QueueAdapter<TCollection, T> : IQueue<T>, IReadOnlyCollection<T>, ICollection, IEnumerable<T>, IEnumerable
		where TCollection : ICollection, IReadOnlyCollection<T>
	{
		private readonly TCollection _collection;
		private readonly Action<T> _enqueue;
		private readonly Func<T> _dequeue;
		private readonly Action _clear;

		internal QueueAdapter([NotNull] TCollection collection, [NotNull] Action<T> enqueue, [NotNull] Func<T> dequeue, [NotNull] Action clear)
		{
			_collection = collection;
			_enqueue = enqueue;
			_dequeue = dequeue;
			_clear = clear;
		}

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc cref="ICollection.Count" />
		public int Count => ((ICollection)_collection).Count;

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return _collection.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }

		/// <inheritdoc />
		public void Enqueue(T item) { _enqueue(item); }

		/// <inheritdoc />
		public bool Remove(T item) { return _dequeue(item); }

		/// <inheritdoc />
		public void Clear() { _clear(); }

		/// <inheritdoc />
		public bool Contains(T item) { return _contains(item); }

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { _copyTo(array, arrayIndex); }
	}

	public static class CollectionAdapter
	{
		[NotNull]
		public static ICollection<T> Create<T>([NotNull] ICollection<T> collection)
		{
			ICollection col = collection as ICollection ?? throw new NotSupportedException();
			return new QueueAdapter<T>(col, collection.Add, collection.Remove, collection.Clear, collection.Contains, collection.CopyTo);
		}

		[NotNull]
		public static ICollection<T> Create<T>([NotNull] Queue<T> queue)
		{
			return new QueueAdapter<T>(queue, queue.Enqueue, queue.Dequeue, queue.Clear, queue.Contains, queue.CopyTo);
		}
	}
}

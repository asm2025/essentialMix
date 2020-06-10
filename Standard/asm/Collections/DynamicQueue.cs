using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("Count = {Count}")]
	public sealed class DynamicQueue<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	{
		private readonly dynamic _queueOrStack;
		private readonly ICollection _collection;
		private readonly Action<T> _enqueue;
		private readonly Func<T> _dequeue;

		public DynamicQueue(DequeuePriority priority)
			: this(priority, 0, null)
		{
		}

		public DynamicQueue(DequeuePriority priority, int capacity)
			: this(priority, capacity, null)
		{
		}

		public DynamicQueue(DequeuePriority priority, [NotNull] IEnumerable<T> collection)
			: this(priority, 0, collection)
		{
		}

		private DynamicQueue(DequeuePriority priority, int capacity, IEnumerable<T> collection)
		{
			Priority = priority;

			switch (priority)
			{
				case DequeuePriority.FIFO:
					_queueOrStack = collection == null
										? new Queue<T>(capacity)
										: new Queue<T>(collection);
					_enqueue = e => _queueOrStack.Enqueue(e);
					_dequeue = () => _queueOrStack.Dequeue();
					break;
				case DequeuePriority.LIFO:
					_queueOrStack = collection == null
										? new Stack<T>(capacity)
										: new Stack<T>(collection);
					_enqueue = e => _queueOrStack.Push(e);
					_dequeue = () => _queueOrStack.Pop();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
			}

			_collection = _queueOrStack;
		}

		public DequeuePriority Priority { get; }

		public int Count => _queueOrStack.Count;

		public object SyncRoot => _collection.SyncRoot;

		public bool IsSynchronized => _collection.IsSynchronized;

		public IEnumerator<T> GetEnumerator() { return _queueOrStack.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Enqueue(T item) { _enqueue(item); }
		
		public T Dequeue() { return _dequeue(); }

		public T Peek() { return _queueOrStack.Peek(); }

		public void Clear() { _queueOrStack.Clear(); }

		void ICollection.CopyTo(Array array, int index)
		{
			_collection.CopyTo(array, index);
		}

		public void CopyTo(T[] array, int index) { _queueOrStack.CopyTo(array, index); }
		
		public bool Contains(T item) { return _queueOrStack.Contains(item); }

		public T[] ToArray() { return _queueOrStack.ToArray(); }

		public void TrimExcess() { _queueOrStack.TrimExcess(); }
	}
}
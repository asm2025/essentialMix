using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using essentialMix.Delegation;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public readonly struct QueueAdapter<TQueue, T> : ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly ICollection _collection;
		private readonly Action<T> _enqueue;
		private readonly OutFunc<T, bool> _dequeue;
		private readonly OutFunc<T, bool> _peek;
		private readonly Action _clear;

		public QueueAdapter([NotNull] TQueue tQueue)
		{
			switch (tQueue)
			{
				case Queue<T> queue:
					_enqueue = queue.Enqueue;
					_dequeue = (out T item) =>
					{
						if (queue.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = queue.Dequeue();
						return true;
					};
					_peek = (out T item) =>
					{
						if (queue.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = queue.Peek();
						return true;
					};
					_clear = queue.Clear;
					break;
				case Stack<T> stack:
					_enqueue = stack.Push;
					_dequeue = (out T item) =>
					{
						if (stack.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = stack.Pop();
						return true;
					};
					_peek = (out T item) =>
					{
						if (stack.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = stack.Peek();
						return true;
					};
					_clear = stack.Clear;
					break;
				case ConcurrentQueue<T> concurrentQueue:
					_enqueue = concurrentQueue.Enqueue;
					_dequeue = concurrentQueue.TryDequeue;
					_peek = concurrentQueue.TryPeek;
					MethodInfo miClear = typeof(TQueue).GetMethod("Clear", Constants.BF_PUBLIC_INSTANCE);
					_clear = miClear != null
								? () => miClear.Invoke(concurrentQueue, null)
								: () =>
								{
									while (concurrentQueue.TryDequeue(out _))
									{
									}
								};
					break;
				case ConcurrentStack<T> concurrentStack:
					_enqueue = concurrentStack.Push;
					_dequeue = concurrentStack.TryPop;
					_peek = concurrentStack.TryPeek;
					_clear = concurrentStack.Clear;
					break;
				case BlockingCollection<T> blockingCollection:
					_enqueue = blockingCollection.Add;
					_dequeue = blockingCollection.TryTake;
					_peek = (out T item) => throw new NotSupportedException();
					_clear = blockingCollection.Clear;
					break;
				case IDeque<T> deque:
					_enqueue = deque.Enqueue;
					_dequeue = (out T item) =>
					{
						if (deque.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = deque.Dequeue();
						return true;
					};
					_peek = (out T item) =>
					{
						if (deque.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = deque.Peek();
						return true;
					};
					_clear = deque.Clear;
					break;
				case ICircularBuffer<T> circularBuffer:
					_enqueue = circularBuffer.Enqueue;
					_dequeue = (out T item) =>
					{
						if (circularBuffer.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = circularBuffer.Dequeue();
						return true;
					};
					_peek = (out T item) =>
					{
						if (circularBuffer.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = circularBuffer.Peek();
						return true;
					};
					_clear = circularBuffer.Clear;
					break;
				case IHeap<T> heap:
					_enqueue = heap.Add;
					_dequeue = (out T item) =>
					{
						if (heap.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = heap.ExtractValue();
						return true;
					};
					_peek = (out T item) =>
					{
						if (heap.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = heap.Value();
						return true;
					};
					_clear = heap.Clear;
					break;
				case IQueue<T> queue:
					_enqueue = queue.Enqueue;
					_dequeue = (out T item) =>
					{
						if (queue.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = queue.Dequeue();
						return true;
					};
					_peek = (out T item) =>
					{
						if (queue.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = queue.Peek();
						return true;
					};
					_clear = queue.Clear;
					break;
				case LinkedList<T> linkedList:
					_enqueue = item => linkedList.AddLast(item);
					_dequeue = (out T item) =>
					{
						if (linkedList.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = linkedList.First.Value;
						linkedList.RemoveFirst();
						return true;
					};
					_peek = (out T item) =>
					{
						if (linkedList.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = linkedList.First.Value;
						return true;
					};
					_clear = linkedList.Clear;
					break;
				case IList<T> list:
					_enqueue = list.Add;
					_dequeue = (out T item) =>
					{
						if (list.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = list[0];
						list.RemoveAt(0);
						return true;
					};
					_peek = (out T item) =>
					{
						if (list.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = list[0];
						return true;
					};
					_clear = list.Clear;
					break;
				default:
					throw new NotSupportedException();
			}
			
			Queue = tQueue;
			_collection = tQueue;
		}

		public TQueue Queue { get; }

		/// <inheritdoc />
		public bool IsSynchronized => Queue.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => Queue.SyncRoot;

		/// <inheritdoc cref="ICollection.Count" />
		public int Count => _collection.Count;

		public bool IsEmpty => _collection.Count == 0;

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Queue.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { Queue.CopyTo(array, index); }

		public void Enqueue(T item) { _enqueue(item); }

		public bool TryDequeue(out T item) { return _dequeue(out item); }

		public bool TryPeek(out T item) { return _peek(out item); }

		public void Clear() { _clear(); }
	}
}
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using essentialMix.Delegation;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public readonly struct QueueAdapter<TQueue, T> : ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly ICollection _collection;
		private readonly Action<T> _enqueue;
		private readonly Func<T> _dequeue;
		private readonly Func<T> _peek;
		private readonly OutFunc<T, bool> _tryDequeue;
		private readonly OutFunc<T, bool> _tryPeek;
		private readonly Action _clear;

		public QueueAdapter([NotNull] TQueue tQueue)
		{
			switch (tQueue)
			{
				case Queue<T> queue:
					_enqueue = queue.Enqueue;
					_dequeue = queue.Dequeue;
					_peek = queue.Peek;
					_tryDequeue = (out T item) =>
					{
						if (queue.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = queue.Dequeue();
						return true;
					};
					_tryPeek = (out T item) =>
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
					_dequeue = stack.Pop;
					_peek = stack.Peek;
					_tryDequeue = (out T item) =>
					{
						if (stack.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = stack.Pop();
						return true;
					};
					_tryPeek = (out T item) =>
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
					_dequeue = () =>
					{
						if (!concurrentQueue.TryDequeue(out T item)) throw new CollectionIsEmptyException();
						return item;
					};
					_peek = () =>
					{
						if (!concurrentQueue.TryPeek(out T item)) throw new CollectionIsEmptyException();
						return item;
					};
					_tryDequeue = concurrentQueue.TryDequeue;
					_tryPeek = concurrentQueue.TryPeek;
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
					_dequeue = () =>
					{
						if (!concurrentStack.TryPop(out T item)) throw new CollectionIsEmptyException();
						return item;
					};
					_peek = () =>
					{
						if (!concurrentStack.TryPeek(out T item)) throw new CollectionIsEmptyException();
						return item;
					};
					_tryDequeue = concurrentStack.TryPop;
					_tryPeek = concurrentStack.TryPeek;
					_clear = concurrentStack.Clear;
					break;
				case BlockingCollection<T> blockingCollection:
					_enqueue = blockingCollection.Add;
					_dequeue = () =>
					{
						if (!blockingCollection.TryTake(out T item)) throw new CollectionIsEmptyException();
						return item;
					};
					_peek = () => throw new NotSupportedException();
					_tryDequeue = blockingCollection.TryTake;
					_tryPeek = (out T item) => throw new NotSupportedException();
					_clear = blockingCollection.Clear;
					break;
				case IDeque<T> deque:
					_enqueue = deque.Enqueue;
					_dequeue = deque.Dequeue;
					_peek = deque.Peek;
					_tryDequeue = (out T item) =>
					{
						if (deque.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = deque.Dequeue();
						return true;
					};
					_tryPeek = (out T item) =>
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
					_dequeue = circularBuffer.Dequeue;
					_peek = circularBuffer.Peek;
					_tryDequeue = (out T item) =>
					{
						if (circularBuffer.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = circularBuffer.Dequeue();
						return true;
					};
					_tryPeek = (out T item) =>
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
					_dequeue = heap.ExtractValue;
					_peek = heap.Value;
					_tryDequeue = (out T item) =>
					{
						if (heap.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = heap.ExtractValue();
						return true;
					};
					_tryPeek = (out T item) =>
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
					_dequeue = queue.Dequeue;
					_peek = queue.Peek;
					_tryDequeue = (out T item) =>
					{
						if (queue.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = queue.Dequeue();
						return true;
					};
					_tryPeek = (out T item) =>
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
				case IStack<T> stack:
					_enqueue = stack.Push;
					_dequeue = stack.Pop;
					_peek = stack.Peek;
					_tryDequeue = (out T item) =>
					{
						if (stack.Count == 0)
						{
							item = default(T);
							return false;
						}

						item = stack.Pop();
						return true;
					};
					_tryPeek = (out T item) =>
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
				case LinkedList<T> linkedList:
					_enqueue = item => linkedList.AddLast(item);
					_dequeue = () =>
					{
						if (linkedList.Count == 0) throw new CollectionIsEmptyException();
						T item = linkedList.First.Value;
						linkedList.RemoveFirst();
						return item;
					};
					_peek = () =>
					{
						if (linkedList.Count == 0) throw new CollectionIsEmptyException();
						T item = linkedList.First.Value;
						return item;
					};
					_tryDequeue = (out T item) =>
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
					_tryPeek = (out T item) =>
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
					_dequeue = () =>
					{
						if (list.Count == 0) throw new CollectionIsEmptyException();
						T item = list[0];
						list.RemoveAt(0);
						return item;
					};
					_peek = () =>
					{
						if (list.Count == 0) throw new CollectionIsEmptyException();
						return list[0];
					};
					_tryDequeue = (out T item) =>
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
					_tryPeek = (out T item) =>
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

		public T Dequeue() { return _dequeue(); }

		public T Peek() { return _peek(); }

		public bool TryDequeue(out T item) { return _tryDequeue(out item); }

		public bool TryPeek(out T item) { return _tryPeek(out item); }

		public void Clear() { _clear(); }
	}
}
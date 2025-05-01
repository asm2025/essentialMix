using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("Count = {Count}")]
public readonly struct QueueAdapter<TQueue, T> : ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
	where TQueue : ICollection, IReadOnlyCollection<T>
{
	#region Proxies
	private interface IQueueAdapter : IQueue<T>
	{
	}

	private readonly struct QueueProxy(IQueue<T> queue) : IQueueAdapter
	{
		public QueueProxy([NotNull] Queue<T> queue)
			: this(new QueueWrapper<T>(queue))
		{
		}

		/// <inheritdoc />
		public int Count => queue.Count;

		/// <inheritdoc />
		public bool IsSynchronized => queue.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => queue.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return queue.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { queue.Enqueue(item); }

		/// <inheritdoc />
		public T Dequeue() { return queue.Dequeue(); }

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (queue.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = queue.Dequeue();
			return true;
		}

		/// <inheritdoc />
		public T Peek() { return queue.Peek(); }

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (queue.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = queue.Peek();
			return true;
		}

		/// <inheritdoc />
		public void Clear() { queue.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { queue.CopyTo(array, index); }
	}

	private readonly struct StackProxy(IStack<T> stack) : IQueueAdapter
	{
		public StackProxy([NotNull] Stack<T> stack)
			: this(new StackWrapper<T>(stack))
		{
		}

		/// <inheritdoc />
		public int Count => stack.Count;

		/// <inheritdoc />
		public bool IsSynchronized => stack.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => stack.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return stack.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { stack.Push(item); }

		/// <inheritdoc />
		public T Dequeue() { return stack.Pop(); }

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (stack.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = stack.Pop();
			return true;
		}

		/// <inheritdoc />
		public T Peek() { return stack.Peek(); }

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (stack.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = stack.Peek();
			return true;
		}

		/// <inheritdoc />
		public void Clear() { stack.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { stack.CopyTo(array, index); }
	}

	private readonly struct ConcurrentQueueProxy(ConcurrentQueue<T> queue) : IQueueAdapter
	{
		private readonly ICollection _collection = queue;

		/// <inheritdoc />
		public int Count => queue.Count;

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return _collection.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { queue.Enqueue(item); }

		/// <inheritdoc />
		public T Dequeue()
		{
			if (!queue.TryDequeue(out T item)) throw new CollectionIsEmptyException();
			return item;
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item) { return queue.TryDequeue(out item); }

		/// <inheritdoc />
		public T Peek()
		{
			if (!queue.TryPeek(out T item)) throw new CollectionIsEmptyException();
			return item;
		}

		/// <inheritdoc />
		public bool TryPeek(out T item) { return queue.TryPeek(out item); }

		/// <inheritdoc />
		public void Clear() { queue.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
	}

	private readonly struct ConcurrentStackProxy(ConcurrentStack<T> stack) : IQueueAdapter
	{
		private readonly ICollection _collection = stack;

		/// <inheritdoc />
		public int Count => stack.Count;

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return _collection.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { stack.Push(item); }

		/// <inheritdoc />
		public T Dequeue()
		{
			if (!stack.TryPop(out T item)) throw new CollectionIsEmptyException();
			return item;
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item) { return stack.TryPop(out item); }

		/// <inheritdoc />
		public T Peek()
		{
			if (!stack.TryPeek(out T item)) throw new CollectionIsEmptyException();
			return item;
		}

		/// <inheritdoc />
		public bool TryPeek(out T item) { return stack.TryPeek(out item); }

		/// <inheritdoc />
		public void Clear() { stack.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
	}

	private readonly struct BlockingCollectionProxy(BlockingCollection<T> blockingCollection) : IQueueAdapter
	{
		private readonly ICollection _collection = blockingCollection;

		/// <inheritdoc />
		public int Count => blockingCollection.Count;

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return _collection.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { blockingCollection.Add(item); }

		/// <inheritdoc />
		public T Dequeue()
		{
			if (!blockingCollection.TryTake(out T item)) throw new CollectionIsEmptyException();
			return item;
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item) { return blockingCollection.TryTake(out item); }

		/// <inheritdoc />
		public T Peek()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc />
		public bool TryPeek(out T item) { throw new NotSupportedException(); }

		/// <inheritdoc />
		public void Clear() { blockingCollection.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
	}

	private readonly struct DequeProxy(IDeque<T> deque) : IQueueAdapter
	{
		/// <inheritdoc />
		public int Count => deque.Count;

		/// <inheritdoc />
		public bool IsSynchronized => deque.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => deque.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return deque.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { deque.Enqueue(item); }

		/// <inheritdoc />
		public T Dequeue() { return deque.Dequeue(); }

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (deque.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = deque.Dequeue();
			return true;
		}

		/// <inheritdoc />
		public T Peek() { return deque.Peek(); }

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (deque.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = deque.Peek();
			return true;
		}

		/// <inheritdoc />
		public void Clear() { deque.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { deque.CopyTo(array, index); }
	}

	private readonly struct CircularBufferProxy(ICircularBuffer<T> buffer) : IQueueAdapter
	{
		/// <inheritdoc />
		public int Count => buffer.Count;

		/// <inheritdoc />
		public bool IsSynchronized => buffer.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => buffer.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return buffer.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { buffer.Enqueue(item); }

		/// <inheritdoc />
		public T Dequeue() { return buffer.Dequeue(); }

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (buffer.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = buffer.Dequeue();
			return true;
		}

		/// <inheritdoc />
		public T Peek() { return buffer.Peek(); }

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (buffer.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = buffer.Peek();
			return true;
		}

		/// <inheritdoc />
		public void Clear() { buffer.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { buffer.CopyTo(array, index); }
	}

	private readonly struct HeapProxy(IHeap<T> heap) : IQueueAdapter
	{
		private readonly ICollection _collection = heap as ICollection ?? throw new NotSupportedException();

		/// <inheritdoc />
		public int Count => _collection.Count;

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return heap.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { heap.Add(item); }

		/// <inheritdoc />
		[NotNull]
		public T Dequeue() { return heap.ExtractValue(); }

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (heap.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = heap.ExtractValue();
			return true;
		}

		/// <inheritdoc />
		[NotNull]
		public T Peek() { return heap.Value(); }

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (heap.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = heap.Value();
			return true;
		}

		/// <inheritdoc />
		public void Clear() { heap.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
	}

	private readonly struct LinkedListProxy(LinkedList<T> linkedList) : IQueueAdapter
	{
		private readonly ICollection _collection = linkedList;

		/// <inheritdoc />
		public int Count => linkedList.Count;

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return linkedList.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { linkedList.AddLast(item); }

		/// <inheritdoc />
		public T Dequeue()
		{
			if (linkedList.Count == 0) throw new CollectionIsEmptyException();
			T item = linkedList.First.Value;
			linkedList.RemoveFirst();
			return item;
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (linkedList.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = linkedList.First.Value;
			linkedList.RemoveFirst();
			return true;
		}

		/// <inheritdoc />
		public T Peek()
		{
			if (linkedList.Count == 0) throw new CollectionIsEmptyException();
			T item = linkedList.First.Value;
			return item;
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (linkedList.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = linkedList.First.Value;
			return true;
		}

		/// <inheritdoc />
		public void Clear() { linkedList.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
	}

	private readonly struct ListProxy(List<T> list) : IQueueAdapter
	{
		private readonly ICollection _collection = list;

		/// <inheritdoc />
		public int Count => list.Count;

		/// <inheritdoc />
		public bool IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return list.GetEnumerator(); }

		/// <inheritdoc />
		public void Enqueue(T item) { list.Add(item); }

		/// <inheritdoc />
		public T Dequeue()
		{
			if (list.Count == 0) throw new CollectionIsEmptyException();
			T item = list[0];
			list.RemoveAt(0);
			return item;
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (list.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = list[0];
			list.RemoveAt(0);
			return true;
		}

		/// <inheritdoc />
		public T Peek()
		{
			if (list.Count == 0) throw new CollectionIsEmptyException();
			return list[0];
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (list.Count == 0)
			{
				item = default(T);
				return false;
			}

			item = list[0];
			return true;
		}

		/// <inheritdoc />
		public void Clear() { list.Clear(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
	}
	#endregion

	private readonly IQueueAdapter _adapter;

	public QueueAdapter([NotNull] TQueue tQueue)
	{
		_adapter = tQueue switch
		{
			IDeque<T> deque => new DequeProxy(deque),
			ICircularBuffer<T> buffer => new CircularBufferProxy(buffer),
			IHeap<T> heap => new HeapProxy(heap),
			Queue<T> queue => new QueueProxy(queue),
			IQueue<T> queue => new QueueProxy(queue),
			Stack<T> stack => new StackProxy(stack),
			IStack<T> stack => new StackProxy(stack),
			ConcurrentQueue<T> queue => new ConcurrentQueueProxy(queue),
			ConcurrentStack<T> stack => new ConcurrentStackProxy(stack),
			BlockingCollection<T> blockingCollection => new BlockingCollectionProxy(blockingCollection),
			LinkedList<T> linkedList => new LinkedListProxy(linkedList),
			List<T> list => new ListProxy(list),
			_ => throw new NotSupportedException()
		};
			
		Queue = tQueue;
	}

	public TQueue Queue { get; }

	/// <inheritdoc />
	public bool IsSynchronized => Queue.IsSynchronized;

	/// <inheritdoc />
	public object SyncRoot => Queue.SyncRoot;

	/// <inheritdoc cref="ICollection.Count" />
	public int Count => _adapter.Count;

	public bool IsEmpty => _adapter.Count == 0;

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() { return Queue.GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public void CopyTo(Array array, int index) { Queue.CopyTo(array, index); }

	public void Enqueue(T item) { _adapter.Enqueue(item); }

	public T Dequeue() { return _adapter.Dequeue(); }

	public T Peek() { return _adapter.Peek(); }

	public bool TryDequeue(out T item) { return _adapter.TryDequeue(out item); }

	public bool TryPeek(out T item) { return _adapter.TryPeek(out item); }

	public void Clear() { _adapter.Clear(); }
}
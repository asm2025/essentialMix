using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public readonly struct QueueAdapter<TQueue, T> : ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		#region Proxies
		private interface IQueueAdapter : IQueue<T>
		{
		}

		private readonly struct QueueProxy : IQueueAdapter
		{
			private readonly IQueue<T> _queue;

			public QueueProxy([NotNull] Queue<T> queue)
				: this(new QueueWrapper<T>(queue))
			{
			}

			public QueueProxy(IQueue<T> queue)
			{
				_queue = queue;
			}

			/// <inheritdoc />
			public int Count => _queue.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _queue.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _queue.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _queue.GetEnumerator(); }

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
				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Peek();
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _queue.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _queue.CopyTo(array, index); }
		}

		private readonly struct StackProxy : IQueueAdapter
		{
			private readonly IStack<T> _stack;

			public StackProxy([NotNull] Stack<T> stack)
				: this(new StackWrapper<T>(stack))
			{
			}

			public StackProxy(IStack<T> stack)
			{
				_stack = stack;
			}

			/// <inheritdoc />
			public int Count => _stack.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _stack.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _stack.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _stack.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _stack.Push(item); }

			/// <inheritdoc />
			public T Dequeue() { return _stack.Pop(); }

			/// <inheritdoc />
			public bool TryDequeue(out T item)
			{
				if (_stack.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _stack.Pop();
				return true;
			}

			/// <inheritdoc />
			public T Peek() { return _stack.Peek(); }

			/// <inheritdoc />
			public bool TryPeek(out T item)
			{
				if (_stack.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _stack.Peek();
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _stack.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _stack.CopyTo(array, index); }
		}

		private readonly struct ConcurrentQueueProxy : IQueueAdapter
		{
			private readonly ConcurrentQueue<T> _queue;
			private readonly ICollection _collection;

			public ConcurrentQueueProxy(ConcurrentQueue<T> queue)
			{
				_queue = queue;
				_collection = queue;
			}

			/// <inheritdoc />
			public int Count => _queue.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _collection.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _collection.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _collection.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _queue.Enqueue(item); }

			/// <inheritdoc />
			public T Dequeue()
			{
				if (!_queue.TryDequeue(out T item)) throw new CollectionIsEmptyException();
				return item;
			}

			/// <inheritdoc />
			public bool TryDequeue(out T item) { return _queue.TryDequeue(out item); }

			/// <inheritdoc />
			public T Peek()
			{
				if (!_queue.TryPeek(out T item)) throw new CollectionIsEmptyException();
				return item;
			}

			/// <inheritdoc />
			public bool TryPeek(out T item) { return _queue.TryPeek(out item); }

			/// <inheritdoc />
			public void Clear() { _queue.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
		}

		private readonly struct ConcurrentStackProxy : IQueueAdapter
		{
			private readonly ConcurrentStack<T> _stack;
			private readonly ICollection _collection;

			public ConcurrentStackProxy(ConcurrentStack<T> stack)
			{
				_stack = stack;
				_collection = stack;
			}

			/// <inheritdoc />
			public int Count => _stack.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _collection.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _collection.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _collection.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _stack.Push(item); }

			/// <inheritdoc />
			public T Dequeue()
			{
				if (!_stack.TryPop(out T item)) throw new CollectionIsEmptyException();
				return item;
			}

			/// <inheritdoc />
			public bool TryDequeue(out T item) { return _stack.TryPop(out item); }

			/// <inheritdoc />
			public T Peek()
			{
				if (!_stack.TryPeek(out T item)) throw new CollectionIsEmptyException();
				return item;
			}

			/// <inheritdoc />
			public bool TryPeek(out T item) { return _stack.TryPeek(out item); }

			/// <inheritdoc />
			public void Clear() { _stack.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
		}

		private readonly struct BlockingCollectionProxy : IQueueAdapter
		{
			private readonly BlockingCollection<T> _blockingCollection;
			private readonly ICollection _collection;

			public BlockingCollectionProxy(BlockingCollection<T> blockingCollection)
			{
				_blockingCollection = blockingCollection;
				_collection = blockingCollection;
			}

			/// <inheritdoc />
			public int Count => _blockingCollection.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _collection.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _collection.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _collection.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _blockingCollection.Add(item); }

			/// <inheritdoc />
			public T Dequeue()
			{
				if (!_blockingCollection.TryTake(out T item)) throw new CollectionIsEmptyException();
				return item;
			}

			/// <inheritdoc />
			public bool TryDequeue(out T item) { return _blockingCollection.TryTake(out item); }

			/// <inheritdoc />
			public T Peek()
			{
				throw new NotSupportedException();
			}

			/// <inheritdoc />
			public bool TryPeek(out T item) { throw new NotSupportedException(); }

			/// <inheritdoc />
			public void Clear() { _blockingCollection.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
		}

		private readonly struct DequeProxy : IQueueAdapter
		{
			private readonly IDeque<T> _deque;

			public DequeProxy(IDeque<T> deque)
			{
				_deque = deque;
			}

			/// <inheritdoc />
			public int Count => _deque.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _deque.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _deque.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _deque.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _deque.Enqueue(item); }

			/// <inheritdoc />
			public T Dequeue() { return _deque.Dequeue(); }

			/// <inheritdoc />
			public bool TryDequeue(out T item)
			{
				if (_deque.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _deque.Dequeue();
				return true;
			}

			/// <inheritdoc />
			public T Peek() { return _deque.Peek(); }

			/// <inheritdoc />
			public bool TryPeek(out T item)
			{
				if (_deque.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _deque.Peek();
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _deque.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _deque.CopyTo(array, index); }
		}

		private readonly struct CircularBufferProxy : IQueueAdapter
		{
			private readonly ICircularBuffer<T> _buffer;

			public CircularBufferProxy(ICircularBuffer<T> buffer)
			{
				_buffer = buffer;
			}

			/// <inheritdoc />
			public int Count => _buffer.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _buffer.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _buffer.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _buffer.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _buffer.Enqueue(item); }

			/// <inheritdoc />
			public T Dequeue() { return _buffer.Dequeue(); }

			/// <inheritdoc />
			public bool TryDequeue(out T item)
			{
				if (_buffer.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _buffer.Dequeue();
				return true;
			}

			/// <inheritdoc />
			public T Peek() { return _buffer.Peek(); }

			/// <inheritdoc />
			public bool TryPeek(out T item)
			{
				if (_buffer.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _buffer.Peek();
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _buffer.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _buffer.CopyTo(array, index); }
		}

		private readonly struct HeapProxy : IQueueAdapter
		{
			private readonly IHeap<T> _heap;
			private readonly ICollection _collection;

			public HeapProxy(IHeap<T> heap)
			{
				_heap = heap;
				_collection = heap;
			}

			/// <inheritdoc />
			public int Count => _collection.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _heap.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _heap.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _heap.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _heap.Add(item); }

			/// <inheritdoc />
			[NotNull]
			public T Dequeue() { return _heap.ExtractValue(); }

			/// <inheritdoc />
			public bool TryDequeue(out T item)
			{
				if (_collection.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _heap.ExtractValue();
				return true;
			}

			/// <inheritdoc />
			[NotNull]
			public T Peek() { return _heap.Value(); }

			/// <inheritdoc />
			public bool TryPeek(out T item)
			{
				if (_collection.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _heap.Value();
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _heap.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _heap.CopyTo(array, index); }
		}

		private readonly struct LinkedListProxy : IQueueAdapter
		{
			private readonly LinkedList<T> _linkedList;
			private readonly ICollection _collection;

			public LinkedListProxy(LinkedList<T> linkedList)
			{
				_linkedList = linkedList;
				_collection = linkedList;
			}

			/// <inheritdoc />
			public int Count => _linkedList.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _collection.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _collection.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _linkedList.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _linkedList.AddLast(item); }

			/// <inheritdoc />
			public T Dequeue()
			{
				if (_linkedList.Count == 0) throw new CollectionIsEmptyException();
				T item = _linkedList.First.Value;
				_linkedList.RemoveFirst();
				return item;
			}

			/// <inheritdoc />
			public bool TryDequeue(out T item)
			{
				if (_linkedList.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _linkedList.First.Value;
				_linkedList.RemoveFirst();
				return true;
			}

			/// <inheritdoc />
			public T Peek()
			{
				if (_linkedList.Count == 0) throw new CollectionIsEmptyException();
				T item = _linkedList.First.Value;
				return item;
			}

			/// <inheritdoc />
			public bool TryPeek(out T item)
			{
				if (_linkedList.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _linkedList.First.Value;
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _linkedList.Clear(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
		}

		private readonly struct ListProxy : IQueueAdapter
		{
			private readonly List<T> _list;
			private readonly ICollection _collection;

			public ListProxy(List<T> list)
			{
				_list = list;
				_collection = list;
			}

			/// <inheritdoc />
			public int Count => _list.Count;

			/// <inheritdoc />
			public bool IsSynchronized => _collection.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => _collection.SyncRoot;

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return _list.GetEnumerator(); }

			/// <inheritdoc />
			public void Enqueue(T item) { _list.Add(item); }

			/// <inheritdoc />
			public T Dequeue()
			{
				if (_list.Count == 0) throw new CollectionIsEmptyException();
				T item = _list[0];
				_list.RemoveAt(0);
				return item;
			}

			/// <inheritdoc />
			public bool TryDequeue(out T item)
			{
				if (_list.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _list[0];
				_list.RemoveAt(0);
				return true;
			}

			/// <inheritdoc />
			public T Peek()
			{
				if (_list.Count == 0) throw new CollectionIsEmptyException();
				return _list[0];
			}

			/// <inheritdoc />
			public bool TryPeek(out T item)
			{
				if (_list.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _list[0];
				return true;
			}

			/// <inheritdoc />
			public void Clear() { _list.Clear(); }

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
}
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;
//using asm.Exceptions.Collections;
//using JetBrains.Annotations;

//namespace asm.Collections
//{
//	/// <summary>
//	/// <see href="https://en.wikipedia.org/wiki/Fibonacci_heap">Fibonacci heap</see> is a data structure for priority
//	/// queue operations, consisting of a collection of heap-ordered trees. It has a better amortized running time than
//	/// many other priority queue data structures including the binary heap and binomial heap.
//	/// </summary>
//	/// <typeparam name="T">The element type of the heap</typeparam>
//	// https://stackoverflow.com/questions/19508526/what-is-the-intuition-behind-the-fibonacci-heap-data-structure
//	// https://cstheory.stackexchange.com/questions/46796/is-there-a-simple-intuitive-explanation-for-why-trees-in-fibonacci-heaps-have-t
//	// https://stackoverflow.com/questions/14333314/why-is-a-fibonacci-heap-called-a-fibonacci-heap
//	// https://www.growingwiththeweb.com/data-structures/fibonacci-heap/overview/
//	// https://brilliant.org/wiki/fibonacci-heap/
//	[DebuggerDisplay("Count = {Count}")]
//	[DebuggerTypeProxy(typeof(FibonacciHeap<>.DebugView))]
//	[Serializable]
//	public abstract class FibonacciHeap<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
//	{
//		internal sealed class DebugView
//		{
//			private readonly FibonacciHeap<T> _heap;

//			public DebugView([NotNull] FibonacciHeap<T> heap)
//			{
//				_heap = heap;
//			}

//			[NotNull]
//			public FibonacciNode<T> Head => _heap.Head;
//		}

//		private struct Enumerator : IEnumerableEnumerator<T>
//		{
//			private readonly FibonacciHeap<T> _heap;
//			private readonly int _version;
//			private readonly FibonacciNode<T> _root;
//			private readonly Queue<FibonacciNode<T>> _queue;

//			private FibonacciNode<T> _current;
//			private bool _started;
//			private bool _done;

//			internal Enumerator([NotNull] FibonacciHeap<T> heap, FibonacciNode<T> root)
//			{
//				_heap = heap;
//				_version = _heap._version;
//				_root = root;
//				_queue = new Queue<FibonacciNode<T>>();
//				_current = null;
//				_started = false;
//				_done = _heap.Count == 0 || _root == null;
//			}

//			/// <inheritdoc />
//			public T Current
//			{
//				get
//				{
//					if (!_started || _current == null) throw new InvalidOperationException();
//					return _current.Value;
//				}
//			}

//			/// <inheritdoc />
//			object IEnumerator.Current => Current;

//			/// <inheritdoc />
//			public IEnumerator<T> GetEnumerator()
//			{
//				IEnumerator enumerator = this;
//				enumerator.Reset();
//				return this;
//			}

//			/// <inheritdoc />
//			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

//			public bool MoveNext()
//			{
//				if (_version != _heap._version) throw new VersionChangedException();
//				// Parent-Next-Child (Queue)
//				if (_done) return false;

//				if (!_started)
//				{
//					_started = true;
//					// Start at the root
//					_queue.Enqueue(_root);
//				}

//				// visit the next queued node
//				_current = _queue.Count > 0
//								? _queue.Dequeue()
//								: null;

//				if (_current == null)
//				{
//					_done = true;
//					return false;
//				}

//				// Queue the next nodes
//				if (_current.Next != null) _queue.Enqueue(_current.Next);
//				if (_current.Child != null) _queue.Enqueue(_current.Child);

//				return true;
//			}

//			void IEnumerator.Reset()
//			{
//				if (_version != _heap._version) throw new VersionChangedException();
//				_current = null;
//				_started = false;
//				_queue.Clear();
//				_done = _heap.Count == 0 || _root == null;
//			}

//			/// <inheritdoc />
//			public void Dispose() { }
//		}

//		protected internal int _version;

//		private object _syncRoot;

//		/// <inheritdoc />
//		protected FibonacciHeap()
//			: this((IComparer<T>)null)
//		{
//		}

//		protected FibonacciHeap(IComparer<T> comparer)
//		{
//			Comparer = comparer ?? Comparer<T>.Default;
//		}

//		protected FibonacciHeap([NotNull] IEnumerable<T> enumerable)
//			: this(enumerable, null)
//		{
//		}

//		protected FibonacciHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
//			: this(comparer)
//		{
//			Add(enumerable);
//		}

//		[NotNull]
//		public IComparer<T> Comparer { get; }
	
//		public int Count { get; protected internal set; }
	
//		protected internal FibonacciNode<T> Value { get; set; }

//		/// <inheritdoc />
//		bool ICollection<T>.IsReadOnly => false;

//		/// <inheritdoc />
//		bool ICollection.IsSynchronized => false;

//		/// <inheritdoc />
//		object ICollection.SyncRoot
//		{
//			get
//			{
//				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
//				return _syncRoot;
//			}
//		}

//		/// <inheritdoc />
//		public IEnumerator<T> GetEnumerator() { return Enumerate(Value); }

//		/// <inheritdoc />
//		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

//		/// <summary>
//		/// Enumerate nodes' values in a semi recursive approach
//		/// </summary>
//		/// <param name="head">The starting node</param>
//		/// <returns></returns>
//		[NotNull]
//		public IEnumerableEnumerator<T> Enumerate(FibonacciNode<T> head)
//		{
//			return new Enumerator(this, head);
//		}

//		/// <summary>
//		/// Iterate over nodes with a callback action
//		/// </summary>
//		/// <param name="head">The starting node</param>
//		/// <param name="visitCallback">callback action to handle the node</param>
//		public void Iterate(FibonacciNode<T> head, [NotNull] Action<FibonacciNode<T>> visitCallback)
//		{
//			if (Count == 0) return;

//			int version = _version;
//			// Head-Sibling-Child (Queue)
//			Queue<FibonacciNode<T>> queue = new Queue<FibonacciNode<T>>();

//			// Start at the root
//			queue.Enqueue(head);

//			while (queue.Count > 0)
//			{
//				if (version != _version) throw new VersionChangedException();

//				// visit the next queued node
//				FibonacciNode<T> current = queue.Dequeue();
//				visitCallback(current);

//				// Queue the next nodes
//				if (current.Next != null) queue.Enqueue(current.Next);
//				if (current.Child != null) queue.Enqueue(current.Child);
//			}
//		}

//		/// <summary>
//		/// Iterate over nodes with a callback function
//		/// </summary>
//		/// <param name="head">The starting node</param>
//		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
//		public void Iterate(FibonacciNode<T> head, [NotNull] Func<FibonacciNode<T>, bool> visitCallback)
//		{
//			if (head == null) return;

//			int version = _version;
//			// Head-Sibling-Child (Queue)
//			Queue<FibonacciNode<T>> queue = new Queue<FibonacciNode<T>>();

//			// Start at the root
//			queue.Enqueue(head);

//			while (queue.Count > 0)
//			{
//				if (version != _version) throw new VersionChangedException();

//				// visit the next queued node
//				FibonacciNode<T> current = queue.Dequeue();
//				if (!visitCallback(current)) break;

//				// Queue the next nodes
//				if (current.Next != null) queue.Enqueue(current.Next);
//				if (current.Child != null) queue.Enqueue(current.Child);
//			}
//		}

//		public void Add(T value)
//		{
//			??
//			Count++;
//			_version++;
//		}

//		public void Add([NotNull] IEnumerable<T> enumerable)
//		{
//			foreach (T item in enumerable)
//				Add(item);
//		}

//		public bool Remove(T value)
//		{
//			??
//		}

//		public void Clear()
//		{
//			Value = null;
//			Count = 0;
//			_version++;
//		}
//	}
//}
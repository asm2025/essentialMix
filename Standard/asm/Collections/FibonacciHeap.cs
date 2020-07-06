using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Fibonacci_heap">Fibonacci heap</see> is a data structure for priority
	/// queue operations, consisting of a collection of heap-ordered trees. It has a better amortized running time than
	/// many other priority queue data structures including the binary heap and binomial heap.
	/// </summary>
	/// <typeparam name="TNode">The node type. This is just for abstraction purposes and shouldn't be dealt with directly.</typeparam>
	/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
	/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
	/// priority of the item.</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	// https://stackoverflow.com/questions/19508526/what-is-the-intuition-behind-the-fibonacci-heap-data-structure
	// https://cstheory.stackexchange.com/questions/46796/is-there-a-simple-intuitive-explanation-for-why-trees-in-fibonacci-heaps-have-t
	// https://stackoverflow.com/questions/14333314/why-is-a-fibonacci-heap-called-a-fibonacci-heap
	// https://www.growingwiththeweb.com/data-structures/fibonacci-heap/overview/
	// https://brilliant.org/wiki/fibonacci-heap/
	// https://www.geeksforgeeks.org/fibonacci-heap-insertion-and-union/
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(FibonacciHeap<,,>.DebugView))]
	[Serializable]
	public abstract class FibonacciHeap<TNode, TKey, TValue> : IReadOnlyCollection<TValue>, ICollection
		where TNode : FibonacciNode<TNode, TKey, TValue>
	{
		internal sealed class DebugView
		{
			private readonly FibonacciHeap<TNode, TKey, TValue> _heap;

			public DebugView([NotNull] FibonacciHeap<TNode, TKey, TValue> heap)
			{
				_heap = heap;
			}

			[NotNull]
			public TNode Head => _heap.Head;
		}

		private struct Enumerator : IEnumerableEnumerator<TValue>
		{
			private readonly FibonacciHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] FibonacciHeap<TNode, TKey, TValue> heap, TNode root)
			{
				_heap = heap;
				_version = _heap._version;
				_root = root;
				_queue = new Queue<TNode>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			[NotNull]
			public TValue Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			[NotNull]
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<TValue> GetEnumerator()
			{
				IEnumerator enumerator = this;
				enumerator.Reset();
				return this;
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			public bool MoveNext()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				// Head-Next-Child (Queue)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queue.Enqueue(_root);
				}

				// visit the next queued node
				_current = _queue.Count > 0
								? _queue.Dequeue()
								: null;
				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes.
				if (_current.Next != null && !ReferenceEquals(_current.Next, _root))
				{
					/*
					* The previous/next nodes are not exactly a doubly linked list but rather
					* a circular reference so will have to watch out for this.
					*/
					_queue.Enqueue(_current.Next);
				}

				if (_current.Child != null) _queue.Enqueue(_current.Child);

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_queue.Clear();
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		protected internal int _version;

		private object _syncRoot;

		/// <inheritdoc />
		protected FibonacciHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected FibonacciHeap(IComparer<TKey> comparer)
		{
			Comparer = comparer ?? Comparer<TKey>.Default;
		}

		protected FibonacciHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected FibonacciHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		[NotNull]
		public IComparer<TKey> Comparer { get; }

		[NotNull]
		protected EqualityComparer<TValue> ValueComparer { get; } = EqualityComparer<TValue>.Default;

		public int Count { get; protected internal set; }
	
		protected internal TNode Head { get; set; }

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <inheritdoc />
		public IEnumerator<TValue> GetEnumerator() { return Enumerate(Head); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TNode root)
		{
			return new Enumerator(this, root);
		}

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback)
		{
			if (Count == 0) return;

			int version = _version;
			// Head-Next-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			// Start at the root
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				TNode current = queue.Dequeue();
				visitCallback(current);

				// Queue the next nodes
				if (current.Next != null && !ReferenceEquals(current.Next, root))
				{
					/*
					* The previous/next nodes are not exactly a doubly linked list but rather
					* a circular reference so will have to watch out for this.
					*/
					queue.Enqueue(current.Next);
				}

				if (current.Child != null) queue.Enqueue(current.Child);
			}
		}

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			if (root == null) return;

			int version = _version;
			// Head-Next-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			// Start at the root
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				TNode current = queue.Dequeue();
				if (!visitCallback(current)) break;

				// Queue the next nodes
				if (current.Next != null && !ReferenceEquals(current.Next, root))
				{
					/*
					* The previous/next nodes are not exactly a doubly linked list but rather
					* a circular reference so will have to watch out for this.
					*/
					queue.Enqueue(current.Next);
				}

				if (current.Child != null) queue.Enqueue(current.Child);
			}
		}

		public void Add([NotNull] TValue value)
		{
			TNode node = MakeNode(value);
			Head = Merge(Head, node);
			Count++;
			_version++;
		}

		public void Add([NotNull] IEnumerable<TValue> enumerable)
		{
			foreach (TValue item in enumerable)
				Add(item);
		}

		public bool Remove([NotNull] TValue value)
		{
			TNode node = FindByValue(value);
			return node != null && Remove(node);
		}

		public bool Remove([NotNull] TNode node)
		{
			// This is a special implementation of decreaseKey that sets the
			// argument to the minimum value. This is necessary to make generic keys
			// work, since there is no MinimumValue constant for generic types.
			TNode parent = node.Parent;
			
			if (parent != null) 
			{
				Cut(node);
				CascadingCut(parent);
			}

			Head = node;
			ExtractValue();
			return true;
		}

		public void Clear()
		{
			Head = null;
			Count = 0;
			_version++;
		}

		public void DecreaseKey([NotNull] TNode node, [NotNull] TKey newKey)
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");
			if (Compare(node.Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			node.Key = newKey;
			if (node == Head) return;

			TNode parent = node.Parent;

			if (parent != null && Compare(node.Key, parent.Key) < 0)
			{
				Cut(node);
				CascadingCut(parent);
			}

			if (Compare(node.Key, Head.Key) > 0) return;
			Head = node;
			_version++;
		}

		public TValue Value()
		{
			return Head == null
						? default(TValue)
						: Head.Value;
		}

		[NotNull]
		public TValue ExtractValue()
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");

			TNode node = Head;

			if (node.Child != null)
			{
				// Set parent to null for the minimum's children
				foreach (TNode child in node.Children()) 
					child.Parent = null;
			}

			TNode next = Head.Next;
			// Remove Value from root list
			RemoveNodeFromList(node);
			Count--;
			// Merge the children of the minimum node with the root list
			Head = Merge(next, node.Child);

			if (next != null)
			{
				Head = next;
				Consolidate();
			}

			_version++;
			return node.Value;
		}

		public TValue ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++) 
				ExtractValue();

			return Value();
		}

		public bool Contains([NotNull] TValue value)
		{
			return FindByValue(value) != null;
		}

		public TNode FindByKey([NotNull] TKey key)
		{
			if (Head == null) return null;
			TNode node = null;
			Iterate(Head, e =>
			{
				if (Comparer.IsEqual(e.Key, key)) node = e;
				return node == null;
			});
			return node;
		}

		public TNode FindByValue([NotNull] TValue value)
		{
			if (Head == null) return null;
			TNode node = null;
			Iterate(Head, e =>
			{
				if (ValueComparer.Equals(e.Value, value)) node = e;
				return node == null;
			});
			return node;
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(Head, e => array[arrayIndex++] = e.Value);
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is TValue[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			* Catch the obvious case assignment will fail.
			* We can find all possible problems by doing the check though.
			* For example, if the element type of the Array is derived from T,
			* we can't figure out if we can successfully copy the element beforehand.
			*/
			array.Length.ValidateRange(index, Count);

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(TValue);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;
			Iterate(Head, e => objects[index++] = e.Value);
		}

		[NotNull]
		protected abstract TNode MakeNode([NotNull] TValue value);

		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);

		private void RemoveNodeFromList(TNode node)
		{
			if (node == null) return;
			TNode previous = node.Previous;
			TNode next = node.Next;
			if (previous != null) previous.Next = next ?? previous;
			if (next != null) next.Previous = previous ?? next;
			node.Next = node.Previous = node;
		}

		/// <summary>
		/// Merge two lists of nodes together.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private TNode Merge(TNode a, TNode b)
		{
			if (ReferenceEquals(a, b)) return a;
			if (a == null) return b;
			if (b == null) return a;

			TNode aNext = a.Next ?? a;
			a.Next = b.Next ?? b;
			a.Next.Previous = a;
			b.Next = aNext;
			b.Next.Previous = b;
			return Compare(a.Key, b.Key) < 0
						? a
						: b;
		}

		/// <summary>
		/// Cuts the link between a node and its parent, moving the node to the root list.
		/// </summary>
		/// <param name="node">The node being cut.</param>
		private void Cut([NotNull] TNode node)
		{
			TNode parent = node.Parent;
			if (parent == null) return;
			parent.Degree--;
			// null if node.Next = itself
			parent.Child = node.Next;
			RemoveNodeFromList(node);
			Merge(Head, node);
			node.Marked = false;
		}

		/// <summary>
		/// Perform a cascading cut on a node; If it's not marked, mark it, otherwise cut it
		/// and perform a cascading cut on its parent.
		/// </summary>
		/// <param name="node">The node being cut.</param>
		private void CascadingCut([NotNull] TNode node)
		{
			TNode parent = node.Parent;

			while (parent != null)
			{
				if (!node.Marked)
				{
					node.Marked = true;
					break;
				}

				Cut(node);
				node = parent;
				parent = node.Parent;
			}
		}

		/// <summary>
		/// Merge all trees of the same order together until there are no two trees of the same order.
		/// </summary>
		private void Consolidate()
		{
			if (Head == null) return;

			List<TNode> aux = new List<TNode>();
			Queue<TNode> queue = new Queue<TNode>(Head.Forwards());

			while (queue.Count > 0)
			{
				TNode current = queue.Dequeue();

				// fill the list with null to math the node's degree + 1
				while (aux.Count <= current.Degree + 1) 
					aux.Add(null);

				// If there is another node with the same degree, merge it.
				while (aux[current.Degree] != null)
				{
					if (Compare(current.Key, aux[current.Degree].Key) > 0)
					{
						TNode tmp = current;
						current = aux[current.Degree];
						aux[current.Degree] = tmp;
					}

					LinkHeaps(aux[current.Degree], current);
					aux[current.Degree] = null;
					current.Degree++;
				}

				while (aux.Count <= current.Degree + 1) 
					aux.Add(null);

				aux[current.Degree] = current;
			}

			Head = null;

			foreach (TNode node in aux)
			{
				if (node == null) continue;
				// Remove siblings before merging
				node.Next = node.Previous = node;
				Head = Merge(Head, node);
			}
		}

		/// <summary>
		/// Links two heaps of the same order together.
		/// </summary>
		private void LinkHeaps([NotNull] TNode max, [NotNull] TNode min)
		{
			RemoveNodeFromList(max);
			min.Child = Merge(max, min.Child);
			max.Parent = min;
			max.Marked = false;
		}
	}

	[DebuggerTypeProxy(typeof(FibonacciHeap<,>.DebugView))]
	[Serializable]
	public abstract class FibonacciHeap<TKey, TValue> : FibonacciHeap<FibonacciNode<TKey, TValue>, TKey, TValue>
	{
		internal new sealed class DebugView
		{
			private readonly FibonacciHeap<TKey, TValue> _heap;

			public DebugView([NotNull] FibonacciHeap<TKey, TValue> heap) { _heap = heap; }

			[NotNull]
			public FibonacciNode<TKey, TValue> Head => _heap.Head;
		}

		[NotNull]
		protected Func<TValue, TKey> _getKeyForItem;

		/// <inheritdoc />
		protected FibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, (IComparer<TKey>)null)
		{
		}

		protected FibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(comparer)
		{
			_getKeyForItem = getKeyForItem;
		}

		protected FibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		protected FibonacciHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(getKeyForItem, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		protected override FibonacciNode<TKey, TValue> MakeNode(TValue value) { return new FibonacciNode<TKey, TValue>(_getKeyForItem(value), value); }
	}

	[DebuggerTypeProxy(typeof(FibonacciHeap<>.DebugView))]
	[Serializable]
	public abstract class FibonacciHeap<T> : FibonacciHeap<FibonacciNode<T>, T, T>
	{
		internal new sealed class DebugView
		{
			private readonly FibonacciHeap<T> _heap;

			public DebugView([NotNull] FibonacciHeap<T> heap) { _heap = heap; }

			[NotNull]
			public FibonacciNode<T> Head => _heap.Head;
		}

		/// <inheritdoc />
		protected FibonacciHeap()
			: this((IComparer<T>)null)
		{
		}

		protected FibonacciHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected FibonacciHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected FibonacciHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override FibonacciNode<T> MakeNode(T value) { return new FibonacciNode<T>(value); }
	}

	public static class FibonacciHeapExtension
	{
		// todo https://www.geeksforgeeks.org/left-child-right-sibling-representation-tree/
		public static void WriteTo<TNode, TKey, TValue>([NotNull] this FibonacciHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : FibonacciNode<TNode, TKey, TValue>
		{
			const string STR_BLANK = "   ";
			const string STR_EXT = "│  ";
			const string STR_CONNECTOR_L = "└─ ";
			const string STR_NULL = "<null>";

			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			LinkedList<(Queue<TNode> Nodes, int Level)> nodesStack = new LinkedList<(Queue<TNode> Nodes, int Level)>();
			Queue<TNode> rootQueue = new Queue<TNode>(1);
			rootQueue.Enqueue(thisValue.Head);
			nodesStack.AddFirst((rootQueue, 0));

			while (nodesStack.Count > 0)
			{
				(Queue<TNode> nodes, int level) = nodesStack.Last.Value;

				if (nodes.Count == 0)
				{
					nodesStack.RemoveLast();
					continue;
				}

				TNode node = nodes.Dequeue();
				indent.Length = 0;

				foreach ((Queue<TNode> Nodes, int Level) tuple in nodesStack)
				{
					if (tuple == nodesStack.Last.Value) break;
					indent.Append(tuple.Nodes.Count > 0
									? STR_EXT
									: STR_BLANK);
				}

				writer.Write(indent + STR_CONNECTOR_L);

				if (node == null)
				{
					writer.WriteLine(STR_NULL);
					continue;
				}

				writer.WriteLine(node.ToString(level));
				if (node.IsLeaf) continue;

				Queue<TNode> queue = new Queue<TNode>(2);

				// Queue the next nodes.
				if (node.Next != null && !ReferenceEquals(node.Next, thisValue.Head))
				{
					/*
					* The previous/next nodes are not exactly a doubly linked list but rather
					* a circular reference so will have to watch out for this.
					*/
					queue.Enqueue(node.Next);
				}

				if (node.Child != null) queue.Enqueue(node.Child);
				nodesStack.AddLast((queue, level + 1));
			}
		}
	}
}
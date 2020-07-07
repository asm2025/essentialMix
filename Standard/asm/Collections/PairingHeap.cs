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
	/// <see href="https://en.wikipedia.org/wiki/Pairing_heap">Pairing heap</see> are heap-ordered multi-way tree structures, and can
	/// be considered simplified Fibonacci heaps. They are considered a "robust choice" for implementing such algorithms as Prim's MST
	/// algorithm because they have fast running time for their operations. They are modification of Pairing Heaps.
	/// </summary>
	/// <typeparam name="TNode">The node type. This is just for abstraction purposes and shouldn't be dealt with directly.</typeparam>
	/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
	/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
	/// priority of the item.</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	// https://en.wikipedia.org/wiki/Pairing_heap
	// https://brilliant.org/wiki/pairing-heap/
	// https://users.cs.fiu.edu/~weiss/dsaa_c++/code/PairingHeap.cpp <= actually nice one :)
	// https://iq.opengenus.org/pairing-heap/
	// https://www.sanfoundry.com/cpp-program-implement-pairing-heap/
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(PairingHeap<,,>.DebugView))]
	[Serializable]
	public abstract class PairingHeap<TNode, TKey, TValue> : IKeyedHeap<TNode, TKey, TValue>, ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
		where TNode : PairingNode<TNode, TKey, TValue>
	{
		internal sealed class DebugView
		{
			private readonly PairingHeap<TNode, TKey, TValue> _heap;

			public DebugView([NotNull] PairingHeap<TNode, TKey, TValue> heap)
			{
				_heap = heap;
			}

			[NotNull]
			public TNode Head => _heap.Head;
		}

		private struct BreadthFirstEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly PairingHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] PairingHeap<TNode, TKey, TValue> heap, TNode root)
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
				// Head-Sibling-Child (Queue)
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

				// Queue the next nodes
				if (_current.Sibling != null) _queue.Enqueue(_current.Sibling);
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
		protected PairingHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected PairingHeap(IComparer<TKey> comparer)
		{
			Comparer = comparer ?? Comparer<TKey>.Default;
		}

		protected PairingHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected PairingHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		public IComparer<TKey> Comparer { get; }

		[NotNull]
		protected EqualityComparer<TValue> ValueComparer { get; } = EqualityComparer<TValue>.Default;

		public int Count { get; protected internal set; }
	
		protected internal TNode Head { get; set; }

		/// <inheritdoc />
		bool ICollection<TValue>.IsReadOnly => false;

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
			return new BreadthFirstEnumerator(this, root);
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
			// Head-Sibling-Child (Queue)
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
				if (current.Sibling != null) queue.Enqueue(current.Sibling);
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
			// Head-Sibling-Child (Queue)
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
				if (current.Sibling != null) queue.Enqueue(current.Sibling);
				if (current.Child != null) queue.Enqueue(current.Child);
			}
		}

		/// <inheritdoc />
		public abstract TNode MakeNode(TValue value);

		/// <inheritdoc />
		public void Add(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			Add(MakeNode(value));
		}

		/// <inheritdoc />
		public void Add(TNode node)
		{
			node.Child = node.Sibling = node.Previous = null;
			Head = Head == null
						? node
						: Meld(Head, node);
			Count++;
			_version++;
		}

		/// <inheritdoc />
		public void Add(IEnumerable<TValue> enumerable)
		{
			foreach (TValue item in enumerable)
				Add(item);
		}

		/// <inheritdoc />
		public bool Remove(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			TNode node = Find(value);
			return node != null && Remove(node);
		}

		/// <inheritdoc />
		public bool Remove(TNode node)
		{
			// https://www.geeksforgeeks.org/pairing-heap/
			// https://brilliant.org/wiki/pairing-heap/
			TNode mergedLeftMost = TwoPassMerge(node.LeftMostChild());
			Head = node == Head
						? mergedLeftMost
						: Meld(Head, mergedLeftMost);
			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Head = null;
			Count = 0;
			_version++;
		}

		/// <inheritdoc />
		public void DecreaseKey(TNode node, TKey newKey)
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");
			if (Compare(node.Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			node.Key = newKey;
			if (node == Head) return;
			if (node.Sibling != null) node.Sibling.Previous = node.Previous;
			
			if (node.Previous != null)
			{
				if (node.Previous.Child == node) node.Previous.Child = node.Sibling;
				else node.Previous.Sibling = node.Sibling;
			}

			node.Sibling = null;
			Head = Meld(Head, node);
			_version++;
		}

		/// <inheritdoc />
		public TValue Value()
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");
			return Head.Value;
		}

		/// <inheritdoc />
		public TValue ExtractValue()
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");
			TNode oldRoot = Head;
			Head = Head.Child == null
						? null
						: TwoPassMerge(Head.Child);
			Count--;
			_version++;
			return oldRoot.Value;
		}

		/// <inheritdoc />
		public TValue ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++) 
				ExtractValue();

			return Value();
		}

		/// <inheritdoc />
		public bool Contains(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return Find(value) != null;
		}

		/// <inheritdoc />
		public TNode Find(TValue value)
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

		/// <inheritdoc />
		public TNode FindByKey(TKey key)
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

		public virtual bool Equals(PairingHeap<TNode, TKey, TValue> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Count != other.Count || !ValueComparer.Equals(other.ValueComparer)) return false;

			using (IEnumerator<TValue> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<TValue> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();
					
					while (thisMoved && otherMoved)
					{
						if (!ValueComparer.Equals(thisEnumerator.Current, otherEnumerator.Current)) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
		}

		/// <inheritdoc />
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

		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);

		/// <summary>
		/// Maintains heap properties by comparing and linking a and b together to satisfy heap order.
		/// </summary>
		/// <param name="a">The first node. Usually the Head node.</param>
		/// <param name="b">The second node.</param>
		/// <returns>The merged node. The value returned should be assigned back to whichever node was passed as the first node parameter.</returns>
		private TNode Meld(TNode a, TNode b)
		{
			if (ReferenceEquals(a, b)) return a;
			if (a == null) return b;
			if (b == null) return a;

			if (Compare(b.Key, a.Key) < 0)
			{
				b.Previous = a.Previous;
				a.Previous = b;
				a.Sibling = b.Child;
				if (a.Sibling != null) a.Sibling.Previous = a;
				b.Child = a;
				return b;
			}

			b.Previous = a;
			a.Sibling = b.Sibling;
			if (a.Sibling != null) a.Sibling.Previous = a;
			b.Sibling = a.Child;
			if (b.Sibling != null) b.Sibling.Previous = b;
			a.Child = b;
			return a;
		}

		/// <summary>
		/// Implements two-pass merging. It is usually used to delete the root node and combine the siblings.
		/// </summary>
		/// <param name="node"></param>
		/// <returns>The new root node</returns>
		private TNode TwoPassMerge(TNode node)
		{
			if (node?.Sibling == null) return node;

			List<TNode> nodes = new List<TNode>();
			TNode next = node;

			do
			{
				nodes.Add(next);
				next.Previous.Sibling = null;
				next = next.Sibling;
			}
			while (next != null);

			nodes.Add(null);

			int i = 0;
			/*
			 * Combine subtrees two at a time, going left to right.
			 * The 1st item will keep the merge result.
			 */
			while (i + 1 < nodes.Count)
			{
				nodes[i] = Meld(nodes[i], nodes[i + 1]);
				i += 2;
			}

			// i has the result of last merge.
			i -= 2;
			// If an odd number of nodes, get the last one.
			if (i == nodes.Count - 3) nodes[i] = Meld(nodes[i], nodes[i + 2]);

			/*
			 * Now go right to left, merging last node with next to last.
			 * The result becomes the new last.
			 */
			while (i >= 2)
			{
				nodes[i - 2] = Meld(nodes[i - 2], nodes[i]);
				i -= 2;
			}

			return nodes[0];
		}
	}

	[DebuggerTypeProxy(typeof(PairingHeap<,>.DebugView))]
	[Serializable]
	public abstract class PairingHeap<TKey, TValue> : PairingHeap<PairingNode<TKey, TValue>, TKey, TValue>
	{
		internal new sealed class DebugView
		{
			private readonly PairingHeap<TKey, TValue> _heap;

			public DebugView([NotNull] PairingHeap<TKey, TValue> heap) { _heap = heap; }

			[NotNull]
			public PairingNode<TKey, TValue> Head => _heap.Head;
		}

		[NotNull]
		protected Func<TValue, TKey> _getKeyForItem;

		/// <inheritdoc />
		protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, (IComparer<TKey>)null)
		{
		}

		protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(comparer)
		{
			_getKeyForItem = getKeyForItem;
		}

		protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(getKeyForItem, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public override PairingNode<TKey, TValue> MakeNode(TValue value) { return new PairingNode<TKey, TValue>(_getKeyForItem(value), value); }
	}

	[DebuggerTypeProxy(typeof(PairingHeap<>.DebugView))]
	[Serializable]
	public abstract class PairingHeap<T> : PairingHeap<PairingNode<T>, T, T>
	{
		internal new sealed class DebugView
		{
			private readonly PairingHeap<T> _heap;

			public DebugView([NotNull] PairingHeap<T> heap) { _heap = heap; }

			[NotNull]
			public PairingNode<T> Head => _heap.Head;
		}

		/// <inheritdoc />
		protected PairingHeap()
			: this((IComparer<T>)null)
		{
		}

		protected PairingHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected PairingHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected PairingHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public override PairingNode<T> MakeNode(T value) { return new PairingNode<T>(value); }
	}

	public static class PairingHeapExtension
	{
		public static void WriteTo<TNode, TKey, TValue>([NotNull] this PairingHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : PairingNode<TNode, TKey, TValue>
		{
			const string STR_BLANK = "   ";
			const string STR_EXT = "│  ";
			const string STR_CONNECTOR_L = "└─ ";
			const string STR_CONNECTOR_C = " ► ";
			const string STR_NULL = "<null>";

			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			LinkedList<(Queue<TNode> Nodes, int Level)> nodesList = new LinkedList<(Queue<TNode> Nodes, int Level)>();
			Queue<TNode> rootQueue = new Queue<TNode>(1);
			rootQueue.Enqueue(thisValue.Head);
			nodesList.AddFirst((rootQueue, 0));

			while (nodesList.Count > 0)
			{
				(Queue<TNode> nodes, int level) = nodesList.Last.Value;

				if (nodes.Count == 0)
				{
					nodesList.RemoveLast();
					continue;
				}

				TNode node = nodes.Dequeue();
				indent.Length = 0;

				foreach ((Queue<TNode> Nodes, int Level) tuple in nodesList)
				{
					if (tuple == nodesList.Last.Value) break;
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

				if (nodes.Count == 0 && node.Child != null)
				{
					writer.Write(node.ToString(level));

					// this is a child. so will print the children list
					foreach (TNode child in node.Children())
					{
						writer.Write(STR_CONNECTOR_C);
						writer.Write(child.ToString());
					}

					writer.WriteLine();
				}
				else
				{
					writer.WriteLine(node.ToString(level));
				}

				if (node.IsLeaf) continue;

				Queue<TNode> queue = new Queue<TNode>(2);
				queue.Enqueue(node.Sibling);
				queue.Enqueue(node.Child);
				nodesList.AddLast((queue, level + 1));
			}
		}
	}
}
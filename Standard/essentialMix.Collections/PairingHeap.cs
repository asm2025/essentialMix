using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections
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
	[Serializable]
	public abstract class PairingHeap<TNode, TKey, TValue> : RootedHeap<TNode, TKey, TValue>
		where TNode : PairingNode<TNode, TKey, TValue>
	{
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
								: _current?.Sibling;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Child == null) return true;
				_queue.Enqueue(_current.Child);
				if (_current.Child.Sibling == null) return true;

				foreach (TNode sibling in _current.Child.Siblings())
					_queue.Enqueue(sibling);

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

		private struct DepthFirstEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly PairingHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] PairingHeap<TNode, TKey, TValue> heap, TNode root)
			{
				_heap = heap;
				_version = _heap._version;
				_root = root;
				_stack = new Stack<TNode>();
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
				// Head-Child-Sibling (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_stack.Push(_root);
				}

				// visit the next queued node
				_current = _stack.Count > 0
								? _stack.Pop()
								: _current?.Sibling;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Child == null) return true;
				_stack.Push(_current.Child);
				if (_current.Child.Sibling == null) return true;

				foreach (TNode sibling in _current.Child.Siblings())
					_stack.Push(sibling);

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		/// <inheritdoc />
		protected PairingHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected PairingHeap(IComparer<TKey> comparer)
			: base(comparer)
		{
		}

		protected PairingHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected PairingHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override IEnumerableEnumerator<TValue> Enumerate(TNode root, BreadthDepthTraversal method)
		{
			return method switch
			{
				BreadthDepthTraversal.BreadthFirst => new BreadthFirstEnumerator(this, root),
				BreadthDepthTraversal.DepthFirst => new DepthFirstEnumerator(this, root),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		/// <inheritdoc />
		public sealed override void Iterate(TNode root, BreadthDepthTraversal method, Action<TNode> visitCallback)
		{
			if (Count == 0 || root == null) return;

			switch (method)
			{
				case BreadthDepthTraversal.BreadthFirst:
					BreadthFirst(root, visitCallback);
					break;
				case BreadthDepthTraversal.DepthFirst:
					DepthFirst(root, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}
		}

		/// <inheritdoc />
		public sealed override void Iterate(TNode root, BreadthDepthTraversal method, Func<TNode, bool> visitCallback)
		{
			if (Count == 0 || root == null) return;

			switch (method)
			{
				case BreadthDepthTraversal.BreadthFirst:
					BreadthFirst(root, visitCallback);
					break;
				case BreadthDepthTraversal.DepthFirst:
					DepthFirst(root, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}
		}

		/// <inheritdoc />
		public sealed override TNode Add(TNode node)
		{
			node.Invalidate();
			Head = Head == null
						? node
						: Meld(Head, node);
			Count++;
			_version++;
			return node;
		}

		/// <inheritdoc />
		public sealed override bool Remove(TNode node)
		{
			// https://www.geeksforgeeks.org/pairing-heap/
			// https://brilliant.org/wiki/pairing-heap/
			TNode mergedLeftMost = TwoPassMerge(node.LeftMostChild());
			Head = ReferenceEquals(node, Head)
						? mergedLeftMost
						: Meld(Head, mergedLeftMost);
			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public sealed override void Clear()
		{
			Head = null;
			Count = 0;
			_version++;
		}

		/// <inheritdoc />
		public sealed override void DecreaseKey(TNode node, TKey newKey)
		{
			if (Head == null) throw new CollectionIsEmptyException();
			if (Compare(node.Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			node.Key = newKey;
			if (ReferenceEquals(node, Head)) return;
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
		public sealed override TValue Value()
		{
			if (Head == null) throw new CollectionIsEmptyException();
			return Head.Value;
		}

		/// <inheritdoc />
		public sealed override TNode ExtractValue()
		{
			if (Head == null) throw new CollectionIsEmptyException();
			TNode node = Head;
			Head = Head.Child == null
						? null
						: TwoPassMerge(Head.Child);
			Count--;
			_version++;
			node.Invalidate();
			return node;
		}

		/// <summary>
		/// Maintains heap properties by comparing and linking a and b together to satisfy heap order.
		/// x.Sibling MUST be NULL on entry.
		/// </summary>
		/// <param name="x">The first node. Usually the Head node.</param>
		/// <param name="y">The second node.</param>
		/// <returns>The merged node. The value returned should be assigned back to whichever node was passed as the first node parameter.</returns>
		private TNode Meld(TNode x, TNode y)
		{
			if (ReferenceEquals(x, y)) return x;
			if (x == null) return y;
			if (y == null) return x;

			if (Compare(y.Key, x.Key) < 0)
			{
				y.Previous = x.Previous;
				x.Previous = y;
				x.Sibling = y.Child;
				if (x.Sibling != null) x.Sibling.Previous = x;
				y.Child = x;
				return y;
			}

			y.Previous = x;
			x.Sibling = y.Sibling;
			if (x.Sibling != null) x.Sibling.Previous = x;
			y.Sibling = x.Child;
			if (y.Sibling != null) y.Sibling.Previous = y;
			x.Child = y;
			return x;
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

			do
			{
				nodes.Add(node);
				// break links
				node.Previous.Sibling = null;
				node = node.Sibling;
			}
			while (node != null);

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

		#region Iterator Traversal for Action<TNode>
		private void BreadthFirst([NotNull] TNode root, [NotNull] Action<TNode> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				queue.Enqueue(root);

				while (queue.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = queue.Dequeue();
					visitCallback(current);
					if (current.Child == null) continue;

					// Queue the next nodes
					queue.Enqueue(current.Child);
					if (current.Child.Sibling == null) continue;

					foreach (TNode sibling in current.Child.Siblings())
						queue.Enqueue(sibling);
				}

				root = root.Sibling;
			}
		}

		private void DepthFirst([NotNull] TNode root, [NotNull] Action<TNode> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Stack)
			Stack<TNode> stack = new Stack<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				stack.Push(root);

				while (stack.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = stack.Pop();
					visitCallback(current);

					// Queue the next nodes
					if (current.Child == null) continue;
					if (current.Child.Sibling != null) stack.Push(current.Child.Sibling);

					foreach (TNode child in current.Child.Children())
						stack.Push(child);
				}

				root = root.Sibling;
			}
		}
		#endregion

		#region Iterator Traversal for Func<TNode, bool>
		private void BreadthFirst([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				queue.Enqueue(root);

				while (queue.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = queue.Dequeue();
					if (!visitCallback(current)) return;
					if (current.Child == null) continue;

					// Queue the next nodes
					queue.Enqueue(current.Child);
					if (current.Child.Sibling == null) continue;

					foreach (TNode sibling in current.Child.Siblings())
						queue.Enqueue(sibling);
				}

				root = root.Sibling;
			}
		}

		private void DepthFirst([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			int version = _version;
			// Head-Child-Sibling (Stack)
			Stack<TNode> stack = new Stack<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				stack.Push(root);

				while (stack.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = stack.Pop();
					if (!visitCallback(current)) return;

					// Queue the next nodes
					if (current.Child == null) continue;
					if (current.Child.Sibling != null) stack.Push(current.Child.Sibling);

					foreach (TNode child in current.Child.Children())
						stack.Push(child);
				}

				root = root.Sibling;
			}
		}
		#endregion
	}

	[DebuggerTypeProxy(typeof(PairingHeap<,>.DebugView))]
	[Serializable]
	public abstract class PairingHeap<TKey, TValue> : PairingHeap<PairingNode<TKey, TValue>, TKey, TValue>
	{
		internal sealed class DebugView : Dbg_RootedHeapDebugView<PairingNode<TKey, TValue>, TKey, TValue>
		{
			/// <inheritdoc />
			public DebugView([NotNull] PairingHeap<TKey, TValue> heap)
				: base(heap)
			{
			}
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
		internal sealed class DebugView : Dbg_RootedHeapDebugView<PairingNode<T>, T, T>
		{
			/// <inheritdoc />
			public DebugView([NotNull] PairingHeap<T> heap)
				: base(heap)
			{
			}
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
			const string STR_CONNECTOR_R = " ► ";
			const string STR_CONNECTOR_C = "└─ ";
			const string STR_NULL = "<null>";

			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			LinkedList<(Queue<TNode> Nodes, int Level)> nodesList = new LinkedList<(Queue<TNode> Nodes, int Level)>();
			TNode root = thisValue.Head;

			while (root != null)
			{
				Queue<TNode> queue = new Queue<TNode>(1);
				queue.Enqueue(root);
				nodesList.AddFirst((queue, 0));

				while (nodesList.Count > 0)
				{
					(Queue<TNode> nodes, int level) = nodesList.Last.Value;

					if (nodes.Count == 0)
					{
						nodesList.RemoveLast();
						continue;
					}

					TNode node = nodes.Dequeue();

					if (level == 0)
					{
						writer.WriteLine(STR_CONNECTOR_R + node.ToString(level));
					}
					else
					{
						indent.Length = 0;

						foreach ((Queue<TNode> Nodes, int Level) tuple in nodesList)
						{
							if (tuple == nodesList.Last.Value) break;
							indent.Append(tuple.Nodes.Count > 0
											? STR_EXT
											: STR_BLANK);
						}

						writer.Write(indent + STR_CONNECTOR_C);

						if (node == null)
						{
							writer.WriteLine(STR_NULL);
							continue;
						}

						writer.WriteLine(node.ToString(level));
					}

					if (node.Child == null) continue;

					// Queue the next nodes.
					queue = new Queue<TNode>();
					queue.Enqueue(node.Child);

					if (node.Child.Sibling != null)
					{
						foreach (TNode sibling in node.Child.Siblings())
							queue.Enqueue(sibling);
					}

					nodesList.AddLast((queue, level + 1));
				}

				root = root.Sibling;
			}
		}
	}
}
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
	/// <see href="https://en.wikipedia.org/wiki/Binomial_heap">Binomial heap</see> using the linked representation.
	/// It is a data structure that acts as a priority queue but also allows pairs of heaps to be merged together.
	/// It is implemented as a heap similar to a binary heap but using a special tree structure that is different
	/// from the complete binary trees used by binary heaps.
	/// </summary>
	/// <typeparam name="TNode">The node type. This is just for abstraction purposes and shouldn't be dealt with directly.</typeparam>
	/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
	/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
	/// priority of the item.</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	// https://brilliant.org/wiki/binomial-heap/
	/*
	 * https://algorithmtutor.com/Data-Structures/Tree/Binomial-Heaps/ << good (after fixing a couple of bugs - extractMin and merge).
	 * OK, extractMin has a weird implementation and I'm not sure it's working essentially!
	 */
	// https://gist.github.com/chinchila/81a4c9bfd852e775f2bdf68339d212a2 << good. actually this one is better and simpler.
	// the rest, no matter what site it is, has some issues after test, not stable or utter garbage!
	
	/*
	 * And then I found https://keithschwarz.com/interesting/code/?dir=binomial-heap from Keith Schwarz a.k.a templatetypedef
	 * @stackOverflow. It contains some useful and dense explanation everywhere. He seems to be an interesting lecturer at Stanford as
	 * well with a bunch of interesting code.
	 * His implementation has a different style which does not use a degree or parent node pointer per each node, which is cool because it
	 * enhances the structure in terms of space required to store the nodes. Unfortunately, It does not have a Head/Root node but rather a
	 * trees list and does not implement a few essential functions such as remove a node or DecreaseKey!
	 * So, Maybe will try it some other time.
	 */
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(BinomialHeap<,,>.DebugView))]
	[Serializable]
	public abstract class BinomialHeap<TNode, TKey, TValue> : IKeyedHeap<TNode, TKey, TValue>, ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
		where TNode : BinomialNode<TNode, TKey, TValue>
	{
		internal sealed class DebugView
		{
			private readonly BinomialHeap<TNode, TKey, TValue> _heap;

			public DebugView([NotNull] BinomialHeap<TNode, TKey, TValue> heap)
			{
				_heap = heap;
			}

			[NotNull]
			public TNode Head => _heap.Head;
		}

		private struct BreadthFirstEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly BinomialHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] BinomialHeap<TNode, TKey, TValue> heap, TNode root)
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
			private readonly BinomialHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] BinomialHeap<TNode, TKey, TValue> heap, TNode root)
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

		protected internal int _version;

		private object _syncRoot;

		/// <inheritdoc />
		protected BinomialHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected BinomialHeap(IComparer<TKey> comparer)
		{
			Comparer = comparer ?? Comparer<TKey>.Default;
		}

		protected BinomialHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected BinomialHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
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
		/// <param name="method">The technique to use when traversing</param>
		/// <returns>An <see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TNode root, BreadthDepthTraverse method)
		{
			return method switch
			{
				BreadthDepthTraverse.BreadthFirst => new BreadthFirstEnumerator(this, root),
				BreadthDepthTraverse.DepthFirst => new DepthFirstEnumerator(this, root),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TNode root) { return Enumerate(root, BreadthDepthTraverse.BreadthFirst); }
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(TNode root, BreadthDepthTraverse method, [NotNull] Action<TNode> visitCallback)
		{
			if (Count == 0 || root == null) return;

			switch (method)
			{
				case BreadthDepthTraverse.BreadthFirst:
					BreadthFirst(root, visitCallback);
					break;
				case BreadthDepthTraverse.DepthFirst:
					DepthFirst(root, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}
		}

		#region Iterate overloads - visitCallback action
		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, BreadthDepthTraverse.BreadthFirst, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The technique to use when traversing</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(TNode root, BreadthDepthTraverse method, [NotNull] Func<TNode, bool> visitCallback)
		{
			if (Count == 0 || root == null) return;

			switch (method)
			{
				case BreadthDepthTraverse.BreadthFirst:
					BreadthFirst(root, visitCallback);
					break;
				case BreadthDepthTraverse.DepthFirst:
					DepthFirst(root, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}
		}

		#region Iterate overloads - visitCallback action
		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, BreadthDepthTraverse.BreadthFirst, visitCallback);
		}
		#endregion

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
			node.Invalidate();
			Head = Head == null
						? node
						: Union(Head, node);
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
			BubbleUp(node, true);
			ExtractValue();
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
			BubbleUp(node);
		}

		/// <inheritdoc />
		public TValue Value()
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");

			TNode node = Head;
			if (node.Sibling == null) return node.Value;

			foreach (TNode sibling in node.Siblings())
			{
				if (Compare(sibling.Key, node.Key) >= 0) continue;
				node = sibling;
			}

			return node.Value;
		}

		/// <inheritdoc />
		public TValue ExtractValue()
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");

			TNode minPrev = null
				, min = Head
				, nextPrev = min
				, next = min.Sibling;

			// search root nodes for the value (min/max)
			while (next != null)
			{
				if (Compare(next.Key, min.Key) < 0)
				{
					minPrev = nextPrev;
					min = next;
				}

				nextPrev = next;
				next = next.Sibling;
			}

			if (Head == min) Head = min.Sibling;
			else if (minPrev != null) minPrev.Sibling = min.Sibling;
			
			TNode head = null;
			TNode child = min.Child;

			while (child != null)
			{
				next = child.Sibling;
				child.Sibling = head;
				child.Parent = null;
				head = child;
				child = next;
			}

			Head = Union(Head, head);
			Count--;
			_version++;
			return min.Value;
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
		public TNode Find(TValue value)
		{
			if (Head == null || ValueComparer.Equals(Head.Value, value)) return Head;
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
			if (Head == null || Comparer.IsEqual(Head.Key, key)) return Head;
			TNode node = null;
			Iterate(Head, e =>
			{
				if (Comparer.IsEqual(e.Key, key)) node = e;
				return node == null;
			});
			return node;
		}

		public virtual bool Equals(BinomialHeap<TNode, TKey, TValue> other)
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

		/// <inheritdoc />
		public bool Contains(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return Find(value) != null;
		}

		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);

		[NotNull]
		private TNode BubbleUp([NotNull] TNode node, bool toRoot = false)
		{
			if (node == Head) return node;

			TNode parent = node.Parent;

			while (parent != null && (toRoot || Compare(node.Key, parent.Key) < 0))
			{
				node.Swap(parent);
				node = parent;
				parent = node.Parent;
			}

			return node;
		}

		private void Link([NotNull] TNode x, [NotNull] TNode y)
		{
			y.Parent = x;
			y.Sibling = x.Child;
			x.Child = y;
			x.Degree++;
		}

		private TNode Merge(TNode x, TNode y)
		{
			if (ReferenceEquals(x, y)) return x;
			if (x == null) return y;
			if (y == null) return x;

			TNode head;

			if (x.Degree <= y.Degree)
			{
				head = x;
				x = x.Sibling;
			}
			else
			{
				head = y;
				y = y.Sibling;
			}

			TNode tail = head;

			/*
			 * merge two heaps without taking care of trees with same degree the roots of the tree must be in
			 * ascending order of degree from left to right.
			 */
			while (x != null && y != null)
			{
				if (x.Degree <= y.Degree)
				{
					tail.Sibling = x;
					x = x.Sibling;
				}
				else
				{
					tail.Sibling = y;
					y = y.Sibling;
				}

				tail = tail.Sibling;
			}

			tail.Sibling = x ?? y;
			return head;
		}

		private TNode Union(TNode x, TNode y)
		{
			TNode head = Merge(x, y);
			if (head == null) return null;

			TNode prev = null
				, node = head
				, next = node.Sibling;

			// scan the merged list and merge binomial trees with same degree
			while (next != null)
			{
				/*
				 * if two adjacent tree roots have different degree or 3 consecutive roots
				 * have same degree move to the next tree.
				 */
				if (node.Degree != next.Degree || next.Sibling != null && node.Degree == next.Sibling.Degree)
				{
					prev = node;
					node = next;
				}
				else
				{
					// otherwise merge binomial trees with same degree
					if (Compare(node.Key, next.Key) < 0)
					{
						node.Sibling = next.Sibling;
						Link(node, next);
					}
					else
					{
						if (prev == null) head = next;
						else prev.Sibling = next;

						Link(next, node);
						node = next;
					}
				}

				next = node.Sibling;
			}

			return head;
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
					if (current.Child == null) continue;

					// Queue the next nodes
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
					if (current.Child == null) continue;

					// Queue the next nodes
					if (current.Child.Sibling != null) stack.Push(current.Child.Sibling);

					foreach (TNode child in current.Child.Children())
						stack.Push(child);
				}

				root = root.Sibling;
			}
		}
		#endregion
	}

	[DebuggerTypeProxy(typeof(BinomialHeap<,>.DebugView))]
	[Serializable]
	public abstract class BinomialHeap<TKey, TValue> : BinomialHeap<BinomialNode<TKey, TValue>, TKey, TValue>
	{
		internal new sealed class DebugView
		{
			private readonly BinomialHeap<TKey, TValue> _heap;

			public DebugView([NotNull] BinomialHeap<TKey, TValue> heap) { _heap = heap; }

			[NotNull]
			public BinomialNode<TKey, TValue> Head => _heap.Head;
		}

		[NotNull]
		protected Func<TValue, TKey> _getKeyForItem;

		/// <inheritdoc />
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, (IComparer<TKey>)null)
		{
		}

		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: base(comparer)
		{
			_getKeyForItem = getKeyForItem;
		}

		/// <inheritdoc />
		internal BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] BinomialNode<TKey, TValue> head)
			: this(getKeyForItem, head, null)
		{
		}

		/// <inheritdoc />
		internal BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] BinomialNode<TKey, TValue> head, IComparer<TKey> comparer)
			: this(getKeyForItem, comparer)
		{
			Head = head;
			if (Head == null) return;
			Count += Head.Degree + 1;
			if (Head.IsLeaf) return;

			foreach (BinomialNode<TKey, TValue> sibling in Head.Siblings())
				Count += sibling.Degree + 1;
		}

		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(getKeyForItem, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public override BinomialNode<TKey, TValue> MakeNode(TValue value) { return new BinomialNode<TKey, TValue>(_getKeyForItem(value), value); }
	}

	[DebuggerTypeProxy(typeof(BinomialHeap<>.DebugView))]
	[Serializable]
	public abstract class BinomialHeap<T> : BinomialHeap<BinomialNode<T>, T, T>
	{
		internal new sealed class DebugView
		{
			private readonly BinomialHeap<T> _heap;

			public DebugView([NotNull] BinomialHeap<T> heap) { _heap = heap; }

			[NotNull]
			public BinomialNode<T> Head => _heap.Head;
		}

		/// <inheritdoc />
		protected BinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		protected BinomialHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected BinomialHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected BinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public override BinomialNode<T> MakeNode(T value) { return new BinomialNode<T>(value); }
	}

	public static class BinomialHeapExtension
	{
		public static void WriteTo<TNode, TKey, TValue>([NotNull] this BinomialHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : BinomialNode<TNode, TKey, TValue>
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

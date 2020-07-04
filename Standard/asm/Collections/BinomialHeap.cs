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
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	// https://www.growingwiththeweb.com/data-structures/binomial-heap/overview/
	// https://brilliant.org/wiki/binomial-heap/
	// IMPORTANT: the count property will need more work todo make sure the count is right when using methods such as Union, merge, etc.
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(BinomialHeap<,,>.DebugView))]
	[Serializable]
	public abstract class BinomialHeap<TNode, TKey, TValue> : IReadOnlyCollection<TValue>, ICollection
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

		private struct Enumerator : IEnumerableEnumerator<TValue>
		{
			private readonly BinomialHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] BinomialHeap<TNode, TKey, TValue> heap, TNode root)
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
		protected BinomialHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected BinomialHeap(IComparer<TKey> comparer)
		{
			Comparer = comparer ?? Comparer<TKey>.Default;
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] TNode head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] TNode head, IComparer<TKey> comparer)
			: this(comparer)
		{
			Head = head;
			if (Head == null) return;
			Count += Head.Degree + 1;
			if (Head.IsLeaf) return;

			foreach (TNode sibling in Head.Siblings())
				Count += sibling.Degree + 1;
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
		/// <param name="head">The starting node</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TNode head)
		{
			return new Enumerator(this, head);
		}

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="head">The starting node</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(TNode head, [NotNull] Action<TNode> visitCallback)
		{
			if (Count == 0) return;

			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			// Start at the root
			queue.Enqueue(head);

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
		/// <param name="head">The starting node</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(TNode head, [NotNull] Func<TNode, bool> visitCallback)
		{
			if (head == null) return;

			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			// Start at the root
			queue.Enqueue(head);

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

		public void Add([NotNull] TValue value)
		{
			TNode node = MakeNode(value);
			Head = Union(MakeHeap(node));
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
			TNode node = Find(value);
			return node != null && Remove(node);
		}

		public bool Remove([NotNull] TNode node)
		{
			node = BubbleUp(node, true);
			return RemoveRoot(node, GetPrevious(node));
		}

		public void Clear()
		{
			Head = null;
			Count = 0;
			_version++;
		}

		public void DecreaseKey([NotNull] TNode node, [NotNull] TKey newKey)
		{
			node.Key = newKey;
			BubbleUp(node);
		}

		public TValue Value()
		{
			if (Head == null) return default(TValue);

			TNode node = Head, next = Head.Sibling;

			while (next != null)
			{
				if (Compare(next, node) < 0) node = next;
				next = next.Sibling;
			}

			return node.Value;
		}

		[NotNull]
		public TValue ExtractValue()
		{
			if (Head == null) throw new InvalidOperationException("Heap is empty.");

			TNode node = Head
				, nodePrev = null
				, next = node.Sibling
				, nextPrev = node;

			while (next != null)
			{
				if (Compare(next, node) < 0)
				{
					node = next;
					nodePrev = nextPrev;
				}

				nextPrev = next;
				next = next.Sibling;
			}

			RemoveRoot(node, nodePrev);
			return node.Value;
		}

		public TValue ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++) 
				ExtractValue();

			return Value();
		}

		public TNode Union([NotNull] BinomialHeap<TNode, TKey, TValue> heap)
		{
			TNode newHead = Merge(heap);
			Head = null;
			heap.Head = null;

			if (newHead != null)
			{
				TNode prev = null, current = newHead, next = newHead.Sibling;

				while (next != null)
				{
					if (current.Degree != next.Degree || next.Sibling != null && next.Sibling.Degree == current.Degree)
					{
						prev = current;
						current = next;
					}
					else
					{
						if (Compare(current, next) < 0)
						{
							current.Sibling = next.Sibling;
							current.Link(next);
						}
						else
						{
							if (prev == null) newHead = next;
							else prev.Sibling = next;

							next.Link(current);
							current = next;
						}
					}

					next = current.Sibling;
				}
			}

			_version++;
			return newHead;
		}

		public bool Contains([NotNull] TValue value)
		{
			return Find(value) != null;
		}

		public TNode Find([NotNull] TValue value)
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

		[NotNull]
		protected abstract BinomialHeap<TNode, TKey, TValue> MakeHeap([NotNull] TNode head);

		protected abstract int Compare([NotNull] TNode x, [NotNull] TNode y);

		[NotNull]
		protected TNode BubbleUp([NotNull] TNode node, bool toRoot = false)
		{
			TNode parent = node.Parent;

			while (parent != null && (toRoot || Compare(node, parent) < 0))
			{
				node.Swap(parent);
				node = parent;
				parent = parent.Parent;
			}

			return node;
		}

		protected TNode GetPrevious([NotNull] TNode node)
		{
			if (node == Head) return null;
			if (node == Head.Sibling) return Head;

			foreach (TNode sibling in Head.Siblings())
			{
				if (sibling.Sibling == node) return sibling;
			}

			return null;
		}

		protected bool RemoveRoot([NotNull] TNode root, TNode previous)
		{
			if (root == Head) Head = root.Sibling;
			else if (previous != null) previous.Sibling = root.Sibling;
			else return false;

			// Reverse the order of root's children and make a new heap
			TNode newHead = null, child = root.Child;

			while (child != null)
			{
				TNode next = child.Sibling;
				child.Sibling = newHead;
				child.Parent = null;
				newHead = child;
				child = next;
			}

			if (newHead != null) Head = Union(MakeHeap(newHead));
			Count--;
			_version++;
			return true;
		}

		protected TNode Merge([NotNull] BinomialHeap<TNode, TKey, TValue> other)
		{
			if (Head == null) return other.Head;
			if (other.Head == null) return Head;

			TNode head,
							thisNext = Head,
							otherNext = other.Head;

			if (Head.Degree <= other.Head.Degree)
			{
				head = Head;
				thisNext = head.Sibling;
			}
			else
			{
				head = other.Head;
				otherNext = head.Sibling;
			}

			TNode tail = head;

			while (thisNext != null && otherNext != null)
			{
				if (thisNext.Degree <= otherNext.Degree)
				{
					tail.Sibling = thisNext;
					thisNext = thisNext.Sibling;
				}
				else
				{
					tail.Sibling = otherNext;
					otherNext = otherNext.Sibling;
				}

				tail = tail.Sibling;
			}

			tail.Sibling = thisNext ?? otherNext;
			_version++;
			return head;
		}
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
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] BinomialNode<TKey, TValue> head)
			: this(getKeyForItem, head, null)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] BinomialNode<TKey, TValue> head, IComparer<TKey> comparer)
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
		protected override BinomialNode<TKey, TValue> MakeNode(TValue value) { return new BinomialNode<TKey, TValue>(_getKeyForItem(value), value); }
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

		/// <inheritdoc />
		protected BinomialHeap([NotNull] BinomialNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] BinomialNode<T> head, IComparer<T> comparer)
			: base(head, comparer)
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
		protected override BinomialNode<T> MakeNode(T value) { return new BinomialNode<T>(value); }
	}

	public static class BinomialHeapExtension
	{
		public static void WriteTo<TNode, TKey, TValue>([NotNull] this BinomialHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : BinomialNode<TNode, TKey, TValue>
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
				if (node.Sibling != null) queue.Enqueue(node.Sibling);
				if (node.Child != null) queue.Enqueue(node.Child);
				nodesStack.AddLast((queue, level + 1));
			}
		}
	}
}

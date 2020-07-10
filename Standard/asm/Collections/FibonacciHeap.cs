﻿using System;
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
	/// many other priority queue data structures including the binary heap and Fibonacci heap.
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

	/*
	 * And then I found this website: https://keithschwarz.com/interesting/code/?dir=fibonacci-heap
	 * from Keith Schwarz, a.k.a. the amazing templatetypedef @stackOverflow from Stanford university.
	 * This might be a whole new level to understand how this works! I modified the implementation to go
	 * with the generic other types I have already implemented so far.
	 *
	 * The following is an excerpt from the description in his implementation:
	 *
	 * An implementation of a priority queue backed by a Fibonacci heap,
	 * as described by Fredman and Tarjan. Fibonacci heaps are interesting
	 * theoretically because they have asymptotically good runtime guarantees
	 * for many operations.  In particular, insert, peek, and decrease-key all
	 * run in amortized O(1) time.  dequeueMin and delete each run in amortized
	 * O(lg n) time.  This allows algorithms that rely heavily on decrease-key
	 * to gain significant performance boosts.  For example, Dijkstra's algorithm
	 * for single-source shortest paths can be shown to run in O(m + n lg n) using
	 * a Fibonacci heap, compared to O(m lg n) using a standard binary or binomial
	 * heap.
	 *
	 * Internally, a Fibonacci heap is represented as a circular, doubly-linked
	 * list of trees obeying the min-heap property.  Each node stores pointers
	 * to its parent (if any) and some arbitrary child.  Additionally, every
	 * node stores its degree (the number of children it has) and whether it
	 * is a "marked" node.  Finally, each Fibonacci heap stores a pointer to
	 * the tree with the minimum value.
	 *
	 * The tricky operations are dequeueMin and decreaseKey.  dequeueMin works
	 * by removing the root of the tree containing the smallest element, then
	 * merging its children with the topmost roots.  Then, the roots are scanned
	 * and merged so that there is only one tree of each degree in the root list.
	 * This works by maintaining a dynamic array of trees, each initially null,
	 * pointing to the roots of trees of each dimension.  The list is then scanned
	 * and this array is populated.  Whenever a conflict is discovered, the
	 * appropriate trees are merged together until no more conflicts exist.  The
	 * resulting trees are then put into the root list.  A clever analysis using
	 * the potential method can be used to show that the amortized cost of this
	 * operation is O(lg n), see "Introduction to Algorithms, Second Edition" by
	 * Cormen, Rivest, Leiserson, and Stein for more details.
	 *
	 * The other hard operation is decreaseKey, which works as follows.  First, we
	 * update the key of the node to be the new value.  If this leaves the node
	 * smaller than its parent, we're done.  Otherwise, we cut the node from its
	 * parent, add it as a root, and then mark its parent.  If the parent was
	 * already marked, we cut that node as well, recursively mark its parent,
	 * and continue this process.  This can be shown to run in O(1) amortized time
	 * using yet another clever potential function.  Finally, given this function,
	 * we can implement delete by decreasing a key to - infinity, then calling
	 * dequeueMin to extract it.
	 */
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(FibonacciHeap<,,>.DebugView))]
	[Serializable]
	public abstract class FibonacciHeap<TNode, TKey, TValue> : IKeyedHeap<TNode, TKey, TValue>, ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
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

		private struct BreadthFirstEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly FibonacciHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] FibonacciHeap<TNode, TKey, TValue> heap, TNode root)
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
				if (_current.Child != null)
				{
					_queue.Enqueue(_current.Child);

					/*
					* The previous/next nodes are not exactly a doubly linked list but rather
					* a circular reference so will have to watch out for this.
					*/
					if (_current.Child.Next != null)
					{
						foreach (TNode forward in _current.Child.Forwards(_root))
							_queue.Enqueue(forward);
					}
				}

				if (_current.Next != null && !ReferenceEquals(_current.Next, _root)) _queue.Enqueue(_current.Next);

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
			private readonly FibonacciHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] FibonacciHeap<TNode, TKey, TValue> heap, TNode root)
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
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Child != null)
				{
					_stack.Push(_current.Child);

					/*
					* The previous/next nodes are not exactly a doubly linked list but rather
					* a circular reference so will have to watch out for this.
					*/
					if (_current.Child.Next != null)
					{
						foreach (TNode forward in _current.Child.Forwards(_root))
							_stack.Push(forward);
					}
				}

				if (_current.Next != null && !ReferenceEquals(_current.Next, _root)) _stack.Push(_current.Next);

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
			Head = Merge(Head, node);
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
			// This is a special implementation of decreaseKey that sets the
			// argument to the min/maximum value. This is necessary to make generic keys
			// work, since there is no Min/MaximumValue constant for generic types.
			TNode parent = node.Parent;
			if (parent != null) Cut(node);
			Head = node;
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

			TNode parent = node.Parent;

			/*
			 * If the node no longer has a higher priority than its parent, cut it.
			 * Note that this also means that if we try to run a delete operation
			 * that decreases the key to -infinity, it's guaranteed to cut the node
			 * from its parent.
			 */
			if (parent != null && Compare(node.Key, parent.Key) < 0) Cut(node);
			if (Compare(node.Key, Head.Key) > 0) return;
			Head = node;
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

			// Grab the min/maximum element so we know what to return.
			TNode value = Head;

			/*
			 * Now, we need to get rid of this element from the list of roots. There
			 * are two cases to consider. First, if this is the only element in the
			 * list of roots, we set the list of roots to be null by clearing Head.
			 * Otherwise, if it's not null, then we write the elements next to the
			 * Head element around it to remove it, then arbitrarily reassign the Head.
			 */
			if (value.Next == value)
			{
				// Case 1
				Head = null;
			}
			else
			{
				// Case 2
				Head.Previous.Next = Head.Next;
				Head.Next.Previous = Head.Previous;
				Head = Head.Next; // Arbitrary element of the root list.
			}

			/*
			 * Next, clear the parent fields of all of the min element's children,
			 * since they're about to become roots. Because the elements are
			 * stored in a circular list, the traversal is a bit complex.
			 */
			if (value.Child != null)
			{
				// Keep track of the first visited node.
				TNode next = value.Child;

				do
				{
					next.Parent = null;
					next = next.Next;
				}
				while (next != value.Child);
			}

			/*
			 * Next, splice the children of the root node into the topmost list, 
			 * then set Head to point somewhere in that list.
			 */
			Head = Merge(Head, value.Child);
			Count--;
			_version++;

			// If there are no entries left, we're done.
			if (Head == null) return value.Value;

			// the next code is sick to say the least!
			/*
			 * Next, we need to coalesce all of the roots so that there is only one
			 * tree of each degree. To track trees of each size, we allocate an
			 * List where the entry at position i is either null or the 
			 * unique tree of degree i.
			 */
			List<TNode> treeTable = new List<TNode>();

			/*
			 * We need to traverse the entire list, but since we're going to be
			 * messing around with it we have to be careful not to break our
			 * traversal order mid-stream. One major challenge is how to detect
			 * whether we're visiting the same node twice. To do this, we'll
			 * spend a bit of overhead adding all of the nodes to a list, and
			 * then will visit each element of this list in order.
			 */
			List<TNode> toVisit = new List<TNode>();

			/*
			 * To add everything, we'll iterate across the elements until we
			 * find the first element twice. We check this by looping while the
			 * list is empty or while the current element isn't the first element
			 * of that list.
			 */
			for (TNode next = Head; toVisit.Count == 0 || toVisit[0] != next; next = next.Next)
				toVisit.Add(next);

			// Traverse this list and perform the appropriate union steps.
			foreach (TNode node in toVisit)
			{
				TNode next = node;

				// Keep merging until a match arises.
				while (true)
				{
					// Ensure that the list is long enough to hold an element of this degree.
					while (next.Degree >= treeTable.Count)
						treeTable.Add(null);

					/*
					 * If nothing's here, we're can record that this tree has this size
					 * and are done processing.
					 */
					if (treeTable[next.Degree] == null)
					{
						treeTable[next.Degree] = next;
						break;
					}

					// Otherwise, merge with what's there.
					TNode other = treeTable[next.Degree];
					treeTable[next.Degree] = null; // Clear the slot

					/*
					 * Determine which of the two trees has the smaller root, storing
					 * the two tree accordingly.
					 */
					TNode min;
					TNode max;

					if (Compare(other.Key, next.Key) < 0)
					{
						min = other;
						max = next;
					}
					else
					{
						min = next;
						max = other;
					}

					// Break max out of the root list, then merge it into min's child list.
					max.Next.Previous = max.Previous;
					max.Previous.Next = max.Next;

					// Make it a singleton so that we can merge it.
					max.Next = max.Previous = max;
					min.Child = Merge(min.Child, max);

					// Re-parent max appropriately.
					max.Parent = min;

					// Clear max's mark, since it can now lose another child.
					max.Marked = false;

					// Increase min's degree; it now has another child.
					++min.Degree;

					// Continue merging this tree.
					next = min;
				}

				/*
				 * Update the global min based on this node. Note that we compare
				 * for <= instead of < here. That's because if we just did a
				 * re-parent operation that merged two different trees of equal
				 * priority, we need to make sure that the min pointer points to
				 * the root-level one.
				 */
				if (Compare(next.Key, Head.Key) <= 0) Head = next;
			}

			return value.Value;
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

		public virtual bool Equals(FibonacciHeap<TNode, TKey, TValue> other)
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
		/// Merge two pointers into disjoint circularly linked lists, merges the two lists together
		/// into one circularly-linked list in O(1) time. It's assumed that x and y are the minimum
		/// elements of the lists they are in, and returns a pointer to whichever has a preceding priority.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private TNode Merge(TNode x, TNode y)
		{
			if (ReferenceEquals(x, y)) return x;
			if (x == null) return y;
			if (y == null) return x;

			/*
			 * The next comment and drawing are taken from:
			 * https://keithschwarz.com/interesting/code/?dir=fibonacci-heap
			 * I just changed the parameter names.
			 *
			 * Both non-null; actually do the splice. This is not as easy as
			 * it seems. The idea is that we'll have two lists that look like this:
             *
             * +----+     +----+     +----+
             * |    |--N->| x  |--N->|    |
             * |    |<-P--|    |<-P--|    |
             * +----+     +----+     +----+
             *
             *
             * +----+     +----+     +----+
             * |    |--N->| y  |--N->|    |
             * |    |<-P--|    |<-P--|    |
             * +----+     +----+     +----+
             *
             * And we want to relink everything to get
             *
             * +----+     +----+     +----+---+
             * |    |--N->| x  |     |    |   |
             * |    |<-P--|    |     |    |<+ |
             * +----+     +----+<-\  +----+ | |
             *                  \  P        | |
             *                   N  \       N |
             * +----+     +----+  \->+----+ | |
             * |    |--N->| y  |     |    | | |
             * |    |<-P--|    |     |    | | P
             * +----+     +----+     +----+ | |
             *              ^ |             | |
             *              | +-------------+ |
             *              +-----------------+
			 */
			TNode xNext = x.Next;
			x.Next = y.Next;
			x.Next.Previous = x;
			y.Next = xNext;
			y.Next.Previous = y;
			return Compare(x.Key, y.Key) < 0
						? x
						: y;
		}

		/// <summary>
		/// Cuts the link between a node and its parent, moving the node to the root list.
		/// If the parent was already marked, recursively cuts that node from its parent
		/// as well.
		/// </summary>
		/// <param name="node">The node being cut.</param>
		private void Cut([NotNull] TNode node)
		{
			// Begin by clearing the node's mark, since we just cut it.
			node.Marked = false;

			// base case
			if (node.Parent == null) return;

			// Rewire the node's siblings around it, if it has any siblings.
			if (node.Next != node)
			{
				// Has siblings
				node.Next.Previous = node.Previous;
				node.Previous.Next = node.Next;
			}

			/*
			 * If the node is the one identified by its parent as its child,
			 * we need to rewrite that pointer to point to some arbitrary other
			 * child.
			 */
			if (node.Parent.Child == node)
			{
				/*
				 * If there are any other children, pick one of them arbitrarily.
				 * Otherwise, there aren't any children left and we should clear the
				 * pointer and drop the node's degree.
				 */
				node.Parent.Child = node.Next != node
										? node.Next
										: null;
			}

			// Decrease the degree of the parent, since it just lost a child.
			--node.Parent.Degree;

			/*
			 * Splice this tree into the root list by converting it to a singleton
			 * and invoking the merge subroutine.
			 */
			node.Previous = node.Next = node;
			Head = Merge(Head, node);

			// Mark the parent and recursively cut it if it's already been marked.
			if (node.Parent.Marked)
				Cut(node.Parent);
			else
				node.Parent.Marked = true;

			// Clear the relocated node's parent; it's now a root.
			node.Parent = null;
		}

		#region Iterator Traversal for Action<TNode>
		private void BreadthFirst([NotNull] TNode root, [NotNull] Action<TNode> visitCallback)
		{
			int version = _version;
			// Head-Next-Child (Queue)
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
					if (current.Child != null)
					{
						queue.Enqueue(current.Child);

						/*
						* The previous/next nodes are not exactly a doubly linked list but rather
						* a circular reference so will have to watch out for this.
						*/
						if (current.Child.Next != null)
						{
							foreach (TNode forward in current.Child.Forwards(root))
								queue.Enqueue(forward);
						}
					}

					if (current.Next != null && !ReferenceEquals(current.Next, root)) queue.Enqueue(current.Next);
				}

				root = root.Next != null && !ReferenceEquals(root.Next, root)
							? root.Next
							: null;
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
					if (current.Child != null)
					{
						stack.Push(current.Child);

						/*
						* The previous/next nodes are not exactly a doubly linked list but rather
						* a circular reference so will have to watch out for this.
						*/
						if (current.Child.Next != null)
						{
							foreach (TNode forward in current.Child.Forwards(root))
								stack.Push(forward);
						}
					}

					if (current.Next != null && !ReferenceEquals(current.Next, root)) stack.Push(current.Next);
				}

				root = root.Next != null && !ReferenceEquals(root.Next, root)
							? root.Next
							: null;
			}
		}
		#endregion

		#region Iterator Traversal for Func<TNode, bool>
		private void BreadthFirst([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			int version = _version;
			// Head-Next-Child (Queue)
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
					if (current.Child != null)
					{
						queue.Enqueue(current.Child);

						/*
						* The previous/next nodes are not exactly a doubly linked list but rather
						* a circular reference so will have to watch out for this.
						*/
						if (current.Child.Next != null)
						{
							foreach (TNode forward in current.Child.Forwards(root))
								queue.Enqueue(forward);
						}
					}

					if (current.Next != null && !ReferenceEquals(current.Next, root)) queue.Enqueue(current.Next);
				}

				root = root.Next != null && !ReferenceEquals(root.Next, root)
							? root.Next
							: null;
			}
		}

		private void DepthFirst([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback)
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
					if (!visitCallback(current)) return;

					// Queue the next nodes
					if (current.Child != null)
					{
						stack.Push(current.Child);

						/*
						* The previous/next nodes are not exactly a doubly linked list but rather
						* a circular reference so will have to watch out for this.
						*/
						if (current.Child.Next != null)
						{
							foreach (TNode forward in current.Child.Forwards(root))
								stack.Push(forward);
						}
					}

					if (current.Next != null && !ReferenceEquals(current.Next, root)) stack.Push(current.Next);
				}

				root = root.Next != null && !ReferenceEquals(root.Next, root)
							? root.Next
							: null;
			}
		}
		#endregion
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
		public override FibonacciNode<TKey, TValue> MakeNode(TValue value) { return new FibonacciNode<TKey, TValue>(_getKeyForItem(value), value); }
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
		public override FibonacciNode<T> MakeNode(T value) { return new FibonacciNode<T>(value); }
	}

	public static class FibonacciHeapExtension
	{
		public static void WriteTo<TNode, TKey, TValue>([NotNull] this FibonacciHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : FibonacciNode<TNode, TKey, TValue>
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

					if (node.Child.Next != null)
					{
						foreach (TNode sibling in node.Child.Forwards())
							queue.Enqueue(sibling);
					}

					nodesList.AddLast((queue, level + 1));
				}

				root = root.Next == thisValue.Head
							? null
							: root.Next;
			}
		}
	}
}
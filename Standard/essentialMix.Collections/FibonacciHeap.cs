using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections
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
	[Serializable]
	public abstract class FibonacciHeap<TNode, TKey, TValue> : SiblingsHeap<TNode, TKey, TValue>
		where TNode : FibonacciNode<TNode, TKey, TValue>
	{
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
								/*
								* The previous/next nodes are not exactly a doubly linked list but rather
								* a circular reference so will have to watch out for this.
								*/
								: _current?.Next != null && !ReferenceEquals(_current.Next, _root)
									? _current.Next
									: null;
				
				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes.
				if (_current.Child == null) return true;
				_queue.Enqueue(_current.Child);
				if (_current.Child.Next == null) return true;

				foreach (TNode forward in _current.Child.Forwards(_root))
					_queue.Enqueue(forward);

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
								/*
								* The previous/next nodes are not exactly a doubly linked list but rather
								* a circular reference so will have to watch out for this.
								*/
								: _current?.Next != null && !ReferenceEquals(_current.Next, _root)
									? _current.Next
									: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Child == null) return true;
				_stack.Push(_current.Child);
				if (_current.Child.Next == null) return true;

				foreach (TNode forward in _current.Child.Forwards(_root))
					_stack.Push(forward);

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
		protected FibonacciHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected FibonacciHeap(IComparer<TKey> comparer)
			: base(comparer)
		{
		}

		protected FibonacciHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected FibonacciHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
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
			Head = Merge(Head, node);
			Count++;
			_version++;
			return node;
		}

		/// <inheritdoc />
		public sealed override bool Remove(TNode node)
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
		public sealed override TValue Value()
		{
			if (Head == null) throw new CollectionIsEmptyException();
			return Head.Value;
		}

		/// <inheritdoc />
		public sealed override TNode ExtractValue()
		{
			if (Head == null) throw new CollectionIsEmptyException();

			// Grab the min/maximum element so we know what to return.
			TNode head = Head;

			/*
			 * Now, we need to get rid of this element from the list of roots. There
			 * are two cases to consider. First, if this is the only element in the
			 * list of roots, we set the list of roots to be null by clearing Head.
			 * Otherwise, if it's not null, then we write the elements next to the
			 * Head element around it to remove it, then arbitrarily reassign the Head.
			 */
			if (head.Next == head)
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
			if (head.Child != null)
			{
				// Keep track of the first visited node.
				TNode next = head.Child;

				do
				{
					next.Parent = null;
					next = next.Next;
				}
				while (next != head.Child);
			}

			/*
			 * Next, splice the children of the root node into the topmost list, 
			 * then set Head to point somewhere in that list.
			 */
			Head = Merge(Head, head.Child);
			Count--;
			_version++;

			// If there are no entries left, we're done.
			if (Head == null)
			{
				head.Invalidate();
				return head;
			}

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

			head.Invalidate();
			return head;
		}

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
		internal sealed class DebugView : Dbg_HeapDebugView<FibonacciNode<TKey, TValue>, TKey, TValue>
		{
			/// <inheritdoc />
			public DebugView([NotNull] SiblingsHeap<FibonacciNode<TKey, TValue>, TKey, TValue> heap)
				: base(heap)
			{
			}
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
		internal sealed class DebugView : Dbg_HeapDebugView<FibonacciNode<T>, T, T>
		{
			/// <inheritdoc />
			public DebugView([NotNull] SiblingsHeap<FibonacciNode<T>, T, T> heap)
				: base(heap)
			{
			}
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
}
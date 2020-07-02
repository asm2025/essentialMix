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
	/// <typeparam name="T">The element type of the heap</typeparam>
	// https://www.growingwiththeweb.com/data-structures/binomial-heap/overview/
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(BinomialHeap<>.DebugView))]
	[Serializable]
	public abstract class BinomialHeap<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
	{
		internal sealed class DebugView
		{
			private readonly BinomialHeap<T> _heap;

			public DebugView([NotNull] BinomialHeap<T> heap)
			{
				_heap = heap;
			}

			[NotNull]
			public BinomialNode<T> Head => _heap.Head;
		}

		private struct LevelOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly BinomialHeap<T> _heap;
			private readonly int _version;
			private readonly BinomialNode<T> _head;
			private readonly Queue<BinomialNode<T>> _queue;

			private BinomialNode<T> _current;
			private bool _started;
			private bool _done;

			internal LevelOrderEnumerator([NotNull] BinomialHeap<T> heap, BinomialNode<T> head)
			{
				_heap = heap;
				_version = _heap._version;
				_head = head;
				_queue = new Queue<BinomialNode<T>>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
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
					// Start at the head
					_queue.Enqueue(_head);
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
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct PreOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly BinomialHeap<T> _heap;
			private readonly int _version;
			private readonly BinomialNode<T> _head;
			private readonly Stack<BinomialNode<T>> _stack;

			private BinomialNode<T> _current;
			private bool _started;
			private bool _done;

			internal PreOrderEnumerator([NotNull] BinomialHeap<T> heap, BinomialNode<T> head)
			{
				_heap = heap;
				_version = _heap._version;
				_head = head;
				_stack = new Stack<BinomialNode<T>>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
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

				// Head-Sibling-Child (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the head
					_stack.Push(_head);
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
				if (_current.Sibling != null) _stack.Push(_current.Sibling);
				if (_current.Child != null) _stack.Push(_current.Child);

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct InOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly BinomialHeap<T> _heap;
			private readonly int _version;
			private readonly BinomialNode<T> _head;
			private readonly Stack<BinomialNode<T>> _stack;

			private BinomialNode<T> _current;
			private bool _started;
			private bool _done;

			internal InOrderEnumerator([NotNull] BinomialHeap<T> heap, BinomialNode<T> head)
			{
				_heap = heap;
				_version = _heap._version;
				_head = head;
				_stack = new Stack<BinomialNode<T>>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
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

				// Sibling-Head-Child (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the head
					_current = _head;
				}
				else
				{
					_current = _current?.Child;
				}

				while (_current != null || _stack.Count > 0)
				{
					if (_version != _heap._version) throw new VersionChangedException();

					if (_current != null)
					{
						_stack.Push(_current);
						_current = _current.Sibling;
					}
					else
					{
						// visit the next queued node
						_current = _stack.Pop();
						break; // break from the loop to visit this node
					}
				}

				_done = _current == null;
				return !_done;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}
		
		private struct PostOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly BinomialHeap<T> _heap;
			private readonly int _version;
			private readonly BinomialNode<T> _head;
			private readonly Stack<BinomialNode<T>> _stack;

			private BinomialNode<T> _current;
			private bool _started;
			private bool _done;

			internal PostOrderEnumerator([NotNull] BinomialHeap<T> heap, BinomialNode<T> head)
			{
				_heap = heap;
				_version = _heap._version;
				_head = head;
				_stack = new Stack<BinomialNode<T>>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
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

				// Sibling-Child-Head (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the head
					_current = _head;
				}
				else
				{
					_current = null;
				}
	
				do
				{
					while (_current != null)
					{
						if (_version != _heap._version) throw new VersionChangedException();
						if (_current.Child != null) _stack.Push(_current.Child);

						_stack.Push(_current);
						_current = _current.Sibling;
					}

					if (_version != _heap._version) throw new VersionChangedException();
					_current = _stack.Count > 0
									? _stack.Pop()
									: null;
					if (_current == null) continue;

					/*
						* if Current has a right child and is not processed yet,
						* then make sure right child is processed before head
						*/
					if (_current.Child != null && _stack.Count > 0 && _current.Child == _stack.Peek())
					{
						// remove right child from stack
						_stack.Pop();
						// push Current back to stack
						_stack.Push(_current);
						// process right first
						_current = _current.Child;
						continue;
					}

					if (_current != null)
						break; // break from the loop to visit this node
				} while (_stack.Count > 0);

				_done = _current == null;
				return !_done;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _heap.Count == 0 || _head == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		protected internal int _version;

		private object _syncRoot;

		/// <inheritdoc />
		protected BinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		protected BinomialHeap(IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] BinomialNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] BinomialNode<T> head, IComparer<T> comparer)
			: this(comparer)
		{
			Head = head;
		}

		protected BinomialHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected BinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		[NotNull]
		public IComparer<T> Comparer { get; }

		/// <inheritdoc cref="ICollection{T}" />
		public int Count => Head == null ? 0 : 1;

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

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
		
		protected internal BinomialNode<T> Head { get; set; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Enumerate(Head); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="head">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(BinomialNode<T> head, BinaryTreeTraverseMethod method)
		{
			return method switch
			{
				BinaryTreeTraverseMethod.LevelOrder => new LevelOrderEnumerator(this, head),
				BinaryTreeTraverseMethod.PreOrder => new PreOrderEnumerator(this, head),
				BinaryTreeTraverseMethod.InOrder => new InOrderEnumerator(this, head),
				BinaryTreeTraverseMethod.PostOrder => new PostOrderEnumerator(this, head),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(BinomialNode<T> head)
		{
			return Enumerate(head, BinaryTreeTraverseMethod.LevelOrder);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="head">The starting node</param>
		/// <param name="method">The traverse method <see cref="BinaryTreeTraverseMethod"/></param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(BinomialNode<T> head, BinaryTreeTraverseMethod method, [NotNull] Action<BinomialNode<T>> visitCallback)
		{
			if (Count == 0 || head == null) return;

			switch (method)
			{
				case BinaryTreeTraverseMethod.LevelOrder:
					LevelOrder(head, visitCallback);
					break;
				case BinaryTreeTraverseMethod.PreOrder:
					PreOrder(head, visitCallback);
					break;
				case BinaryTreeTraverseMethod.InOrder:
					InOrder(head, visitCallback);
					break;
				case BinaryTreeTraverseMethod.PostOrder:
					PostOrder(head, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Iterate overloads - visitCallback action
		public void Iterate(BinomialNode<T> head, [NotNull] Action<BinomialNode<T>> visitCallback)
		{
			Iterate(head, BinaryTreeTraverseMethod.LevelOrder, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="head">The starting node</param>
		/// <param name="method">The traverse method <see cref="BinaryTreeTraverseMethod"/></param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(BinomialNode<T> head, BinaryTreeTraverseMethod method, [NotNull] Func<BinomialNode<T>, bool> visitCallback)
		{
			if (head == null) return;

			switch (method)
			{
				case BinaryTreeTraverseMethod.LevelOrder:
					LevelOrder(head, visitCallback);
					break;
				case BinaryTreeTraverseMethod.PreOrder:
					PreOrder(head, visitCallback);
					break;
				case BinaryTreeTraverseMethod.InOrder:
					InOrder(head, visitCallback);
					break;
				case BinaryTreeTraverseMethod.PostOrder:
					PostOrder(head, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Iterate overloads - visitCallback function
		public void Iterate(BinomialNode<T> head, [NotNull] Func<BinomialNode<T>, bool> visitCallback)
		{
			Iterate(head, BinaryTreeTraverseMethod.LevelOrder, visitCallback);
		}
		#endregion

		/// <inheritdoc />
		public void Add(T value)
		{
			BinomialNode<T> node = new BinomialNode<T>(value);
			Head = Union(NewHeap(node));
		}

		public void Add([NotNull] IEnumerable<T> enumerable)
		{
			foreach (T item in enumerable)
				Add(item);
		}

		/// <inheritdoc />
		public bool Remove(T value)
		{
			BinomialNode<T> node = null, next = Head;
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();

			while (next != null)
			{
				if (Comparer.IsEqual(next.Value, value))
				{
					node = next;
					break;
				}

				stack.Push(next);
				next = next.Sibling;
			}

			if (node == null) return false;
			node = BubbleUp(stack, node, true);
			return RemoveRoot(node, GetPrevious(node));
		}

		/// <inheritdoc />
		public void Clear()
		{
			Head = null;
			_version++;
		}

		public void DecreaseKey([NotNull] T value, [NotNull] T newValue)
		{
			BinomialNode<T> node = null, next = Head;
			Stack<BinomialNode<T>> parents = new Stack<BinomialNode<T>>();

			while (next != null)
			{
				if (Comparer.IsEqual(next.Value, value))
				{
					node = next;
					break;
				}

				parents.Push(next);
				next = next.Sibling;
			}

			if (node == null) throw new KeyNotFoundException();
			node.Value = newValue;
			BubbleUp(parents, node);
		}

		public abstract T Value();

		public abstract T ExtractValue();

		public abstract BinomialNode<T> Union([NotNull] BinomialHeap<T> heap);

		/// <inheritdoc />
		public bool Contains(T value)
		{
			if (Head == null) return false;

			bool found = false;
			Iterate(Head, e =>
			{
				if (Comparer.IsEqual(e.Value, value)) found = true;
				return !found;
			});

			return found;
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(Head, e => array[arrayIndex++] = e);
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is T[] tArray)
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
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;
			Iterate(Head, e => objects[index++] = e.Value);
		}

		protected abstract BinomialHeap<T> NewHeap([NotNull] BinomialNode<T> head);

		protected abstract BinomialNode<T> BubbleUp([NotNull] Stack<BinomialNode<T>> stack, [NotNull] BinomialNode<T> node, bool toRoot = false);

		protected BinomialNode<T> GetPrevious([NotNull] BinomialNode<T> node)
		{
			if (node == Head) return null;

			BinomialNode<T> previous = Head;

			while (previous.Sibling != node) 
				previous = previous.Sibling;

			return previous;
		}

		protected BinomialNode<T> FindSibling([NotNull] BinomialNode<T> node, [NotNull] T value)
		{
			foreach (BinomialNode<T> sibling in node.Siblings())
			{
				if (Comparer.IsEqual(sibling.Value, value)) return sibling;
			}

			return null;
		}

		protected BinomialNode<T> FindChild([NotNull] BinomialNode<T> node, [NotNull] T value)
		{
			foreach (BinomialNode<T> child in node.Children())
			{
				if (Comparer.IsEqual(child.Value, value)) return child;
			}

			return null;
		}

		protected bool RemoveRoot([NotNull] BinomialNode<T> root, BinomialNode<T> previous)
		{
			if (root == Head) Head = root.Sibling;
			else if (previous != null) previous.Sibling = root.Sibling;
			else return false;

			// Reverse the order of root's children and make a new heap
			BinomialNode<T> newHead = null, child = root.Child;

			while (child != null)
			{
				BinomialNode<T> next = child.Sibling;
				child.Sibling = newHead;
				newHead = child;
				child = next;
			}

			if (newHead != null) Head = Union(NewHeap(newHead));
			_version++;
			return true;
		}

		protected BinomialNode<T> Merge([NotNull] BinomialHeap<T> other)
		{
			if (Head == null) return other.Head;
			if (other.Head == null) return Head;

			BinomialNode<T> head,
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

			BinomialNode<T> tail = head;

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
			return head;
		}

		#region Iterative Traversal for Action<BinomialNode<T>>
		private void LevelOrder([NotNull] BinomialNode<T> root, [NotNull] Action<BinomialNode<T>> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<BinomialNode<T>> queue = new Queue<BinomialNode<T>>();

			// Start at the head
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				BinomialNode<T> current = queue.Dequeue();
				visitCallback(current);

				// Queue the next nodes
				if (current.Sibling != null) queue.Enqueue(current.Sibling);
				if (current.Child != null) queue.Enqueue(current.Child);
			}
		}

		private void PreOrder([NotNull] BinomialNode<T> root, [NotNull] Action<BinomialNode<T>> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Stack)
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();

			// Start at the head
			stack.Push(root);

			while (stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				BinomialNode<T> current = stack.Pop();
				visitCallback(current);

				// Queue the next nodes
				if (current.Child != null) stack.Push(current.Child);
				if (current.Sibling != null) stack.Push(current.Sibling);
			}
		}

		private void InOrder([NotNull] BinomialNode<T> root, [NotNull] Action<BinomialNode<T>> visitCallback)
		{
			int version = _version;
			// Sibling-Head-Child (Stack)
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();

			// Start at the head
			BinomialNode<T> current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = current.Sibling;
				}
				else
				{
					// visit the next queued node
					current = stack.Pop();
					visitCallback(current);
					current = current.Child;
				}
			}
		}

		private void PostOrder([NotNull] BinomialNode<T> root, [NotNull] Action<BinomialNode<T>> visitCallback)
		{
			int version = _version;
			// Sibling-Child-Head (Stack)
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();
			BinomialNode<T> lastVisited = null;
			// Start at the head
			BinomialNode<T> current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = current.Sibling; // Navigate left
					continue;
				}

				BinomialNode<T> peek = stack.Peek();

				/*
					* At this point we are either coming from
					* either the head node or the left branch.
					* Is there a right node?
					* if yes, then navigate right.
					*/
				if (peek.Child != null && lastVisited != peek.Child)
				{
					current = peek.Child;
				}
				else
				{
					// visit the next queued node
					lastVisited = peek;
					current = stack.Pop();
					visitCallback(current);
					current = null;
				}
			}
		}
		#endregion
		
		#region Iterative Traversal for Func<BinomialNode<T>, bool>
		private void LevelOrder([NotNull] BinomialNode<T> root, [NotNull] Func<BinomialNode<T>, bool> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<BinomialNode<T>> queue = new Queue<BinomialNode<T>>();

			// Start at the head
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				BinomialNode<T> current = queue.Dequeue();
				if (!visitCallback(current)) break;

				// Queue the next nodes
				if (current.Sibling != null) queue.Enqueue(current.Sibling);
				if (current.Child != null) queue.Enqueue(current.Child);
			}
		}

		private void PreOrder([NotNull] BinomialNode<T> root, [NotNull] Func<BinomialNode<T>, bool> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Stack)
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();

			// Start at the head
			stack.Push(root);

			while (stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				BinomialNode<T> current = stack.Pop();
				if (!visitCallback(current)) break;

				// Queue the next nodes
				if (current.Child != null) stack.Push(current.Child);
				if (current.Sibling != null) stack.Push(current.Sibling);
			}
		}

		private void InOrder([NotNull] BinomialNode<T> root, [NotNull] Func<BinomialNode<T>, bool> visitCallback)
		{
			int version = _version;
			// Sibling-Head-Child (Stack)
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();

			// Start at the head
			BinomialNode<T> current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = current.Sibling;
				}
				else
				{
					// visit the next queued node
					current = stack.Pop();
					if (!visitCallback(current)) break;
					current = current.Child;
				}
			}
		}

		private void PostOrder([NotNull] BinomialNode<T> root, [NotNull] Func<BinomialNode<T>, bool> visitCallback)
		{
			int version = _version;
			// Sibling-Child-Head (Stack)
			Stack<BinomialNode<T>> stack = new Stack<BinomialNode<T>>();
			BinomialNode<T> lastVisited = null;
			// Start at the head
			BinomialNode<T> current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = current.Sibling;
					continue;
				}

				BinomialNode<T> peek = stack.Peek();

				/*
					* At this point we are either coming from
					* either the head node or the left branch.
					* Is there a right node?
					* if yes, then navigate right.
					*/
				if (peek.Child != null && lastVisited != peek.Child)
				{
					// Navigate right
					current = peek.Child;
				}
				else
				{
					// visit the next queued node
					lastVisited = peek;
					current = stack.Pop();
					if (!visitCallback(current)) break;
					current = null;
				}
			}
		}
		#endregion
	}

	public static class BinomialHeapExtension
	{
		public static void WriteTo<T>([NotNull] this BinomialHeap<T> thisValue, [NotNull] TextWriter writer)
		{
			const string STR_BLANK = "   ";
			const string STR_EXT = "│  ";
			const string STR_CONNECTOR_L = "└─ ";
			const string STR_NULL = "<null>";

			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			LinkedList<Queue<BinomialNode<T>>> nodesStack = new LinkedList<Queue<BinomialNode<T>>>();
			Queue<BinomialNode<T>> rootQueue = new Queue<BinomialNode<T>>(1);
			rootQueue.Enqueue(thisValue.Head);
			nodesStack.AddFirst(rootQueue);

			while (nodesStack.Count > 0)
			{
				Queue<BinomialNode<T>> nodes = nodesStack.Last.Value;

				if (nodes.Count == 0)
				{
					nodesStack.RemoveLast();
					continue;
				}

				BinomialNode<T> node = nodes.Dequeue();
				indent.Length = 0;

				foreach (Queue<BinomialNode<T>> nodesQueue in nodesStack)
				{
					if (nodesQueue == nodesStack.Last.Value) break;
					indent.Append(nodesQueue.Count > 0
									? STR_EXT
									: STR_BLANK);
				}

				writer.Write(indent + STR_CONNECTOR_L);

				if (node == null)
				{
					writer.WriteLine(STR_NULL);
					continue;
				}

				writer.WriteLine(node.ToString());
				if (node.IsLeaf) continue;

				Queue<BinomialNode<T>> queue = new Queue<BinomialNode<T>>(2);
				if (node.Sibling != null) queue.Enqueue(node.Sibling);
				if (node.Child != null) queue.Enqueue(node.Child);
				nodesStack.AddLast(queue);
			}
		}
	}
}

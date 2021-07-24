using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{

	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Binary_heap">Binary Heap</see> using the array representation.
	/// </summary>
	/// <typeparam name="TNode">The node type. This is just for abstraction purposes and shouldn't be dealt with directly.</typeparam>
	/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
	/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
	/// priority of the item.</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	/*
	 *	     ___ F ____
	 *	    /          \
	 *	   C            I
	 *	 /   \        /   \
	 *	A     D      G     J
	 *	  \    \      \     \
	 *	    B   E      H     K
	 *
	 *	    BFS            DFS            DFS            DFS
	 *	  LevelOrder      PreOrder       InOrder        PostOrder
	 *	     1              1              4              5
	 *	   /   \          /   \          /   \          /   \
	 *	  2     3        2     5        2     5        3     4
	 *	 /   \          /   \          /   \          /   \
	 *	4     5        3     4        1     3        1     2
	 *
	 * BFS (LevelOrder): FCIADGJBEHK => Root-Left-Right (Queue)
	 * DFS [PreOrder]:   FCABDEIGHJK => Root-Left-Right (Stack)
	 * DFS [InOrder]:    ABCDEFGHIJK => Left-Root-Right (Stack)
	 * DFS [PostOrder]:  BAEDCHGKJIF => Left-Right-Root (Stack)
	 */
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_BinaryHeapDebugView<,,>))]
	[Serializable]
	public abstract class BinaryHeap<TNode, TKey, TValue> : IKeyedHeap<TNode, TKey, TValue>, ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
	{
		/// <summary>
		/// A node metadata used to navigate the tree and print it
		/// </summary>
		[DebuggerDisplay("{Key} = {Value}")]
		public struct Navigator
		{
			[NotNull]
			private readonly BinaryHeap<TNode, TKey, TValue> _heap;

			private int _index;

			internal Navigator([NotNull] BinaryHeap<TNode, TKey, TValue> heap, int index = -1)
				: this()
			{
				_heap = heap;
				Index = index;
			}

			public int Index
			{
				get => _index;
				set
				{
					_index = value;

					if (_index < 0)
					{
						Invalidate();
						return;
					}

					ParentIndex = _heap.ParentIndex(_index);
					LeftIndex = _heap.LeftIndex(_index);
					RightIndex = _heap.RightIndex(_index);
				}
			}

			[NotNull]
			public TKey Key => _heap.Items[Index].Key;

			[NotNull]
			public TValue Value => _heap.Items[Index].Value;

			public int ParentIndex { get; private set; }

			public int LeftIndex { get; private set; }

			public int RightIndex { get; private set; }

			public bool IsRoot => Index == 0;
			public bool IsLeft => Index > 0 && Index % 2 != 0;
			public bool IsRight => Index > 0 && Index % 2 == 0;
			public bool IsLeaf => LeftIndex < 0 && RightIndex < 0;
			public bool IsNode => LeftIndex > 0 && RightIndex > 0;
			public bool HasOneChild => (LeftIndex > 0) ^ (RightIndex > 0);
			public bool IsFull => !HasOneChild;

			public bool ParentIsRoot => ParentIndex == 0;
			public bool ParentIsLeft => ParentIndex > 0 && ParentIndex % 2 != 0;
			public bool ParentIsRight => ParentIndex > 0 && ParentIndex % 2 == 0;

			/// <inheritdoc />
			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }

			[NotNull]
			internal string ToString(int level)
			{
				return _heap.Items[Index].ToString(level);
			}

			public IEnumerable<int> Ancestors()
			{
				if (ParentIndex <= 0) yield break;

				int index = ParentIndex;

				while (index > -1)
				{
					yield return index;
					index = _heap.ParentIndex(index);
				}
			}

			public int LeftMost()
			{
				int index = Index, next = _heap.LeftIndex(index);

				while (next > -1)
				{
					index = next;
					next = _heap.LeftIndex(next);
				}

				return index;
			}

			public int RightMost()
			{
				int index = Index, next = _heap.RightIndex(index);

				while (next > -1)
				{
					index = next;
					next = _heap.RightIndex(next);
				}

				return index;
			}

			[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
			public void Swap(int other)
			{
				TNode tmp = _heap.Items[other];
				_heap.Items[other] = _heap.Items[Index];
				_heap.Items[Index] = tmp;
			}

			private void Invalidate()
			{
				_index = ParentIndex = LeftIndex = RightIndex = -1;
			}
		}

		private struct LevelOrderEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly BinaryHeap<TNode, TKey, TValue> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Queue<int> _queue;
			private readonly bool _rightToLeft;

			private Navigator _current;
			private bool _started;
			private bool _done;

			internal LevelOrderEnumerator([NotNull] BinaryHeap<TNode, TKey, TValue> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_queue = new Queue<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.NewNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			[NotNull]
			public TValue Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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
				if (_version != _tree._version) throw new VersionChangedException();
				// Root-Left-Right (Queue)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queue.Enqueue(_index);
				}

				// visit the next queued node
				_current.Index = _queue.Count > 0
								? _queue.Dequeue()
								: -1;

				if (_current.Index < 0)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_rightToLeft)
				{
					if (_current.RightIndex > -1) _queue.Enqueue(_current.RightIndex);
					if (_current.LeftIndex > -1) _queue.Enqueue(_current.LeftIndex);
				}
				else
				{
					if (_current.LeftIndex > -1) _queue.Enqueue(_current.LeftIndex);
					if (_current.RightIndex > -1) _queue.Enqueue(_current.RightIndex);
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current.Index = -1;
				_started = false;
				_queue.Clear();
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct PreOrderEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly BinaryHeap<TNode, TKey, TValue> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private Navigator _current;
			private bool _started;
			private bool _done;

			internal PreOrderEnumerator([NotNull] BinaryHeap<TNode, TKey, TValue> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_stack = new Stack<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.NewNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			[NotNull]
			public TValue Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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
				if (_version != _tree._version) throw new VersionChangedException();

				// Root-Left-Right (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_stack.Push(_index);
				}

				// visit the next queued node
				_current.Index = _stack.Count > 0
								? _stack.Pop()
								: -1;

				if (_current.Index < 0)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_rightToLeft)
				{
					if (_current.LeftIndex > -1) _stack.Push(_current.LeftIndex);
					if (_current.RightIndex > -1) _stack.Push(_current.RightIndex);
				}
				else
				{
					if (_current.RightIndex > -1) _stack.Push(_current.RightIndex);
					if (_current.LeftIndex > -1) _stack.Push(_current.LeftIndex);
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current.Index = -1;
				_started = false;
				_stack.Clear();
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct InOrderEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly BinaryHeap<TNode, TKey, TValue> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private Navigator _current;
			private bool _started;
			private bool _done;

			internal InOrderEnumerator([NotNull] BinaryHeap<TNode, TKey, TValue> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_stack = new Stack<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.NewNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			[NotNull]
			public TValue Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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
				if (_version != _tree._version) throw new VersionChangedException();

				// Left-Root-Right (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_current.Index = _index;
				}
				else
				{
					_current.Index = _rightToLeft
									// Navigate left
									? _current.LeftIndex
									// Navigate right
									: _current.RightIndex;
				}

				while (_current.Index > -1 || _stack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current.Index > -1)
					{
						_stack.Push(_current.Index);
						_current.Index = _rightToLeft
										// Navigate right
										? _current.RightIndex
										// Navigate left
										: _current.LeftIndex;
					}
					else
					{
						// visit the next queued node
						_current.Index = _stack.Pop();
						break; // break from the loop to visit this node
					}
				}

				_done = _current.Index < 0;
				return !_done;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current.Index = -1;
				_started = false;
				_stack.Clear();
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct PostOrderEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly BinaryHeap<TNode, TKey, TValue> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private Navigator _current;
			private bool _started;
			private bool _done;

			internal PostOrderEnumerator([NotNull] BinaryHeap<TNode, TKey, TValue> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_stack = new Stack<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.NewNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			[NotNull]
			public TValue Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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
				if (_version != _tree._version) throw new VersionChangedException();

				// Left-Right-Root (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_current.Index = _index;
				}
				else
				{
					_current.Index = -1;
				}

				do
				{
					while (_current.Index > -1)
					{
						if (_version != _tree._version) throw new VersionChangedException();

						if (_rightToLeft)
						{
							// Navigate left
							if (_current.LeftIndex > -1) _stack.Push(_current.LeftIndex);
						}
						else
						{
							// Navigate right
							if (_current.RightIndex > -1) _stack.Push(_current.RightIndex);
						}

						_stack.Push(_current.Index);
						_current.Index = _rightToLeft
										? _current.RightIndex // Navigate right
										: _current.LeftIndex; // Navigate left
					}

					if (_version != _tree._version) throw new VersionChangedException();
					_current.Index = _stack.Count > 0
									? _stack.Pop()
									: -1;
					if (_current.Index < 0) continue;

					/*
					* if Current has a right child and is not processed yet,
					* then make sure right child is processed before root
					*/
					if (_rightToLeft)
					{
						if (_current.LeftIndex > -1 && _stack.Count > 0 && _current.LeftIndex == _stack.Peek())
						{
							// remove left child from stack
							_stack.Pop();
							// push Current back to stack
							_stack.Push(_current.Index);
							// process left first
							_current.Index = _current.LeftIndex;
							continue;
						}
					}
					else
					{
						if (_current.RightIndex > -1 && _stack.Count > 0 && _current.RightIndex == _stack.Peek())
						{
							// remove right child from stack
							_stack.Pop();
							// push Current back to stack
							_stack.Push(_current.Index);
							// process right first
							_current.Index = _current.RightIndex;
							continue;
						}
					}

					if (_current.Index > -1)
						break; // break from the loop to visit this node
				} while (_stack.Count > 0);

				_done = _current.Index < 0;
				return !_done;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current.Index = -1;
				_started = false;
				_stack.Clear();
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private int _version;

		[NonSerialized]
		private object _syncRoot;

		/// <inheritdoc />
		protected BinaryHeap() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeap(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeap(IComparer<TKey> comparer)
			: this(0, comparer)
		{
		}

		protected BinaryHeap(int capacity, IComparer<TKey> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Comparer = comparer ?? Comparer<TKey>.Default;
			Items = capacity == 0
						? Array.Empty<TNode>()
						: new TNode[capacity];
		}

		/// <inheritdoc />
		protected BinaryHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(0, comparer)
		{
			Add(enumerable);
		}

		public int Capacity
		{
			get => Items.Length;
			set
			{
				if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
				if (value == Items.Length) return;

				if (value > 0)
				{
					TNode[] newItems = new TNode[value];
					if (Count > 0) Array.Copy(Items, 0, newItems, 0, Count);
					Items = newItems;
				}
				else
				{
					Items = Array.Empty<TNode>();
				}

				_version++;
			}
		}

		/// <inheritdoc />
		public IComparer<TKey> Comparer { get; }

		[NotNull]
		protected EqualityComparer<TValue> ValueComparer { get; } = EqualityComparer<TValue>.Default;

		/// <inheritdoc cref="ICollection{TValue}" />
		public int Count { get; protected set; }

		[NotNull]
		protected TNode[] Items { get; private set; }

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
		public IEnumerator<TValue> GetEnumerator() { return Enumerate(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public Navigator NewNavigator(int index = -1)
		{
			return new Navigator(this, index);
		}

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <returns><see cref="IEnumerableEnumerator{TValue}"/></returns>
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(int index, TreeTraverseMethod method, bool rightToLeft)
		{
			return method switch
			{
				TreeTraverseMethod.LevelOrder => new LevelOrderEnumerator(this, index, rightToLeft),
				TreeTraverseMethod.PreOrder => new PreOrderEnumerator(this, index, rightToLeft),
				TreeTraverseMethod.InOrder => new InOrderEnumerator(this, index, rightToLeft),
				TreeTraverseMethod.PostOrder => new PostOrderEnumerator(this, index, rightToLeft),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate()
		{
			return Enumerate(0, TreeTraverseMethod.InOrder, false);
		}

		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TreeTraverseMethod method)
		{
			return Enumerate(0, method, false);
		}

		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(bool rightToLeft)
		{
			return Enumerate(0, TreeTraverseMethod.InOrder, rightToLeft);
		}

		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(int index)
		{
			return Enumerate(index, TreeTraverseMethod.InOrder, false);
		}

		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(int index, bool rightToLeft)
		{
			return Enumerate(index, TreeTraverseMethod.InOrder, rightToLeft);
		}

		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(int index, TreeTraverseMethod method)
		{
			return Enumerate(index, method, false);
		}

		[NotNull]
		public IEnumerableEnumerator<TValue> Enumerate(TreeTraverseMethod method, bool rightToLeft)
		{
			return Enumerate(0, method, rightToLeft);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method <see cref="TreeTraverseMethod"/></param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(int index, TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<int> visitCallback)
		{
			if (!index.InRangeRx(0, Count)) return;

			switch (method)
			{
				case TreeTraverseMethod.LevelOrder:
					LevelOrder(index, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PreOrder:
					PreOrder(index, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.InOrder:
					InOrder(index, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PostOrder:
					PostOrder(index, visitCallback, rightToLeft);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Action<int> visitCallback)
		{
			Iterate(0, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, [NotNull] Action<int> visitCallback)
		{
			Iterate(0, method, false, visitCallback);
		}

		public void Iterate(bool rightToLeft, [NotNull] Action<int> visitCallback)
		{
			Iterate(0, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(int index, [NotNull] Action<int> visitCallback)
		{
			Iterate(index, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(int index, bool rightToLeft, [NotNull] Action<int> visitCallback)
		{
			Iterate(index, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(int index, TreeTraverseMethod method, [NotNull] Action<int> visitCallback)
		{
			Iterate(index, method, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<int> visitCallback)
		{
			Iterate(0, method, rightToLeft, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method <see cref="TreeTraverseMethod"/></param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(int index, TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<int, bool> visitCallback)
		{
			if (!index.InRangeRx(0, Count)) return;

			switch (method)
			{
				case TreeTraverseMethod.LevelOrder:
					LevelOrder(index, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PreOrder:
					PreOrder(index, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.InOrder:
					InOrder(index, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PostOrder:
					PostOrder(index, visitCallback, rightToLeft);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Iterate overloads - visitCallback function
		public void Iterate([NotNull] Func<int, bool> visitCallback)
		{
			Iterate(0, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(0, method, false, visitCallback);
		}

		public void Iterate(bool rightToLeft, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(0, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(int index, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(index, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(int index, bool rightToLeft, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(index, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(int index, TreeTraverseMethod method, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(index, method, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(0, method, rightToLeft, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback action to handle the nodes of the level.</param>
		public void IterateLevels(int index, bool rightToLeft, [NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
		{
			// Root-Left-Right (Queue)
			if (!index.InRangeRx(0, Count)) return;

			int version = _version;
			int level = 0;
			Queue<int> queue = new Queue<int>(GetCapacityForLevelQueueing(this));
			// Start at the root
			Navigator current = NewNavigator(index);
			queue.Enqueue(current.Index);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();
				levelCallback(level, queue);

				int count = queue.Count;
				level++;

				for (int i = 0; i < count; i++)
				{
					// visit the next queued node
					current.Index = queue.Dequeue();

					// Queue the next nodes
					if (rightToLeft)
					{
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
					}
					else
					{
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
					}
				}
			}
		}

		#region LevelIterate overloads - visitCallback action
		public void IterateLevels([NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
		{
			IterateLevels(0, false, levelCallback);
		}

		public void IterateLevels(bool rightToLeft, [NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
		{
			IterateLevels(0, rightToLeft, levelCallback);
		}

		public void IterateLevels(int index, [NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
		{
			IterateLevels(index, false, levelCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback function to handle the nodes of the level and can cancel the loop.</param>
		public void IterateLevels(int index, bool rightToLeft, [NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
		{
			// Root-Left-Right (Queue)
			if (!index.InRangeRx(0, Count)) return;

			int version = _version;
			int level = 0;
			Queue<int> queue = new Queue<int>(GetCapacityForLevelQueueing(this));
			// Start at the root
			Navigator current = NewNavigator(index);
			queue.Enqueue(current.Index);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();
				levelCallback(level, queue);

				int count = queue.Count;
				level++;

				for (int i = 0; i < count; i++)
				{
					// visit the next queued node
					current.Index = queue.Dequeue();

					// Queue the next nodes
					if (rightToLeft)
					{
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
					}
					else
					{
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
					}
				}
			}
		}

		#region LevelIterate overloads - visitCallback function
		public void IterateLevels([NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
		{
			IterateLevels(0, false, levelCallback);
		}

		public void IterateLevels(bool rightToLeft, [NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
		{
			IterateLevels(0, rightToLeft, levelCallback);
		}

		public void IterateLevels(int index, [NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
		{
			IterateLevels(index, false, levelCallback);
		}
		#endregion

		/// <inheritdoc />
		public abstract TNode MakeNode(TValue value);

		public virtual bool Equals(BinaryHeap<TNode, TKey, TValue> other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() || Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			for (int i = 0; i < Count; i++)
			{
				if (ValueComparer.Equals(Items[i].Value, other.Items[i].Value)) continue;
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public bool Contains(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return IndexOf(value) > -1;
		}

		public bool Exists([NotNull] Predicate<TValue> match) { return Count > 0 && Array.FindIndex(Items, 0, Count, e => match(e.Value)) > -1; }

		/// <summary>
		/// Finds the node's index with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node's index or -1 if no match is found</returns>
		public int IndexOf([NotNull] TValue value)
		{
			return Count == 0
						? -1
						: Array.FindIndex(Items, 0, Count, e => ValueComparer.Equals(e.Value, value));
		}

		public int IndexOfByKey([NotNull] TKey key)
		{
			return Count == 0
						? -1
						: Array.FindIndex(Items, 0, Count, e => Comparer.IsEqual(e.Key, key));
		}

		/// <inheritdoc />
		public TNode Find(TValue value)
		{
			int index = IndexOf(value);
			return index < 0
						? null
						: Items[index];
		}

		/// <inheritdoc />
		public TNode FindByKey(TKey key)
		{
			int index = IndexOfByKey(key);
			return index < 0
						? null
						: Items[index];
		}

		/// <inheritdoc />
		public void Add(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			Add(MakeNode(value));
		}

		/// <inheritdoc />
		public TNode Add(TNode node)
		{
			if (Count == Items.Length) EnsureCapacity(Count + 1);
			Items[Count] = node;
			Count++;
			_version++;
			if (Count > 1) BubbleUp(Count - 1);
			return node;
		}

		public void Add(IEnumerable<TValue> values)
		{
			foreach (TValue value in values) 
				Add(value);
		}

		/// <inheritdoc />
		public bool Remove(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return RemoveNode(IndexOf(value));
		}

		/// <inheritdoc />
		public bool Remove(TNode node)
		{
			return Count > 0 && RemoveNode(Array.IndexOf(Items, node, 0, Count));
		}

		/// <inheritdoc cref="ICollection{T}" />
		public void Clear()
		{
			if (Count == 0) return;
			Array.Clear(Items, 0, Count);
			Count = 0;
			_version++;
		}

		/// <inheritdoc />
		public void DecreaseKey(TNode node, TKey newKey)
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			int index = Array.IndexOf(Items, node, 0, Count);
			if (index < 0) throw new NotFoundException();
			DecreaseKeyInternal(index, newKey);
		}

		public void DecreaseKey(int index, [NotNull] TKey newKey)
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			DecreaseKeyInternal(index, newKey);
		}

		/// <inheritdoc />
		public TValue Value()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			return Items[0].Value;
		}

		/// <inheritdoc />
		TValue IHeap<TValue>.ExtractValue() { return ExtractValue().Value; }

		/// <inheritdoc />
		public TNode ExtractValue()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			TNode node = Items[0];
			Items[0] = Items[Count - 1];
			Count--;
			_version++;
			if (Count > 1) BubbleDown(0);
			return node;
		}

		/// <inheritdoc />
		public TValue ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++) 
				ExtractValue();

			return Value();
		}

		public void TrimExcess()
		{
			int threshold = (int)(Items.Length * 0.9);
			if (Count >= threshold) return;
			Capacity = Count;
		}

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<TValue, TOutput> converter)
		{
			for (int i = 0; i < Count; i++)
				yield return converter(Items[i].Value);
		}

		public virtual TValue LeftMost()
		{
			/*
			 * This tree might not be a valid binary search tree. So a traversal is needed to search the entire tree.
			 * In the overriden method of the BinarySearchTree (and any similar type of tree), this implementation
			 * just grabs the root's left most node's value.
			 */
			if (Count == 0) return default(TValue);

			int index = -1, next = 0;

			while (next > -1)
			{
				index = next;
				next = LeftIndex(next);
			}

			return index > -1
						? Items[index].Value
						: default(TValue);
		}

		public virtual TValue RightMost()
		{
			if (Count == 0) return default(TValue);

			int index = -1, next = 0;

			while (next > -1)
			{
				index = next;
				next = RightIndex(next);
			}

			return index > -1
						? Items[index].Value
						: default(TValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public int ParentIndex(int index)
		{
			// Even() => Math.Floor(1 + Math.Pow(-1, index) / 2) => produces 0 for odd and 1 for even numbers
			return NormalizeIndex(index <= 0
									? -1
									: (index - (1 + index.Even())) / 2);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public int LeftIndex(int index)
		{
			return NormalizeIndex(index < 0
									? -1
									: index * 2 + 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public int RightIndex(int index)
		{
			return NormalizeIndex(index < 0
									? -1
									: index * 2 + 2);
		}

		public int GetHeight()
		{
			/*
			 * https://ece.uwaterloo.ca/~cmoreno/ece250/4.05.PerfectBinaryTrees.pdf
			 * of course heaps are complete trees, not full or perfect trees but the same
			 * rule apply for height.
			 *
			 * Full Binary Tree: every node has 0 or 2 children.
			 *
			 *	     ___ F ____
			 *	    /          \
			 *	   C            I
			 *	 /   \
			 *	A     D
			 *
			 * Complete Binary Tree: all levels are completely filled except possibly the
			 * last level and the last level has all keys as left as possible.
			 *
			 *	     ___ F ____
			 *	    /          \
			 *	   C            I
			 *	 /   \        /
			 *	A     D      G
			 *
			 * Perfect Binary Tree: all internal nodes have two children and all leaves are
			 * at same level.
			 *
			 *	     ___ F ____
			 *	    /          \
			 *	   C            I
			 *	 /   \        /   \
			 *	A     D      G     J
			 */
			return Count == 0
						? 0
						: (int)Math.Ceiling(Math.Log(Count + 1, 2) - 1);
		}

		public IReadOnlyCollection<int> GeNodesAtLevel(int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Count == 0) return null;

			IReadOnlyCollection<int> collection = null;
			IterateLevels((lvl, indexes) =>
			{
				if (lvl < level) return true;
				if (lvl == level) collection = indexes;
				return false;
			});
			return collection;
		}

		/// <inheritdoc />
		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(e => array[arrayIndex++] = Items[e].Value);
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
			if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));
			Iterate(e => objects[index++] = Items[e].Value);
		}

		internal void CopyTo(TNode[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			// Delegate rest of error checking to Array.Copy.
			Array.Copy(Items, 0, array, arrayIndex, Count);
		}

		[NotNull]
		public TValue[] ToArray(TreeTraverseMethod method = TreeTraverseMethod.InOrder, bool rightToLeft = false)
		{
			switch (Count)
			{
				case 0:
					return Array.Empty<TValue>();
				case 1:
					return new[] { Items[0].Value };
				default:
					int index = 0;
					TValue[] array = new TValue[Count];
					Iterate(method, rightToLeft, e => array[index++] = Items[e].Value);
					return array;
			}
		}

		[ItemNotNull]
		public IEnumerable<TValue> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			int last = index + count;

			for (int i = index; i < last; i++)
				yield return Items[i].Value;
		}

		public void ForEach([NotNull] Action<TValue> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i].Value);
			}

			if (version != _version) throw new VersionChangedException();
		}

		public void ForEach([NotNull] Action<TValue, int> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i].Value, i);
			}

			if (version != _version) throw new VersionChangedException();
		}

		public void Reverse() { Reverse(0, Count); }
		public void Reverse(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (Count < 2 || count < 2) return;
			Array.Reverse(Items, index, count);
			_version++;
		}

		public bool TrueForAll([NotNull] Predicate<TValue> match)
		{
			for (int i = 0; i < Count; i++)
			{
				if (match(Items[i].Value)) continue;
				return false;
			}
			return true;
		}

		protected void EnsureCapacity(int min)
		{
			if (Items.Length >= min) return;
			Capacity = (Items.Length == 0 ? Constants.DEFAULT_CAPACITY : Items.Length * 2).NotBelow(min);
		}

		protected bool RemoveNode(int index)
		{
			if (Count == 0 || index < 0) return false;
			BubbleUp(index, true);
			ExtractValue();
			return true;
		}

		protected void BubbleUp(int index, bool toRoot = false)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (index == 0) return;

			bool changed = false;
			Navigator node = NewNavigator(index);

			// the parent's value must be greater than its children so move the greater value up.
			while (node.ParentIndex > -1 && (toRoot || Compare(Items[node.ParentIndex].Key, Items[node.Index].Key) > 0))
			{
				Swap(node.Index, node.ParentIndex);
				node.Index = node.ParentIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		protected void BubbleDown(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			/*
			 * the parent's value must be greater than its children.
			 * move the smaller value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the greater value.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && Compare(Items[node.LeftIndex].Key, Items[childIndex].Key) < 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && Compare(Items[node.RightIndex].Key, Items[childIndex].Key) < 0) childIndex = node.RightIndex;
				if (childIndex == node.Index) break;
				Swap(node.Index, childIndex);
				node.Index = childIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected internal void Swap(int x, int y)
		{
			TNode tmp = Items[x];
			Items[x] = Items[y];
			Items[y] = tmp;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private int NormalizeIndex(int value)
		{
			return !value.InRangeRx(0, Count)
						? -1
						: value;
		}

		private void DecreaseKeyInternal(int index, [NotNull] TKey newKey)
		{
			if (Compare(Items[index].Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			Items[index].Key = newKey;
			if (index == 0) return;
			BubbleUp(index);
		}

		#region Iterator Traversal for Action<int>
		private void LevelOrder(int index, [NotNull] Action<int> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Root-Left-Right (Queue)
			Queue<int> queue = new Queue<int>();
			Navigator current = NewNavigator(index);

			// Start at the root
			queue.Enqueue(current.Index);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				current.Index = queue.Dequeue();
				visitCallback(current.Index);

				// Queue the next nodes
				if (rightToLeft)
				{
					if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
					if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
				}
				else
				{
					if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
					if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
				}
			}
		}

		private void PreOrder(int index, [NotNull] Action<int> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Root-Left-Right (Stack)
			Stack<int> stack = new Stack<int>();
			Navigator current = NewNavigator(index);

			// Start at the root
			stack.Push(current.Index);

			while (stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				current.Index = stack.Pop();
				visitCallback(current.Index);

				// Queue the next nodes
				if (rightToLeft)
				{
					if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
					if (current.RightIndex > -1) stack.Push(current.RightIndex);
				}
				else
				{
					if (current.RightIndex > -1) stack.Push(current.RightIndex);
					if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
				}
			}
		}

		private void InOrder(int index, [NotNull] Action<int> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Left-Root-Right (Stack)
			Stack<int> stack = new Stack<int>();
			// Start at the root
			Navigator current = NewNavigator(index);

			while (current.Index > -1 || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current.Index > -1)
				{
					stack.Push(current.Index);
					current.Index = rightToLeft
										? current.RightIndex // Navigate right
										: current.LeftIndex; // Navigate left
				}
				else
				{
					// visit the next queued node
					current.Index = stack.Pop();
					visitCallback(current.Index);
					current.Index = rightToLeft
										? current.LeftIndex // Navigate left
										: current.RightIndex; // Navigate right
				}
			}
		}

		private void PostOrder(int index, [NotNull] Action<int> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Left-Right-Root (Stack)
			Stack<int> stack = new Stack<int>();
			// Start at the root
			Navigator current = NewNavigator(index);
			int lastVisited = -1;

			while (current.Index > -1 || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current.Index > -1)
				{
					stack.Push(current.Index);
					current.Index = rightToLeft
										? current.RightIndex // Navigate right
										: current.LeftIndex; // Navigate left
					continue;
				}

				int peek = stack.Peek();

				if (rightToLeft)
				{
					/*
					* At this point we are either coming from
					* either the root node or the right branch.
					* Is there a left node?
					* if yes, then navigate left.
					*/
					int left = LeftIndex(peek);

					if (left > -1 && lastVisited != left)
					{
						// Navigate left
						current.Index = left;
					}
					else
					{
						// visit the next queued node
						current.Index = peek;
						lastVisited = stack.Pop();
						visitCallback(current.Index);
						current.Index = -1;
					}
				}
				else
				{
					/*
					* At this point we are either coming from
					* either the root node or the left branch.
					* Is there a right node?
					* if yes, then navigate right.
					*/
					int right = RightIndex(peek);

					if (right > -1 && lastVisited != right)
					{
						// Navigate right
						current.Index = right;
					}
					else
					{
						// visit the next queued node
						current.Index = peek;
						lastVisited = stack.Pop();
						visitCallback(current.Index);
						current.Index = -1;
					}
				}
			}
		}
		#endregion

		#region Iterator Traversal for Func<int, bool>
		private void LevelOrder(int index, [NotNull] Func<int, bool> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Root-Left-Right (Queue)
			Queue<int> queue = new Queue<int>();
			Navigator current = NewNavigator(index);

			// Start at the root
			queue.Enqueue(current.Index);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				current.Index = queue.Dequeue();
				if (!visitCallback(current.Index)) break;

				// Queue the next nodes
				if (rightToLeft)
				{
					if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
					if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
				}
				else
				{
					if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
					if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
				}
			}
		}

		private void PreOrder(int index, [NotNull] Func<int, bool> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Root-Left-Right (Stack)
			Stack<int> stack = new Stack<int>();
			Navigator current = NewNavigator(index);

			// Start at the root
			stack.Push(current.Index);

			while (stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				current.Index = stack.Pop();
				if (!visitCallback(current.Index)) break;

				// Queue the next nodes
				if (rightToLeft)
				{
					if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
					if (current.RightIndex > -1) stack.Push(current.RightIndex);
				}
				else
				{
					if (current.RightIndex > -1) stack.Push(current.RightIndex);
					if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
				}
			}
		}
		private void InOrder(int index, [NotNull] Func<int, bool> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Left-Root-Right (Stack)
			Stack<int> stack = new Stack<int>();
			// Start at the root
			Navigator current = NewNavigator(index);

			while (current.Index > -1 || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current.Index > -1)
				{
					stack.Push(current.Index);
					current.Index = rightToLeft
										? current.RightIndex // Navigate right
										: current.LeftIndex; // Navigate left
				}
				else
				{
					// visit the next queued node
					current.Index = stack.Pop();
					if (!visitCallback(current.Index)) break;
					current.Index = rightToLeft
										? current.LeftIndex // Navigate left
										: current.RightIndex; // Navigate right
				}
			}
		}
		private void PostOrder(int index, [NotNull] Func<int, bool> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Left-Right-Root (Stack)
			Stack<int> stack = new Stack<int>();
			// Start at the root
			Navigator current = NewNavigator(index);
			int lastVisited = -1;

			while (current.Index > -1 || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current.Index > -1)
				{
					stack.Push(current.Index);
					current.Index = rightToLeft
										? current.RightIndex // Navigate right
										: current.LeftIndex; // Navigate left
					continue;
				}

				int peek = stack.Peek();

				if (rightToLeft)
				{
					/*
					* At this point we are either coming from
					* either the root node or the right branch.
					* Is there a left node?
					* if yes, then navigate left.
					*/
					int left = LeftIndex(peek);

					if (left > -1 && lastVisited != left)
					{
						// Navigate left
						current.Index = left;
					}
					else
					{
						// visit the next queued node
						current.Index = peek;
						lastVisited = stack.Pop();
						if (!visitCallback(current.Index)) break;
						current.Index = -1;
					}
				}
				else
				{
					/*
					* At this point we are either coming from
					* either the root node or the left branch.
					* Is there a right node?
					* if yes, then navigate right.
					*/
					int right = RightIndex(peek);

					if (right > -1 && lastVisited != right)
					{
						// Navigate right
						current.Index = right;
					}
					else
					{
						// visit the next queued node
						current.Index = peek;
						lastVisited = stack.Pop();
						if (!visitCallback(current.Index)) break;
						current.Index = -1;
					}
				}
			}
		}
		#endregion

		protected static int GetCapacityForQueueing([NotNull] BinaryHeap<TNode, TKey, TValue> heap)
		{
			/* The maximum height of a red-black tree is 2*lg(n+1) which is worse than
			* avl tree. The binary search tree, if is skewed, could be worse. I'll take
			* the red-black tree as the worst case for height.
			* Taken from Microsoft's SortedSet comments.
			*/
			return heap.Count == 0
						? 0
						: 2 * (int)Math.Log(heap.Count + 1, 2);
		}

		protected static int GetCapacityForLevelQueueing([NotNull] BinaryHeap<TNode, TKey, TValue> tree)
		{
			/*
			 * capacity:
			 * 1. Maximum number of nodes in the last level k = 2 ^ h where h = height of the tree.
			 * 2. Maximum number of nodes will be when all levels are completely full.
			 * 3. n should have an estimate value of n = 2 ^ (h + 1) - 1.
			 * 4. h can be found by using: h = log2(n + 1) - 1.
			 * 5. from 1, 3, and 4 k = 2 ^ log2(n + 1) - 1
			 */
			return tree.Count == 0
						? 0
						: (int)Math.Pow(2, Math.Log(tree.Count + 1, 2) - 1);
		}
	}

	[DebuggerTypeProxy(typeof(BinaryHeap<,>.DebugView))]
	[Serializable]
	public abstract class BinaryHeap<TKey, TValue> : BinaryHeap<KeyedBinaryNode<TKey, TValue>, TKey, TValue>
	{
		internal sealed class DebugView : Dbg_BinaryHeapDebugView<KeyedBinaryNode<TKey, TValue>, TKey, TValue>
		{
			public DebugView([NotNull] BinaryHeap<TKey, TValue> heap)
				: base(heap)
			{
			}
		}

		[NotNull]
		protected Func<TValue, TKey> _getKeyForItem;

		/// <inheritdoc />
		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, 0, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
			: this(getKeyForItem, capacity, null)
		{
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: this(getKeyForItem, 0, comparer)
		{
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> comparer)
			: base(capacity, comparer)
		{
			_getKeyForItem = getKeyForItem;
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(getKeyForItem, 0, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public override KeyedBinaryNode<TKey, TValue> MakeNode(TValue value) { return new KeyedBinaryNode<TKey, TValue>(_getKeyForItem(value), value); }
	}

	[DebuggerTypeProxy(typeof(BinaryHeap<>.DebugView))]
	[Serializable]
	public abstract class BinaryHeap<T> : BinaryHeap<KeyedBinaryNode<T>, T, T>
	{
		internal sealed class DebugView : Dbg_BinaryHeapDebugView<KeyedBinaryNode<T>, T, T>
		{
			public DebugView([NotNull] BinaryHeap<T> heap)
				: base(heap)
			{
			}
		}

		/// <inheritdoc />
		protected BinaryHeap()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeap(int capacity)
			: this(capacity, null)
		{
		}

		protected BinaryHeap(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		protected BinaryHeap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		protected BinaryHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected BinaryHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: this(0, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public override KeyedBinaryNode<T> MakeNode(T value) { return new KeyedBinaryNode<T>(value); }
	}

	public static class BinaryHeap
	{
		public static void WriteTo<TNode, TKey, TValue>([NotNull] this BinaryHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : KeyedBinaryNode<TNode, TKey, TValue>
		{
			const string STR_BLANK = "   ";
			const string STR_EXT = "│  ";
			const string STR_CONNECTOR_L = "└─ ";
			const string STR_NULL = "<null>";

			if (thisValue.Count == 0) return;

			StringBuilder indent = new StringBuilder();
			LinkedList<(Queue<int> Nodes, int Depth)> nodesList = new LinkedList<(Queue<int> Nodes, int Depth)>();
			BinaryHeap<TNode, TKey, TValue>.Navigator node = thisValue.NewNavigator();
			Queue<int> rootQueue = new Queue<int>(1);
			rootQueue.Enqueue(0);
			nodesList.AddFirst((rootQueue, 0));

			while (nodesList.Count > 0)
			{
				(Queue<int> nodes, int depth) = nodesList.Last.Value;

				if (nodes.Count == 0)
				{
					nodesList.RemoveLast();
					continue;
				}

				node.Index = nodes.Dequeue();
				indent.Length = 0;

				foreach ((Queue<int> Nodes, int Depth) tuple in nodesList)
				{
					if (tuple == nodesList.Last.Value) break;
					indent.Append(tuple.Nodes.Count > 0
									? STR_EXT
									: STR_BLANK);
				}

				writer.Write(indent + STR_CONNECTOR_L);

				if (node.Index < 0)
				{
					writer.WriteLine(STR_NULL);
					continue;
				}

				writer.WriteLine(node.ToString(depth));
				if (node.IsLeaf) continue;

				Queue<int> queue = new Queue<int>(2);
				queue.Enqueue(node.LeftIndex);
				queue.Enqueue(node.RightIndex);
				nodesList.AddLast((queue, depth + 1));
			}
		}

		public static void Heapify<T>([NotNull] IList<T> values) { Heapify(values, 0, -1, false, null); }
		public static void Heapify<T>([NotNull] IList<T> values, bool descending) { Heapify(values, 0, -1, descending, null); }
		public static void Heapify<T>([NotNull] IList<T> values, IComparer<T> comparer) { Heapify(values, 0, -1, false, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, bool descending, IComparer<T> comparer) { Heapify(values, 0, -1, descending, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index) { Heapify(values, index, -1, false, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, bool descending) { Heapify(values, index, -1, descending, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, IComparer<T> comparer) { Heapify(values, index, -1, false, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, bool descending, IComparer<T> comparer) { Heapify(values, index, -1, descending, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count) { Heapify(values, index, count, false, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count, bool descending) { Heapify(values, index, count, descending, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count, IComparer<T> comparer) { Heapify(values, index, count, false, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count, bool descending, IComparer<T> comparer)
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms part 2
			values.Count.ValidateRange(index, ref count);
			if (count < 2 || values.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// Build heap (rearrange array) starting from the last parent
			for (int i = count / 2 - 1; i >= index; i--)
			{
				// Heapify
				for (int parent = i, left = parent * 2 + 1, right = parent * 2 + 2, largest = parent;
					left < count;
					parent = largest, left = parent * 2 + 1, right = parent * 2 + 2, largest = parent)
				{
					// If left child is larger than root 
					if (left < count && comparer.IsGreaterThan(values[left], values[largest])) largest = left;
					// If right child is larger than largest so far 
					if (right < count && comparer.IsGreaterThan(values[right], values[largest])) largest = right;
					if (largest == parent) break;
					values.FastSwap(i, largest);
				}

				if (comparer.IsLessThanOrEqual(values[i], values[(i - 1) / 2])) continue;
				
				// if child is (bigger/smaller) than parent
				for (int child = i, parent = (child - 1) / 2; 
					parent >= index && comparer.IsGreaterThan(values[child], values[parent]); 
					child = parent, parent = (child - 1) / 2)
				{
					values.FastSwap(child, parent);
				}
			}
		}
	}
}
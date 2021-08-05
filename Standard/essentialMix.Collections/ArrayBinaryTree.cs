using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Binary_tree">BinaryTree</see> using the array representation.
	/// </summary>
	/// <typeparam name="T">The element type of the tree</typeparam>
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
	[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
	[Serializable]
	public abstract class ArrayBinaryTree<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection
	{
		/// <summary>
		/// A node metadata used to navigate the tree and print it
		/// </summary>
		[DebuggerDisplay("{Value}")]
		private struct Navigator : IArrayBinaryTreeNavigator<T>
		{
			[NotNull]
			private readonly ArrayBinaryTree<T> _tree;

			private int _index;

			internal Navigator([NotNull] ArrayBinaryTree<T> tree, int index = -1)
				: this()
			{
				_tree = tree;
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

					ParentIndex = _tree.ParentIndex(_index);
					LeftIndex = _tree.LeftIndex(_index);
					RightIndex = _tree.RightIndex(_index);
				}
			}

			public T Value => _tree.Items[Index];

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

			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }

			[NotNull]
			public string ToString(int level)
			{
				return $"{Value} :L{level}";
			}

			public IEnumerable<int> Ancestors()
			{
				if (ParentIndex <= 0) yield break;

				int index = ParentIndex;

				while (index > -1)
				{
					yield return index;
					index = _tree.ParentIndex(index);
				}
			}

			public int LeftMost()
			{
				int index = Index, next = _tree.LeftIndex(index);

				while (next > -1)
				{
					index = next;
					next = _tree.LeftIndex(next);
				}

				return index;
			}

			public int RightMost()
			{
				int index = Index, next = _tree.RightIndex(index);

				while (next > -1)
				{
					index = next;
					next = _tree.RightIndex(next);
				}

				return index;
			}

			[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
			public void Swap(int other)
			{
				T tmp = _tree.Items[other];
				_tree.Items[other] = Value;
				_tree.Items[Index] = tmp;
			}

			public void Invalidate()
			{
				_index = ParentIndex = LeftIndex = RightIndex = -1;
			}
		}

		private struct LevelOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Queue<int> _queue;
			private readonly bool _rightToLeft;
			private readonly IArrayBinaryTreeNavigator<T> _current;

			private bool _started;
			private bool _done;

			internal LevelOrderEnumerator([NotNull] ArrayBinaryTree<T> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_queue = new Queue<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.CreateNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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

		private struct PreOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private readonly IArrayBinaryTreeNavigator<T> _current;
			private bool _started;
			private bool _done;

			internal PreOrderEnumerator([NotNull] ArrayBinaryTree<T> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_stack = new Stack<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.CreateNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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

		private struct InOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private readonly IArrayBinaryTreeNavigator<T> _current;
			private bool _started;
			private bool _done;

			internal InOrderEnumerator([NotNull] ArrayBinaryTree<T> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_stack = new Stack<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.CreateNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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

		private struct PostOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private readonly IArrayBinaryTreeNavigator<T> _current;
			private bool _started;
			private bool _done;

			internal PostOrderEnumerator([NotNull] ArrayBinaryTree<T> tree, int index, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_stack = new Stack<int>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = tree.CreateNavigator(index);
				_started = false;
				_done = !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (!_started || _current.Index < 0) throw new InvalidOperationException();
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
		protected ArrayBinaryTree()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected ArrayBinaryTree(int capacity)
			: this(capacity, null)
		{
		}

		protected ArrayBinaryTree(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		protected ArrayBinaryTree(int capacity, IComparer<T> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Comparer = comparer ?? Comparer<T>.Default;
			Items = capacity == 0
						? Array.Empty<T>()
						: new T[capacity];
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
					T[] newItems = new T[value];
					if (Count > 0) Array.Copy(Items, 0, newItems, 0, Count);
					Items = newItems;
				}
				else
				{
					Items = Array.Empty<T>();
				}

				_version++;
			}
		}

		[NotNull]
		public IComparer<T> Comparer { get; }

		/// <inheritdoc cref="ICollection{T}" />
		public int Count { get; protected set; }

		[NotNull]
		protected T[] Items { get; private set; }

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

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Enumerate(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		[NotNull]
		public IArrayBinaryTreeNavigator<T> CreateNavigator(int index = -1)
		{
			return new Navigator(this, index);
		}

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <returns><see cref="IEnumerableEnumerator{T}"/></returns>
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(int index, TreeTraverseMethod method, bool rightToLeft)
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
		public IEnumerableEnumerator<T> Enumerate()
		{
			return Enumerate(0, TreeTraverseMethod.InOrder, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method)
		{
			return Enumerate(0, method, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(bool rightToLeft)
		{
			return Enumerate(0, TreeTraverseMethod.InOrder, rightToLeft);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(int index)
		{
			return Enumerate(index, TreeTraverseMethod.InOrder, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(int index, bool rightToLeft)
		{
			return Enumerate(index, TreeTraverseMethod.InOrder, rightToLeft);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(int index, TreeTraverseMethod method)
		{
			return Enumerate(index, method, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method, bool rightToLeft)
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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);
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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);
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

		public virtual bool Equals(ArrayBinaryTree<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			for (int i = 0; i < Count; i++)
			{
				if (Comparer.IsEqual(Items[i], other.Items[i])) continue;
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public bool Contains(T value) { return Find(value) > -1; }

		public bool Exists([NotNull] Predicate<T> match) { return Count > 0 && Array.FindIndex(Items, 0, Count, match) > -1; }

		/// <summary>
		/// Finds the node's index with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node's index or -1 if no match is found</returns>
		public int Find(T value)
		{
			return Count == 0
						? -1
						: Array.FindIndex(Items, 0, Count, e => Comparer.IsEqual(e, value));
		}

		/// <summary>
		/// Finds the node's index with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <param name="method">The <see cref="TreeTraverseMethod"/> to use in the search.</param>
		/// <returns>The found node's index or -1 if no match is found</returns>
		public int Find(T value, TreeTraverseMethod method)
		{
			if (Count == 0) return -1;
			int index = -1;
			Iterate(method, e =>
			{
				if (!Comparer.IsEqual(Items[e], value)) return true; // continue the search
				index = e;
				return false;
			});
			return index;
		}

		/// <summary>
		/// Finds the node's index with the specified value in a specific level
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <param name="level">The target level starting from base zero.</param>
		/// <returns>The found node's index or -1 if no match is found</returns>
		public int Find(T value, int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Count == 0) return -1;
			
			int index = Find(value);
			if (index < 0) return -1;
			
			int lvl = 0, parentIndex = index;

			while (parentIndex > -1)
			{
				parentIndex = ParentIndex(parentIndex);
				lvl++;
			}

			return lvl == level
						? index
						: -1;
		}

		/// <summary>
		/// Finds the closest parent node's index relative to the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found parent index or -1 if no match is found</returns>
		public virtual int FindNearestParent(T value)
		{
			if (Count == 0) return -1;
			int index = Find(value);
			return index < 0
						? -1
						: ParentIndex(index);
		}

		/// <inheritdoc />
		public abstract void Add(T value);

		/// <inheritdoc />
		public abstract bool Remove(T value);

		/// <inheritdoc />
		public void Clear()
		{
			if (Count == 0) return;
			Array.Clear(Items, 0, Count);
			Count = 0;
			_version++;
		}

		public void TrimExcess()
		{
			int threshold = (int)(Items.Length * 0.9);
			if (Count >= threshold) return;
			Capacity = Count;
		}

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter)
		{
			for (int i = 0; i < Count; i++)
				yield return converter(Items[i]);
		}

		public virtual T LeftMost()
		{
			/*
			 * This tree might not be a valid binary search tree. So a traversal is needed to search the entire tree.
			 * In the overriden method of the BinarySearchTree (and any similar type of tree), this implementation
			 * just grabs the root's left most node's value.
			 */
			if (Count == 0) return default(T);

			int index = -1, next = 0;

			while (next > -1)
			{
				index = next;
				next = LeftIndex(next);
			}

			return index > -1
						? Items[index]
						: default(T);
		}

		public virtual T RightMost()
		{
			if (Count == 0) return default(T);

			int index = -1, next = 0;

			while (next > -1)
			{
				index = next;
				next = RightIndex(next);
			}

			return index > -1
						? Items[index]
						: default(T);
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

		/// <summary>
		/// Validates the tree nodes
		/// </summary>
		/// <returns></returns>
		public abstract bool Validate();

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
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(e => array[arrayIndex++] = Items[e]);
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
			if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));
			Iterate(e => objects[index++] = Items[e]);
		}

		[NotNull]
		public T[] ToArray(TreeTraverseMethod method = TreeTraverseMethod.InOrder, bool rightToLeft = false)
		{
			switch (Count)
			{
				case 0:
					return Array.Empty<T>();
				case 1:
					return new[] { Items[0] };
				default:
					int index = 0;
					T[] array = new T[Count];
					Iterate(method, rightToLeft, e => array[index++] = Items[e]);
					return array;
			}
		}

		public IEnumerable<T> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			int last = index + count;

			for (int i = index; i < last; i++)
				yield return Items[i];
		}

		public void ForEach([NotNull] Action<T> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i]);
			}

			if (version != _version) throw new VersionChangedException();
		}

		public void ForEach([NotNull] Action<T, int> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i], i);
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

		public bool TrueForAll([NotNull] Predicate<T> match)
		{
			for (int i = 0; i < Count; i++)
			{
				if (match(Items[i])) continue;
				return false;
			}
			return true;
		}

		protected void EnsureCapacity(int min)
		{
			if (Items.Length >= min) return;
			Capacity = (Items.Length == 0 ? Constants.DEFAULT_CAPACITY : Items.Length * 2).NotBelow(min);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected internal void Swap(int first, int second)
		{
			T tmp = Items[first];
			Items[first] = Items[second];
			Items[second] = tmp;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected internal int ParentIndex(int index)
		{
			// Even() => Math.Floor(1 + Math.Pow(-1, index) / 2) => produces 0 for odd and 1 for even numbers
			return NormalizeIndex(index <= 0
						? -1
						: (index - (1 + index.Even())) / 2);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected internal int LeftIndex(int index)
		{
			return NormalizeIndex(index < 0
						? -1
						: index * 2 + 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected internal int RightIndex(int index)
		{
			return NormalizeIndex(index < 0
						? -1
						: index * 2 + 2);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private int NormalizeIndex(int value)
		{
			return !value.InRangeRx(0, Count)
						? -1
						: value;
		}

		#region Iterator Traversal for Action<int>
		private void LevelOrder(int index, [NotNull] Action<int> visitCallback, bool rightToLeft)
		{
			int version = _version;
			// Root-Left-Right (Queue)
			Queue<int> queue = new Queue<int>();
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);

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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);

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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);

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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);
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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);

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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);

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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);

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
			IArrayBinaryTreeNavigator<T> current = CreateNavigator(index);
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

		protected static int GetCapacityForQueueing([NotNull] ArrayBinaryTree<T> tree)
		{
			/* The maximum height of a red-black tree is 2*lg(n+1) which is worse than
			* avl tree. The binary search tree, if is skewed, could be worse. I'll take
			* the red-black tree as the worst case for height.
			* Taken from Microsoft's SortedSet comments.
			*/
			return tree.Count == 0
						? 0
						: 2 * (int)Math.Log(tree.Count + 1, 2);
		}

		protected static int GetCapacityForLevelQueueing([NotNull] ArrayBinaryTree<T> tree)
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

	public static class ArrayBinaryTreeExtension
	{
		public static void WriteTo<T>([NotNull] this ArrayBinaryTree<T> thisValue, [NotNull] TextWriter writer)
		{
			if (thisValue.Count == 0) return;

			StringBuilder indent = new StringBuilder();
			Stack<(int NodeIndex, int Level)> stack = new Stack<(int NodeIndex, int Level)>(1);
			IArrayBinaryTreeNavigator<T> navigator = thisValue.CreateNavigator();
			stack.Push((0, 0));

			while (stack.Count > 0)
			{
				(int nodeIndex, int level) = stack.Pop();
				int n = Constants.INDENT * level;
				navigator.Index = nodeIndex;
	
				if (indent.Length > n) indent.Length = n;
				else if (indent.Length < n) indent.Append(' ', n - indent.Length);

				writer.Write(indent);

				if (navigator.Index < 0)
				{
					writer.WriteLine(Constants.NULL);
					continue;
				}

				writer.WriteLine(navigator.ToString(level));
				if (navigator.IsLeaf) continue;
				stack.Push((navigator.RightIndex, level + 1));
				stack.Push((navigator.LeftIndex, level + 1));
			}
		}
	}
}
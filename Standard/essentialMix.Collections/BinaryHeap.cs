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
	/// <typeparam name="T">The element type of the heap</typeparam>
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
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_BinaryHeapDebugView<,>))]
	public abstract class BinaryHeapBase<TNode, T> : IBinaryHeapBase<TNode, T>, ICollection<T>, IReadOnlyCollection<T>, ICollection
		where TNode : class, ITreeNode<TNode, T>
	{
		/// <summary>
		/// A node metadata used to navigate the tree and print it
		/// </summary>
		[DebuggerDisplay("{Value}")]
		private struct Navigator : IBinaryHeapNavigator<TNode, T>
		{
			[NotNull]
			private readonly BinaryHeapBase<TNode, T> _heap;

			private int _index;

			internal Navigator([NotNull] BinaryHeapBase<TNode, T> heap, int index = -1)
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
			public T Value => _heap.Items[Index].Value;

			public int ParentIndex { get; private set; }

			public int LeftIndex { get; private set; }

			public int RightIndex { get; private set; }

			public bool IsRoot => Index == 0;
			public bool IsLeft => Index > 0 && Index % 2 != 0;
			public bool IsRight => Index > 0 && Index % 2 == 0;
			public bool IsLeaf => LeftIndex < 0 && RightIndex < 0;
			public bool IsNode => LeftIndex > 0 && RightIndex > 0;
			public bool HasOneChild => (LeftIndex > 0) ^ (RightIndex > 0);
			public bool IsFull => LeftIndex > 0 && RightIndex > 0;
			public bool ParentIsRoot => ParentIndex == 0;
			public bool ParentIsLeft => ParentIndex > 0 && ParentIndex % 2 != 0;
			public bool ParentIsRight => ParentIndex > 0 && ParentIndex % 2 == 0;

			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }

			[NotNull]
			public string ToString(int level)
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

			public void Swap(int other)
			{
				(_heap.Items[other], _heap.Items[Index]) = (_heap.Items[Index], _heap.Items[other]);
			}

			public void Invalidate()
			{
				_index = ParentIndex = LeftIndex = RightIndex = -1;
			}
		}

		private struct LevelOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly BinaryHeapBase<TNode, T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Queue<int> _queue;
			private readonly bool _rightToLeft;
			private readonly IBinaryHeapNavigator<TNode, T> _current;

			private bool _started;
			private bool _done;

			internal LevelOrderEnumerator([NotNull] BinaryHeapBase<TNode, T> tree, int index, bool rightToLeft)
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
			[NotNull]
			public T Current
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
			private readonly BinaryHeapBase<TNode, T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private readonly IBinaryHeapNavigator<TNode, T> _current;
			private bool _started;
			private bool _done;

			internal PreOrderEnumerator([NotNull] BinaryHeapBase<TNode, T> tree, int index, bool rightToLeft)
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
			[NotNull]
			public T Current
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
			private readonly BinaryHeapBase<TNode, T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private readonly IBinaryHeapNavigator<TNode, T> _current;
			private bool _started;
			private bool _done;

			internal InOrderEnumerator([NotNull] BinaryHeapBase<TNode, T> tree, int index, bool rightToLeft)
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
			[NotNull]
			public T Current
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
			private readonly BinaryHeapBase<TNode, T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly Stack<int> _stack;
			private readonly bool _rightToLeft;

			private readonly IBinaryHeapNavigator<TNode, T> _current;
			private bool _started;
			private bool _done;

			internal PostOrderEnumerator([NotNull] BinaryHeapBase<TNode, T> tree, int index, bool rightToLeft)
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
			[NotNull]
			public T Current
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

		protected internal int _version;

		[NonSerialized]
		private object _syncRoot;

		/// <inheritdoc />
		protected BinaryHeapBase() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeapBase(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeapBase(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		protected BinaryHeapBase(int capacity, IComparer<T> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Comparer = comparer ?? Comparer<T>.Default;
			Items = capacity == 0
						? Array.Empty<TNode>()
						: new TNode[capacity];
		}

		/// <inheritdoc />
		protected BinaryHeapBase([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeapBase([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
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
		public IComparer<T> Comparer { get; }

		/// <inheritdoc cref="ICollection{TValue}" />
		public int Count { get; protected set; }

		[NotNull]
		protected internal TNode[] Items { get; private set; }

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
		public IBinaryHeapNavigator<TNode, T> CreateNavigator(int index = -1)
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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);
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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);
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
		public abstract TNode MakeNode(T value);

		/// <inheritdoc />
		public bool Contains(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return IndexOf(value) > -1;
		}

		public bool Exists([NotNull] Predicate<T> match) { return Count > 0 && Array.FindIndex(Items, 0, Count, e => match(e.Value)) > -1; }

		/// <summary>
		/// Finds the node's index with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node's index or -1 if no match is found</returns>
		public int IndexOf(T value)
		{
			return Count == 0
						? -1
						: Array.FindIndex(Items, 0, Count, e => Comparer.Compare(e.Value, value) == 0);
		}

		/// <inheritdoc />
		public TNode Find(T value)
		{
			int index = IndexOf(value);
			return index < 0
						? null
						: Items[index];
		}

		/// <inheritdoc />
		public void Add(T value)
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

		public void Add(IEnumerable<T> values)
		{
			foreach (T value in values) 
				Add(value);
		}

		/// <inheritdoc />
		public bool Remove(T value)
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
		public T Value()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			return Items[0].Value;
		}

		/// <inheritdoc />
		public T ExtractValue() { return ExtractNode().Value; }

		/// <inheritdoc />
		public TNode ExtractNode()
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
		public T ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++) 
				ExtractNode();

			return Value();
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
				yield return converter(Items[i].Value);
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
						? Items[index].Value
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
						? Items[index].Value
						: default(T);
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
		public void CopyTo(T[] array, int arrayIndex)
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
		public T[] ToArray(TreeTraverseMethod method = TreeTraverseMethod.InOrder, bool rightToLeft = false)
		{
			switch (Count)
			{
				case 0:
					return Array.Empty<T>();
				case 1:
					return new[] { Items[0].Value };
				default:
					int index = 0;
					T[] array = new T[Count];
					Iterate(method, rightToLeft, e => array[index++] = Items[e].Value);
					return array;
			}
		}

		[ItemNotNull]
		public IEnumerable<T> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			int last = index + count;

			for (int i = index; i < last; i++)
				yield return Items[i].Value;
		}

		public void ForEach([NotNull] Action<T> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i].Value);
			}

			if (version != _version) throw new VersionChangedException();
		}

		public void ForEach([NotNull] Action<T, int> action)
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

		public bool TrueForAll([NotNull] Predicate<T> match)
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
			ExtractNode();
			return true;
		}

		protected abstract int Compare(T x, T y);

		protected abstract void BubbleUp(int index, bool toRoot = false);

		protected abstract void BubbleDown(int index);

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected internal void Swap(int x, int y)
		{
			(Items[x], Items[y]) = (Items[y], Items[x]);
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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);

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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);

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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);

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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);
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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);

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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);

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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);

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
			IBinaryHeapNavigator<TNode, T> current = CreateNavigator(index);
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

		protected static int GetCapacityForQueueing([NotNull] BinaryHeapBase<TNode, T> heap)
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

		protected static int GetCapacityForLevelQueueing([NotNull] BinaryHeapBase<TNode, T> heap)
		{
			/*
			 * capacity:
			 * 1. Maximum number of nodes in the last level k = 2 ^ h where h = height of the tree.
			 * 2. Maximum number of nodes will be when all levels are completely full.
			 * 3. n should have an estimate value of n = 2 ^ (h + 1) - 1.
			 * 4. h can be found by using: h = log2(n + 1) - 1.
			 * 5. from 1, 3, and 4 k = 2 ^ log2(n + 1) - 1
			 */
			return heap.Count == 0
						? 0
						: (int)Math.Pow(2, Math.Log(heap.Count + 1, 2) - 1);
		}
	}

	[Serializable]
	public abstract class BinaryHeap<T> : BinaryHeapBase<BinaryNode<T>, T>, IBinaryHeap<BinaryNode<T>, T>
	{
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
		public override BinaryNode<T> MakeNode(T value) { return new BinaryNode<T>(value); }

		/// <inheritdoc />
		public void DecreaseKey(BinaryNode<T> node, T newValue)
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			int index = Array.IndexOf(Items, node, 0, Count);
			if (index < 0) throw new NotFoundException();
			DecreaseKey(index, newValue);
		}

		public void DecreaseKey(int index, T newValue)
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (Compare(Items[index].Value, newValue) < 0) throw new InvalidOperationException("Invalid new key.");
			Items[index].Value = newValue;
			if (index == 0) return;
			BubbleUp(index);
		}
		
		public bool Equals(BinaryHeap<T> other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() || Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			for (int i = 0; i < Count; i++)
			{
				if (Comparer.Compare(Items[i].Value, other.Items[i].Value) == 0) continue;
				return false;
			}

			return true;
		}
		
		protected sealed override void BubbleUp(int index, bool toRoot = false)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (index == 0) return;

			bool changed = false;
			IBinaryHeapNavigator<BinaryNode<T>, T> node = CreateNavigator(index);

			// the parent's value must be greater than its children so move the greater value up.
			while (node.ParentIndex > -1 && (toRoot || Compare(Items[node.ParentIndex].Value, Items[node.Index].Value) > 0))
			{
				Swap(node.Index, node.ParentIndex);
				node.Index = node.ParentIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}
		
		protected sealed override void BubbleDown(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			IBinaryHeapNavigator<BinaryNode<T>, T> node = CreateNavigator(index);

			/*
			 * the parent's value must be greater than its children.
			 * move the smaller value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the greater value.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && Compare(Items[node.LeftIndex].Value, Items[childIndex].Value) < 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && Compare(Items[node.RightIndex].Value, Items[childIndex].Value) < 0) childIndex = node.RightIndex;
				if (childIndex == node.Index) break;
				Swap(node.Index, childIndex);
				node.Index = childIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}
	}

	[Serializable]
	public abstract class BinaryHeap<TKey, TValue> : BinaryHeapBase<BinaryNode<TKey, TValue>, TValue>, IBinaryHeap<BinaryNode<TKey, TValue>, TKey, TValue>
	{
		/// <inheritdoc />
		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, 0, null, null)
		{
		}

		/// <inheritdoc />
		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
			: this(getKeyForItem, capacity, null, null)
		{
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer)
			: this(getKeyForItem, 0, keyComparer, null)
		{
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(capacity, comparer)
		{
			GetKeyForItem = getKeyForItem;
			KeyComparer = keyComparer ?? Comparer<TKey>.Default;
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null, null)
		{
		}

		protected BinaryHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: this(getKeyForItem, 0, keyComparer, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public IComparer<TKey> KeyComparer { get; }

		[NotNull]
		protected Func<TValue, TKey> GetKeyForItem { get; }

		/// <inheritdoc />
		public sealed override BinaryNode<TKey, TValue> MakeNode(TValue value) { return new BinaryNode<TKey, TValue>(GetKeyForItem(value), value); }

		public bool Equals(BinaryHeap<TKey, TValue> other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() || Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			for (int i = 0; i < Count; i++)
			{
				if (KeyComparer.Compare(Items[i].Key, other.Items[i].Key) == 0 
					&& Comparer.Compare(Items[i].Value, other.Items[i].Value) == 0) continue;
				return false;
			}

			return true;
		}

		public int IndexOfKey(TKey key)
		{
			return Count == 0
						? -1
						: Array.FindIndex(Items, 0, Count, e => KeyComparer.Compare(e.Key, key) == 0);
		}

		/// <inheritdoc />
		public BinaryNode<TKey, TValue> FindByKey(TKey key)
		{
			int index = IndexOfKey(key);
			return index < 0
						? null
						: Items[index];
		}

		/// <inheritdoc />
		public void DecreaseKey(BinaryNode<TKey, TValue> node, TKey newKey)
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			int index = Array.IndexOf(Items, node, 0, Count);
			if (index < 0) throw new NotFoundException();
			DecreaseKey(index, newKey);
		}

		public void DecreaseKey(int index, TKey newKey)
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (KeyCompare(Items[index].Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			Items[index].Key = newKey;
			if (index == 0) return;
			BubbleUp(index);
		}

		protected abstract int KeyCompare(TKey x, TKey y);

		protected sealed override void BubbleUp(int index, bool toRoot = false)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (index == 0) return;

			bool changed = false;
			IBinaryHeapNavigator<BinaryNode<TKey, TValue>, TValue> node = CreateNavigator(index);

			// the parent's value must be greater than its children so move the greater value up.
			while (node.ParentIndex > -1 && (toRoot || KeyCompare(Items[node.ParentIndex].Key, Items[node.Index].Key) > 0))
			{
				Swap(node.Index, node.ParentIndex);
				node.Index = node.ParentIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}
		
		protected sealed override void BubbleDown(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			IBinaryHeapNavigator<BinaryNode<TKey, TValue>, TValue> node = CreateNavigator(index);

			/*
			 * the parent's value must be greater than its children.
			 * move the smaller value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the greater value.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && KeyCompare(Items[node.LeftIndex].Key, Items[childIndex].Key) < 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && KeyCompare(Items[node.RightIndex].Key, Items[childIndex].Key) < 0) childIndex = node.RightIndex;
				if (childIndex == node.Index) break;
				Swap(node.Index, childIndex);
				node.Index = childIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}
	}

	public static class BinaryHeap
	{
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

	public static class BinaryHeapExtension
	{
		public static void WriteTo<TNode, T>([NotNull] this BinaryHeapBase<TNode, T> thisValue, [NotNull] TextWriter writer)
			where TNode : class, ITreeNode<TNode, T>
		{
			if (thisValue.Count == 0) return;

			StringBuilder indent = new StringBuilder();
			Stack<(int NodeIndex, int Level)> stack = new Stack<(int NodeIndex, int Level)>(1);
			IBinaryHeapNavigator<TNode, T> navigator = thisValue.CreateNavigator();
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
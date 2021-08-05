using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Binary_tree">Binary tree</see> using the linked representation.
	/// </summary>
	/// <typeparam name="TNode">The node type. Must inherit from <see cref="LinkedBinaryNode{TNode, T}"/></typeparam>
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
	[DebuggerTypeProxy(typeof(LinkedBinaryTree<,>.DebugView))]
	[Serializable]
	public abstract class LinkedBinaryTree<TNode, T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
		where TNode : LinkedBinaryNode<TNode, T>
	{
		[DebuggerNonUserCode]
		internal sealed class DebugView
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;

			public DebugView([NotNull] LinkedBinaryTree<TNode, T> tree)
			{
				_tree = tree;
			}

			[NotNull]
			public TNode Root => _tree.Root;
		}

		private struct LevelOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;
			private readonly bool _rightToLeft;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal LevelOrderEnumerator([NotNull] LinkedBinaryTree<TNode, T> tree, TNode root, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_root = root;
				_queue = new Queue<TNode>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = null;
				_started = false;
				_done = _root == null;
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
				if (_version != _tree._version) throw new VersionChangedException();
				// Root-Left-Right (Queue)
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
				if (_rightToLeft)
				{
					if (_current.Right != null) _queue.Enqueue(_current.Right);
					if (_current.Left != null) _queue.Enqueue(_current.Left);
				}
				else
				{
					if (_current.Left != null) _queue.Enqueue(_current.Left);
					if (_current.Right != null) _queue.Enqueue(_current.Right);
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_queue.Clear();
				_done = _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct PreOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;
			private readonly bool _rightToLeft;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal PreOrderEnumerator([NotNull] LinkedBinaryTree<TNode, T> tree, TNode root, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_root = root;
				_stack = new Stack<TNode>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = null;
				_started = false;
				_done = _root == null;
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
				if (_version != _tree._version) throw new VersionChangedException();

				// Root-Left-Right (Stack)
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
				if (_rightToLeft)
				{
					if (_current.Left != null) _stack.Push(_current.Left);
					if (_current.Right != null) _stack.Push(_current.Right);
				}
				else
				{
					if (_current.Right != null) _stack.Push(_current.Right);
					if (_current.Left != null) _stack.Push(_current.Left);
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct InOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;
			private readonly bool _rightToLeft;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal InOrderEnumerator([NotNull] LinkedBinaryTree<TNode, T> tree, TNode root, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_root = root;
				_stack = new Stack<TNode>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = null;
				_started = false;
				_done = _root == null;
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
				if (_version != _tree._version) throw new VersionChangedException();

				// Left-Root-Right (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_current = _root;
				}
				else
				{
					_current = _rightToLeft
									// Navigate left
									? _current?.Left
									// Navigate right
									: _current?.Right;
				}

				while (_current != null || _stack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current != null)
					{
						_stack.Push(_current);
						_current = _rightToLeft
										// Navigate right
										? _current.Right
										// Navigate left
										: _current.Left;
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
				if (_version != _tree._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}
		
		private struct PostOrderEnumerator : IEnumerableEnumerator<T>
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;
			private readonly bool _rightToLeft;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal PostOrderEnumerator([NotNull] LinkedBinaryTree<TNode, T> tree, TNode root, bool rightToLeft)
			{
				_tree = tree;
				_version = _tree._version;
				_root = root;
				_stack = new Stack<TNode>(GetCapacityForQueueing(_tree));
				_rightToLeft = rightToLeft;
				_current = null;
				_started = false;
				_done = _root == null;
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
				if (_version != _tree._version) throw new VersionChangedException();

				// Left-Right-Root (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_current = _root;
				}
				else
				{
					_current = null;
				}
	
				do
				{
					while (_current != null)
					{
						if (_version != _tree._version) throw new VersionChangedException();

						if (_rightToLeft)
						{
							// Navigate left
							if (_current.Left != null) _stack.Push(_current.Left);
						}
						else
						{
							// Navigate right
							if (_current.Right != null) _stack.Push(_current.Right);
						}

						_stack.Push(_current);
						_current = _rightToLeft
										? _current.Right // Navigate right
										: _current.Left; // Navigate left
					}

					if (_version != _tree._version) throw new VersionChangedException();
					_current = _stack.Count > 0
									? _stack.Pop()
									: null;
					if (_current == null) continue;

					if (_rightToLeft)
					{
						/*
						* if Current has a left child and is not processed yet,
						* then make sure left child is processed before root
						*/
						if (_current.Left != null && _stack.Count > 0 && _current.Left == _stack.Peek())
						{
							// remove left child from stack
							_stack.Pop();
							// push Current back to stack
							_stack.Push(_current);
							// process left first
							_current = _current.Left;
							continue;
						}
					}
					else
					{
						/*
						* if Current has a right child and is not processed yet,
						* then make sure right child is processed before root
						*/
						if (_current.Right != null && _stack.Count > 0 && _current.Right == _stack.Peek())
						{
							// remove right child from stack
							_stack.Pop();
							// push Current back to stack
							_stack.Push(_current);
							// process right first
							_current = _current.Right;
							continue;
						}
					}
		
					if (_current != null)
						break; // break from the loop to visit this node
				} while (_stack.Count > 0);

				_done = _current == null;
				return !_done;
			}

			void IEnumerator.Reset()
			{
				if (_version != _tree._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		internal int _version;

		[NonSerialized]
		private object _syncRoot;

		/// <inheritdoc />
		protected LinkedBinaryTree()
			: this((IComparer<T>)null)
		{
		}

		protected LinkedBinaryTree(IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;
		}

		protected LinkedBinaryTree([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected LinkedBinaryTree([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: this(comparer)
		{
			Add(enumerable);
		}

		[NotNull]
		public IComparer<T> Comparer { get; }

		public TNode Root { get; protected set; }

		public int Count { get; protected set; }

		public abstract bool AutoBalance { get; }

		public bool IsFull => Root == null || Root.IsFull;

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

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TNode root, TreeTraverseMethod method, bool rightToLeft)
		{
			return method switch
			{
				TreeTraverseMethod.LevelOrder => new LevelOrderEnumerator(this, root, rightToLeft),
				TreeTraverseMethod.PreOrder => new PreOrderEnumerator(this, root, rightToLeft),
				TreeTraverseMethod.InOrder => new InOrderEnumerator(this, root, rightToLeft),
				TreeTraverseMethod.PostOrder => new PostOrderEnumerator(this, root, rightToLeft),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		#region Enumerate overloads
		[NotNull]
		public IEnumerableEnumerator<T> Enumerate()
		{
			return Enumerate(Root, TreeTraverseMethod.InOrder, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TNode root)
		{
			return Enumerate(root, TreeTraverseMethod.InOrder, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method)
		{
			return Enumerate(Root, method, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(bool rightToLeft)
		{
			return Enumerate(Root, TreeTraverseMethod.InOrder, rightToLeft);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TNode root, bool rightToLeft)
		{
			return Enumerate(root, TreeTraverseMethod.InOrder, rightToLeft);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TNode root, TreeTraverseMethod method)
		{
			return Enumerate(root, method, false);
		}

		[NotNull]
		public IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method, bool rightToLeft)
		{
			return Enumerate(Root, method, rightToLeft);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TreeTraverseMethod"/></param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(TNode root, TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<TNode> visitCallback)
		{
			if (root == null) return;

			switch (method)
			{
				case TreeTraverseMethod.LevelOrder:
					LevelOrder(root, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PreOrder:
					PreOrder(root, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.InOrder:
					InOrder(root, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PostOrder:
					PostOrder(root, visitCallback, rightToLeft);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Iterate overloads - visitCallback action
		public void Iterate([NotNull] Action<TNode> visitCallback)
		{
			Iterate(Root, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(Root, method, false, visitCallback);
		}

		public void Iterate(bool rightToLeft, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(Root, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(TNode root, bool rightToLeft, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(TNode root, TreeTraverseMethod method, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, method, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(Root, method, rightToLeft, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TreeTraverseMethod"/></param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(TNode root, TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<TNode, bool> visitCallback)
		{
			if (root == null) return;

			switch (method)
			{
				case TreeTraverseMethod.LevelOrder:
					LevelOrder(root, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PreOrder:
					PreOrder(root, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.InOrder:
					InOrder(root, visitCallback, rightToLeft);
					break;
				case TreeTraverseMethod.PostOrder:
					PostOrder(root, visitCallback, rightToLeft);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Iterate overloads - visitCallback function
		public void Iterate([NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(Root, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(Root, method, false, visitCallback);
		}

		public void Iterate(bool rightToLeft, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(Root, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, TreeTraverseMethod.InOrder, false, visitCallback);
		}

		public void Iterate(TNode root, bool rightToLeft, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, TreeTraverseMethod.InOrder, rightToLeft, visitCallback);
		}

		public void Iterate(TNode root, TreeTraverseMethod method, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, method, false, visitCallback);
		}

		public void Iterate(TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(Root, method, rightToLeft, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback action to handle the nodes of the level.</param>
		public void IterateLevels(TNode root, bool rightToLeft, [NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
		{
			// Root-Left-Right (Queue)
			if (root == null) return;

			int version = _version;
			int level = 0;
			Queue<TNode> queue = new Queue<TNode>(GetCapacityForLevelQueueing(this));
			// Start at the root
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();
				levelCallback(level, queue);

				int count = queue.Count;
				level++;

				for (int i = 0; i < count; i++)
				{
					// visit the next queued node
					TNode current = queue.Dequeue();

					// Queue the next nodes
					if (rightToLeft)
					{
						if (current.Right != null) queue.Enqueue(current.Right);
						if (current.Left != null) queue.Enqueue(current.Left);
					}
					else
					{
						if (current.Left != null) queue.Enqueue(current.Left);
						if (current.Right != null) queue.Enqueue(current.Right);
					}
				}
			}
		}

		#region LevelIterate overloads - visitCallback function
		public void IterateLevels([NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
		{
			IterateLevels(Root, false, levelCallback);
		}

		public void IterateLevels(bool rightToLeft, [NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
		{
			IterateLevels(Root, rightToLeft, levelCallback);
		}

		public void IterateLevels(TNode root, [NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
		{
			IterateLevels(root, false, levelCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="rightToLeft">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback function to handle the nodes of the level and can cancel the loop.</param>
		public void IterateLevels(TNode root, bool rightToLeft, [NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
		{
			// Root-Left-Right (Queue)
			if (root == null) return;

			int version = _version;
			int level = 0;
			Queue<TNode> queue = new Queue<TNode>(GetCapacityForLevelQueueing(this));
			// Start at the root
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();
				if (!levelCallback(level, queue)) break;

				int count = queue.Count;
				level++;

				for (int i = 0; i < count; i++)
				{
					// visit the next queued node
					TNode current = queue.Dequeue();

					// Queue the next nodes
					if (rightToLeft)
					{
						if (current.Right != null) queue.Enqueue(current.Right);
						if (current.Left != null) queue.Enqueue(current.Left);
					}
					else
					{
						if (current.Left != null) queue.Enqueue(current.Left);
						if (current.Right != null) queue.Enqueue(current.Right);
					}
				}
			}
		}

		#region LevelIterate overloads - visitCallback function
		public void IterateLevels([NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
		{
			IterateLevels(Root, false, levelCallback);
		}

		public void IterateLevels(bool rightToLeft, [NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
		{
			IterateLevels(Root, rightToLeft, levelCallback);
		}

		public void IterateLevels(TNode root, [NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
		{
			IterateLevels(root, false, levelCallback);
		}
		#endregion

		public virtual bool Equals<TTree>(TTree other)
			where TTree : LinkedBinaryTree<TNode, T>
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms Part 2
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
			if (Root == null && other.Root == null) return true;
			if (Root == null || other.Root == null) return false;
			return Root.IsLeaf && other.Root.IsLeaf
						? Comparer.IsEqual(Root.Value, other.Root.Value)
						: EqualsLocal(Root, other.Root);

			bool EqualsLocal(TNode x, TNode y)
			{
				return ReferenceEquals(x, y) 
						|| x != null && y != null 
									&& Comparer.IsEqual(x.Value, y.Value) 
									&& EqualsLocal(x.Left, y.Left) 
									&& EqualsLocal(x.Right, y.Right);
			}
		}

		/// <inheritdoc />
		public bool Contains(T value) { return Find(value) != null; }

		public bool Exists([NotNull] Predicate<T> match)
		{
			if (Count == 0) return false;
			bool found = false;
			Iterate(TreeTraverseMethod.LevelOrder, e =>
			{
				found = match(e.Value);
				return !found;
			});
			return found;
		}

		/// <summary>
		/// Finds the node with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node or null if no match is found</returns>
		public TNode Find(T value)
		{
			TNode current = FindNearestParent(value) ?? Root /* in case root is the required node */;
			if (current == null || Comparer.IsEqual(current.Value, value)) return current;
			if (current.Left != null && Comparer.IsEqual(current.Left.Value, value)) return current.Left;
			if (current.Right != null && Comparer.IsEqual(current.Right.Value, value)) return current.Right;
			return null;
		}

		/// <summary>
		/// Finds the node with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <param name="method">The <see cref="TreeTraverseMethod"/> to use in the search.</param>
		/// <returns>The found node or null if no match is found</returns>
		public TNode Find(T value, TreeTraverseMethod method)
		{
			if (Count == 0) return null;
			TNode node = null;
			Iterate(method, e =>
			{
				if (!Comparer.IsEqual(e.Value, value)) return true; // continue the search
				node = e;
				return false;
			});
			return node;
		}

		/// <summary>
		/// Finds the node with the specified value in a specific level
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <param name="level">The target level starting from base zero.</param>
		/// <returns>The found node or null if no match is found</returns>
		public TNode Find(T value, int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Count == 0) return null;
			
			foreach (TNode e in GeNodesAtLevel(level))
			{
				if (Comparer.IsEqual(e.Value, value)) return e;
			}

			return null;
		}

		/// <summary>
		/// Finds the closest parent node relative to the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node or null if no match is found</returns>
		public abstract TNode FindNearestParent(T value);

		/// <inheritdoc />
		public abstract void Add(T value);

		public void Add([NotNull] IEnumerable<T> values)
		{
			foreach (T value in values) 
				Add(value);
		}

		/// <inheritdoc />
		public abstract bool Remove(T value);

		/// <inheritdoc />
		public void Clear()
		{
			Root = null;
			Count = 0;
			_version++;
		}

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter)
		{
			foreach (T item in this)
				yield return converter(item);
		}

		public virtual T Minimum()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			if (Root == null) return default(T);

			/*
			 * This tree might not be a valid binary search tree. So a traversal is needed to search the entire tree.
			 * In the overriden method of the BinarySearchTree (and any similar type of tree), this implementation
			 * just grabs the root's left most node's value.
			 */
			T minimum = Root.Value;
			
			if (Root.Left != null)
			{
				Iterate(Root.Left, TreeTraverseMethod.PreOrder, e =>
				{
					if (Comparer.IsLessThan(minimum, e.Value)) return;
					minimum = e.Value;
				});
			}
			
			if (Root.Right != null)
			{
				Iterate(Root.Right, TreeTraverseMethod.PreOrder, e =>
				{
					if (Comparer.IsLessThan(minimum, e.Value)) return;
					minimum = e.Value;
				});
			}

			return minimum;
		}

		public virtual T Maximum()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			if (Root == null) return default(T);

			/*
			 * This tree might not be a valid binary search tree. So a traversal is needed to search the entire tree.
			 * In the overriden method of the BinarySearchTree (and any similar type of tree), this implementation
			 * just grabs the root's right most node's value.
			 */
			T maximum = Root.Value;
			
			if (Root.Left != null)
			{
				Iterate(Root.Left, TreeTraverseMethod.PreOrder, e =>
				{
					if (Comparer.IsGreaterThan(maximum, e.Value)) return;
					maximum = e.Value;
				});
			}
			
			if (Root.Right != null)
			{
				Iterate(Root.Right, TreeTraverseMethod.PreOrder, e =>
				{
					if (Comparer.IsGreaterThan(maximum, e.Value)) return;
					maximum = e.Value;
				});
			}

			return maximum;
		}

		public TNode Predecessor(T value)
		{
			if (Root == null) return null;

			TNode node = null, root = Root;

			// find the node with the specified value
			// if value is greater, find the value in the right sub-tree
			while (root != null && Comparer.IsGreaterThan(value, root.Value))
			{
				node = root;
				root = root.Right;
			}

			// the maximum value in left subtree is the predecessor node
			if (root != null && Comparer.IsEqual(value, root.Value) && root.Left != null) 
				node = root.Left.RightMost();

			return node;
		}

		public TNode Successor(T value)
		{
			if (Root == null) return null;

			TNode node = null, root = Root;

			// find the node with the specified value
			// if value is lesser, find the value in the left sub-tree
			while (root != null && Comparer.IsLessThan(value, root.Value))
			{
				node = root;
				root = root.Left;
			}

			// the minimum value in right subtree is the successor node
			if (root != null && Comparer.IsEqual(value, root.Value) && root.Right != null) 
				node = root.Right.LeftMost();

			return node;
		}

		public abstract int GetHeight();

		/// <summary>
		/// Validates the tree nodes
		/// </summary>
		/// <returns></returns>
		public bool Validate() { return Root == null || Root.IsLeaf || Validate(Root); }

		/// <summary>
		/// Validates the node and its children.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public abstract bool Validate(TNode node);

		public abstract bool IsBalanced();

		public IReadOnlyCollection<TNode> GeNodesAtLevel(int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Root == null) return null;

			IReadOnlyCollection<TNode> collection = null;
			IterateLevels((lvl, nodes) =>
			{
				if (lvl < level) return true;
				if (lvl == level) collection = nodes;
				return false;
			});
			return collection;
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(e => array[arrayIndex++] = e.Value);
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
			if (Count == 0) return;
			Iterate(e => objects[index++] = e.Value);
		}

		[NotNull]
		public T[] ToArray(TreeTraverseMethod method = TreeTraverseMethod.InOrder, bool rightToLeft = false)
		{
			switch (Count)
			{
				case 0:
					return Array.Empty<T>();
				case 1:
					return new[] { Root.Value };
				default:
					int index = 0;
					T[] array = new T[Count];
					Iterate(method, rightToLeft, e =>
					{
						array[index++] = e.Value;
						return index < Count;
					});
					return array;
			}
		}
		
		/// <summary>
		/// Fill a <see cref="LinkedBinaryTree{TNode,T}"/> from the LevelOrder <see cref="collection"/>.
		/// <para>
		/// LevelOrder => Root-Left-Right (Queue)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		public virtual void FromLevelOrder([NotNull] IEnumerable<T> collection)
		{
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(list)) return;

			int index = 0;
			TNode node = MakeNode(list[index++]);
			Root = node;
			Count++;

			IComparer<T> comparer = Comparer;
			Queue<TNode> queue = new Queue<TNode>();
			queue.Enqueue(node);

			// not all queued items will be parents, it's expected the queue will contain enough nodes
			while (index < list.Count)
			{
				int oldIndex = index;
				TNode root = queue.Dequeue();

				// add left node
				if (comparer.IsLessThan(list[index], root.Value))
				{
					node = MakeNode(list[index]);
					root.Left = node;
					Count++;
					queue.Enqueue(node);
					index++;
				}

				// add right node
				if (index < list.Count && comparer.IsGreaterThanOrEqual(list[index], root.Value))
				{
					node = MakeNode(list[index]);
					root.Right = node;
					Count++;
					queue.Enqueue(node);
					index++;
				}

				if (oldIndex == index) index++;
			}
		}

		/// <summary>
		/// Constructs a <see cref="LinkedBinaryTree{TNode,T}"/> from the PreOrder <see cref="collection"/>.
		/// <para>
		/// PreOrder => Root-Left-Right (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		public virtual void FromPreOrder([NotNull] IEnumerable<T> collection)
		{
			// https://www.geeksforgeeks.org/construct-bst-from-given-preorder-traversal-set-2/
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(list)) return;

			// first node of PreOrder will be root of tree
			TNode node = MakeNode(list[0]);
			Root = node;
			Count++;

			Stack<TNode> stack = new Stack<TNode>();
			// Push root of the BST to the stack i.e, first element of the array.
			stack.Push(node);

			/*
			 * Keep popping nodes while the stack is not empty.
			 * When the value is greater than stack’s top value, make it the right
			 * child of the last popped node and push it to the stack.
			 * If the next value is less than the stack’s top value, make it the left
			 * child of the stack’s top node and push it to the stack.
			 */
			IComparer<T> comparer = Comparer;

			// Traverse from second node
			for (int i = 1; i < list.Count; i++)
			{
				TNode root = null;

				// Keep popping nodes while top of stack is greater.
				while (stack.Count > 0 && comparer.IsGreaterThan(list[i], stack.Peek().Value))
					root = stack.Pop();

				node = MakeNode(list[i]);

				if (root != null)
				{
					root.Right = node;
					Count++;
				}
				else if (stack.Count > 0)
				{
					stack.Peek().Left = node;
					Count++;
				}

				stack.Push(node);
			}
		}

		/// <summary>
		/// Constructs a <see cref="LinkedBinaryTree{TNode,T}"/> from the InOrder <see cref="collection"/>.
		/// <para>
		/// Note that it is not possible to construct a unique binary tree from InOrder collection alone.
		/// </para>
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		public virtual void FromInOrder([NotNull] IEnumerable<T> collection)
		{
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(list)) return;

			int start = 0;
			int end = list.Count - 1;
			int index = IndexMid(start, end);
			TNode node = MakeNode(list[index]);
			Root = node;
			Count++;

			Queue<(int Index, int Start, int End, TNode Node)> queue = new Queue<(int Index, int Start, int End, TNode Node)>();
			queue.Enqueue((index, start, end, node));

			while (queue.Count > 0)
			{
				(int Index, int Start, int End, TNode Node) tuple = queue.Dequeue();

				// get the next left index
				start = tuple.Start;
				end = tuple.Index - 1;
				int nodeIndex = IndexMid(start, end);

				// add left node
				if (nodeIndex > -1)
				{
					node = MakeNode(list[nodeIndex]);
					tuple.Node.Left = node;
					Count++;
					queue.Enqueue((nodeIndex, start, end, node));
				}

				// get the next right index
				start = tuple.Index + 1;
				end = tuple.End;
				nodeIndex = IndexMid(start, end);

				// add right node
				if (nodeIndex > -1)
				{
					node = MakeNode(list[nodeIndex]);
					tuple.Node.Right = node;
					Count++;
					queue.Enqueue((nodeIndex, start, end, node));
				}
			}

			static int IndexMid(int start, int end)
			{
				return start > end
							? -1
							: start + (end - start) / 2;
			}
		}

		/// <summary>
		/// Constructs a <see cref="LinkedBinaryTree{TNode,T}"/> from the PostOrder <see cref="collection"/>.
		/// <para>
		/// PostOrder => Left-Right-Root (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		public virtual void FromPostOrder([NotNull] IEnumerable<T> collection)
		{
			// https://www.geeksforgeeks.org/construct-a-bst-from-given-postorder-traversal-using-stack/
			IReadOnlyList<T> list = new Lister<T>(collection);
			// try simple cases first
			if (FromSimpleList(list)) return;

			// last node of PostOrder will be root of tree
			TNode node = MakeNode(list[list.Count - 1]);
			Root = node;
			Count++;

			Stack<TNode> stack = new Stack<TNode>();
			// Push root of the BST to the stack i.e, last element of the array.
			stack.Push(node);

			/*
			 * The idea is to traverse the array in reverse.
			 * If next element is > the element at the top of the stack then,
			 * set this element as the right child of the element at the top
			 * of the stack and also push it to the stack.
			 * Else if, next element is < the element at the top of the stack then,
			 * start popping all the elements from the stack until either the stack
			 * is empty or the current element becomes > the element at the top of
			 * the stack.
			 * Make this element left child of the last popped node and repeat until
			 * the array is traversed completely.
			 */
			IComparer<T> comparer = Comparer;

			// Traverse from second last node
			for (int i = list.Count - 2; i >= 0; i--)
			{
				TNode root = null;

				// Keep popping nodes while top of stack is greater.
				while (stack.Count > 0 && comparer.IsLessThan(list[i], stack.Peek().Value))
					root = stack.Pop();

				node = MakeNode(list[i]);

				if (root != null)
				{
					root.Left = node;
					Count++;
				}
				else if (stack.Count > 0)
				{
					stack.Peek().Right = node;
					Count++;
				}

				stack.Push(node);
			}
		}

		/// <summary>
		/// Constructs a <see cref="LinkedBinaryTree{TNode,T}"/> from <see cref="inOrderCollection"/> and <see cref="levelOrderCollection"/>.
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// <para>
		/// LevelOrder => Root-Left-Right (Queue)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inOrderCollection"></param>
		/// <param name="levelOrderCollection"></param>
		public virtual void FromInOrderAndLevelOrder([NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> levelOrderCollection)
		{
			IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
			// Root-Left-Right
			IReadOnlyList<T> levelOrder = new Lister<T>(levelOrderCollection);
			if (inOrder.Count != levelOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(levelOrderCollection));
			if (levelOrder.Count == 1 && inOrder.Count == 1 && !Comparer.IsEqual(inOrder[0], levelOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(levelOrderCollection));
			// try simple cases first
			if (FromSimpleList(levelOrder)) return;

			/*
			 * using the facts that:
			 * 1. LevelOrder is organized in the form Root-Left-Right.
			 * 2. InOrder is organized in the form Left-Root-Right,
			 * from 1 and 2, the LevelOrder list can be used to identify
			 * the root and other elements locations in the InOrder list.
			 */
			// the lookup will enhance the speed of looking for the index of the item to O(1)
			IDictionary<T, int> lookup = new Dictionary<T, int>(Comparer.AsEqualityComparer());

			// add all InOrder items to the lookup
			for (int i = 0; i < inOrder.Count; i++)
			{
				T key = inOrder[i];
				if (lookup.ContainsKey(key)) continue;
				lookup.Add(key, i);
			}

			int index = 0;
			TNode node = MakeNode(levelOrder[index++]);
			Root = node;
			Count++;

			Queue<(int Start, int End, TNode Node)> queue = new Queue<(int Start, int End, TNode Node)>();
			queue.Enqueue((0, inOrder.Count - 1, node));

			while (index < levelOrder.Count && queue.Count > 0)
			{
				(int Start, int End, TNode Node) tuple = queue.Dequeue();

				// get the root index (the current node index in the InOrder collection)
				int rootIndex = lookup[tuple.Node.Value];
				// find out the index of the next entry of LevelOrder in the InOrder collection
				int levelIndex = lookup[levelOrder[index]];

				// add left node
				if (levelIndex >= tuple.Start && levelIndex <= rootIndex - 1)
				{
					node = MakeNode(inOrder[levelIndex]);
					tuple.Node.Left = node;
					Count++;
					queue.Enqueue((tuple.Start, rootIndex - 1, node));
					index++;
					// index and node changed, so will need to get the next entry of LevelOrder in the InOrder collection
					levelIndex = index < levelOrder.Count
									? lookup[levelOrder[index]]
									: -1;
				}

				// add right node
				if (levelIndex >= rootIndex + 1 && levelIndex <= tuple.End)
				{
					node = MakeNode(inOrder[levelIndex]);
					tuple.Node.Right = node;
					Count++;
					queue.Enqueue((rootIndex + 1, tuple.End, node));
					index++;
				}
			}
		}

		/// <summary>
		/// Constructs a <see cref="LinkedBinaryTree{TNode,T}"/> from <see cref="inOrderCollection"/> and <see cref="preOrderCollection"/>.
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// <para>
		/// PreOrder => Root-Left-Right (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inOrderCollection"></param>
		/// <param name="preOrderCollection"></param>
		public virtual void FromInOrderAndPreOrder([NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> preOrderCollection)
		{
			IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
			IReadOnlyList<T> preOrder = new Lister<T>(preOrderCollection);
			if (inOrder.Count != preOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(preOrderCollection));
			if (preOrder.Count == 1 && inOrder.Count == 1 && !Comparer.IsEqual(inOrder[0], preOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(preOrderCollection));
			// try simple cases first
			if (FromSimpleList(preOrder)) return;

			/*
			 * https://stackoverflow.com/questions/48352513/construct-binary-tree-given-its-inorder-and-preorder-traversals-without-recursio#48364040
			 * 
			 * The idea is to keep tree nodes in a stack from PreOrder traversal, till their counterpart is not found in InOrder traversal.
			 * Once a counterpart is found, all children in the left sub-tree of the node must have been already visited.
			 */
			int preIndex = 0;
			int inIndex = 0;
			TNode node = MakeNode(preOrder[preIndex++]);
			Root = node;
			Count++;

			IComparer<T> comparer = Comparer;
			Stack<TNode> stack = new Stack<TNode>();
			stack.Push(node);

			while (stack.Count > 0)
			{
				TNode root = stack.Peek();

				if (comparer.IsEqual(root.Value, inOrder[inIndex]))
				{
					stack.Pop();
					inIndex++;

					// if all the elements in inOrder have been visited, we are done
					if (inIndex == inOrder.Count) break;
					// if there are still some unvisited nodes in the left, skip
					if (stack.Count > 0 && comparer.IsEqual(stack.Peek().Value, inOrder[inIndex])) continue;

					/*
					 * As top node in stack, still has not encountered its counterpart
					 * in inOrder, so next element in preOrder must be right child of
					 * the removed node
					 */
					node = MakeNode(preOrder[preIndex++]);
					root.Right = node;
					Count++;
					stack.Push(node);
				}
				else
				{
					/*
					 * Top node in the stack has not encountered its counterpart
					 * in inOrder, so next element in preOrder must be left child
					 * of this node
					 */
					node = MakeNode(preOrder[preIndex++]);
					root.Left = node;
					Count++;
					stack.Push(node);
				}
			}
		}

		/// <summary>
		/// Constructs a <see cref="LinkedBinaryTree{TNode,T}"/> from <see cref="inOrderCollection"/> and <see cref="postOrderCollection"/>.
		/// <para>
		/// InOrder => Left-Root-Right (Stack)
		/// </para>
		/// <para>
		/// PostOrder => Left-Right-Root (Stack)
		/// </para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="inOrderCollection"></param>
		/// <param name="postOrderCollection"></param>
		public virtual void FromInOrderAndPostOrder([NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> postOrderCollection)
		{
			IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
			IReadOnlyList<T> postOrder = new Lister<T>(postOrderCollection);
			if (inOrder.Count != postOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(postOrderCollection));
			if (postOrder.Count == 1 && inOrder.Count == 1 && !Comparer.IsEqual(inOrder[0], postOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(postOrderCollection));
			if (FromSimpleList(postOrder)) return;

			// the lookup will enhance the speed of looking for the index of the item to O(1)
			IDictionary<T, int> lookup = new Dictionary<T, int>(Comparer.AsEqualityComparer());

			// add all InOrder items to the lookup
			for (int i = 0; i < inOrder.Count; i++)
			{
				T key = inOrder[i];
				if (lookup.ContainsKey(key)) continue;
				lookup.Add(key, i);
			}

			// Traverse postOrder in reverse
			int postIndex = postOrder.Count - 1;
			TNode node = MakeNode(postOrder[postIndex--]);
			Root = node;
			Count++;

			Stack<TNode> stack = new Stack<TNode>();
			// Push root of the BST to the stack i.e, last element of the array.
			stack.Push(node);

			IComparer<T> comparer = Comparer;

			while (postIndex >= 0 && stack.Count > 0)
			{
				TNode root = stack.Peek();
				// get the root index (the current node index in the InOrder collection)
				int rootIndex = lookup[root.Value];
				// find out the index of the next entry of PostOrder in the InOrder collection
				int index = lookup[postOrder[postIndex]];

				// add right node
				if (index > rootIndex)
				{
					node = MakeNode(inOrder[index]);
					root.Right = node;
					Count++;
					stack.Push(node);
					postIndex--;

					// index and node changed, so will need to get the next entry of PostOrder in the InOrder collection
					index = postIndex > -1
								? lookup[postOrder[postIndex]]
								: -1;
				}

				// add left node
				if (index > -1 && index < rootIndex)
				{
					if (comparer.IsLessThan(postOrder[postIndex], root.Value))
					{
						// Keep popping nodes while top of stack is greater.
						while (stack.Count > 0 && comparer.IsLessThan(postOrder[postIndex], stack.Peek().Value))
							root = stack.Pop();
					}

					node = MakeNode(inOrder[index]);
					root.Left = node;
					Count++;
					stack.Push(node);
					postIndex--;
				}
			}
		}

		[NotNull]
		protected internal abstract TNode MakeNode(T value);

		#region Iterative Traversal for Action<TNode>
		private void LevelOrder([NotNull] TNode root, [NotNull] Action<TNode> visitCallback, bool rtl)
		{
			int version = _version;
			// Root-Left-Right (Queue)
			Queue<TNode> queue = new Queue<TNode>(GetCapacityForQueueing(this));

			// Start at the root
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				TNode current = queue.Dequeue();
				visitCallback(current);

				// Queue the next nodes
				if (rtl)
				{
					if (current.Right != null) queue.Enqueue(current.Right);
					if (current.Left != null) queue.Enqueue(current.Left);
				}
				else
				{
					if (current.Left != null) queue.Enqueue(current.Left);
					if (current.Right != null) queue.Enqueue(current.Right);
				}
			}
		}

		private void PreOrder([NotNull] TNode root, [NotNull] Action<TNode> visitCallback, bool rtl)
		{
			int version = _version;
			// Root-Left-Right (Stack)
			Stack<TNode> stack = new Stack<TNode>(GetCapacityForQueueing(this));

			// Start at the root
			stack.Push(root);

			while (stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				TNode current = stack.Pop();
				visitCallback(current);

				// Queue the next nodes
				if (rtl)
				{
					if (current.Left != null) stack.Push(current.Left);
					if (current.Right != null) stack.Push(current.Right);
				}
				else
				{
					if (current.Right != null) stack.Push(current.Right);
					if (current.Left != null) stack.Push(current.Left);
				}
			}
		}

		private void InOrder([NotNull] TNode root, [NotNull] Action<TNode> visitCallback, bool rtl)
		{
			int version = _version;
			// Left-Root-Right (Stack)
			Stack<TNode> stack = new Stack<TNode>(GetCapacityForQueueing(this));

			// Start at the root
			TNode current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = rtl
								? current.Right // Navigate right
								: current.Left; // Navigate left
				}
				else
				{
					// visit the next queued node
					current = stack.Pop();
					visitCallback(current);
					current = rtl
								? current.Left // Navigate left
								: current.Right; // Navigate right
				}
			}
		}

		private void PostOrder([NotNull] TNode root, [NotNull] Action<TNode> visitCallback, bool rtl)
		{
			int version = _version;
			// Left-Right-Root (Stack)
			Stack<TNode> stack = new Stack<TNode>(GetCapacityForQueueing(this));
			TNode lastVisited = null;
			// Start at the root
			TNode current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = rtl
								? current.Right // Navigate right
								: current.Left; // Navigate left
					continue;
				}

				TNode peek = stack.Peek();
				
				if (rtl)
				{
					/*
					* At this point we are either coming from
					* either the root node or the right branch.
					* Is there a left node?
					* if yes, then navigate left.
					*/
					if (peek.Left != null && lastVisited != peek.Left)
					{
						// Navigate left
						current = peek.Left;
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
				else
				{
					/*
					* At this point we are either coming from
					* either the root node or the left branch.
					* Is there a right node?
					* if yes, then navigate right.
					*/
					if (peek.Right != null && lastVisited != peek.Right)
					{
						// Navigate right
						current = peek.Right;
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
		}
		#endregion
		
		#region Iterative Traversal for Func<TNode, bool>
		private void LevelOrder([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback, bool rtl)
		{
			int version = _version;
			// Root-Left-Right (Queue)
			Queue<TNode> queue = new Queue<TNode>(GetCapacityForQueueing(this));

			// Start at the root
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				TNode current = queue.Dequeue();
				if (!visitCallback(current)) break;

				// Queue the next nodes
				if (rtl)
				{
					if (current.Right != null) queue.Enqueue(current.Right);
					if (current.Left != null) queue.Enqueue(current.Left);
				}
				else
				{
					if (current.Left != null) queue.Enqueue(current.Left);
					if (current.Right != null) queue.Enqueue(current.Right);
				}
			}
		}

		private void PreOrder([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback, bool rtl)
		{
			int version = _version;
			// Root-Left-Right (Stack)
			Stack<TNode> stack = new Stack<TNode>(GetCapacityForQueueing(this));

			// Start at the root
			stack.Push(root);

			while (stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				// visit the next queued node
				TNode current = stack.Pop();
				if (!visitCallback(current)) break;

				// Queue the next nodes
				if (rtl)
				{
					if (current.Left != null) stack.Push(current.Left);
					if (current.Right != null) stack.Push(current.Right);
				}
				else
				{
					if (current.Right != null) stack.Push(current.Right);
					if (current.Left != null) stack.Push(current.Left);
				}
			}
		}

		private void InOrder([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback, bool rtl)
		{
			int version = _version;
			// Left-Root-Right (Stack)
			Stack<TNode> stack = new Stack<TNode>(GetCapacityForQueueing(this));

			// Start at the root
			TNode current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = rtl
								? current.Right // Navigate right
								: current.Left; // Navigate left
				}
				else
				{
					// visit the next queued node
					current = stack.Pop();
					if (!visitCallback(current)) break;
					current = rtl
								? current.Left // Navigate left
								: current.Right; // Navigate right
				}
			}
		}

		private void PostOrder([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback, bool rtl)
		{
			int version = _version;
			// Left-Right-Root (Stack)
			Stack<TNode> stack = new Stack<TNode>(GetCapacityForQueueing(this));
			TNode lastVisited = null;
			// Start at the root
			TNode current = root;

			while (current != null || stack.Count > 0)
			{
				if (version != _version) throw new VersionChangedException();

				if (current != null)
				{
					stack.Push(current);
					current = rtl
								? current.Right // Navigate right
								: current.Left; // Navigate left
					continue;
				}

				TNode peek = stack.Peek();

				if (rtl)
				{
					/*
					* At this point we are either coming from
					* either the root node or the right branch.
					* Is there a left node?
					* if yes, then navigate left.
					*/
					if (peek.Left != null && lastVisited != peek.Left)
					{
						// Navigate left
						current = peek.Left;
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
				else
				{
					/*
					* At this point we are either coming from
					* either the root node or the left branch.
					* Is there a right node?
					* if yes, then navigate right.
					*/
					if (peek.Right != null && lastVisited != peek.Right)
					{
						// Navigate right
						current = peek.Right;
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
		}
		#endregion

		protected internal static int GetCapacityForQueueing([NotNull] LinkedBinaryTree<TNode, T> tree)
		{
			/*
			 * The maximum height of a red-black tree is 2*lg(n+1) which is worse than
			 * avl tree. The binary search tree, if is skewed, could be worse. I'll take
			 * the red-black tree as the worst case for height.
			 * Taken from Microsoft's SortedSet comments.
			 */
			return tree.Count == 0
						? 0
						: 2 * (int)Math.Log(tree.Count + 1, 2);
		}

		protected internal static int GetCapacityForLevelQueueing([NotNull] LinkedBinaryTree<TNode, T> tree)
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

		private bool FromSimpleList([NotNull] IReadOnlyList<T> list)
		{
			bool result;
			Clear();

			switch (list.Count)
			{
				case 0:
					result = true;
					break;
				case 1:
					Root = MakeNode(list[0]);
					Count++;
					result = true;
					break;
				default:
					result = false;
					break;
			}

			return result;
		}

		private static void ThrowNotFormingATree(string collection1Name, string collection2Name)
		{
			throw new ArgumentException($"{collection1Name} and {collection2Name} do not form a binary tree.");
		}
	}

	/// <inheritdoc />
	[DebuggerTypeProxy(typeof(LinkedBinaryTree<>.DebugView))]
	[Serializable]
	public abstract class LinkedBinaryTree<T> : LinkedBinaryTree<LinkedBinaryNode<T>, T>
	{
		internal new sealed class DebugView
		{
			private readonly LinkedBinaryTree<T> _tree;

			public DebugView([NotNull] LinkedBinaryTree<T> tree)
			{
				_tree = tree;
			}

			[NotNull]
			public LinkedBinaryNode<T> Root => _tree.Root;
		}

		/// <inheritdoc />
		protected LinkedBinaryTree() 
		{
		}

		/// <inheritdoc />
		protected LinkedBinaryTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected LinkedBinaryTree([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		protected LinkedBinaryTree([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected internal override LinkedBinaryNode<T> MakeNode(T value) { return new LinkedBinaryNode<T>(value); }

		/// <inheritdoc />
		public override int GetHeight() { return Root?.Height ?? 0; }

		/*
		 * https://www.geeksforgeeks.org/avl-tree-set-1-insertion/
		 * https://en.wikipedia.org/wiki/Tree_rotation
		 * https://cs.lmu.edu/~ray/notes/binarysearchtrees/
		 *     y                               x
		 *    / \     Right Rotation(y) ->    /  \
		 *   x   T3                          T1   y 
		 *  / \                                  / \
		 * T1  T2     <- Left Rotation(x)       T2  T3
		 *
		 * A reference to the drawing only, not the code
		 */
		[NotNull]
		protected LinkedBinaryNode<T> RotateLeft([NotNull] LinkedBinaryNode<T> node /* x */)
		{
			if (node.Right == null) return node;
			LinkedBinaryNode<T> newRoot /* y */ = node.Right;

			// rotate
			node.Right = newRoot.Left /* T2 */;
			newRoot.Left = node;

			// update
			SetHeight(node);
			SetHeight(newRoot);
			return newRoot;
		}

		[NotNull]
		protected LinkedBinaryNode<T> RotateRight([NotNull] LinkedBinaryNode<T> node /* y */)
		{
			if (node.Left == null) return node;
			LinkedBinaryNode<T> newRoot /* x */ = node.Left;

			// rotate
			node.Left = newRoot.Right /* T2 */;
			newRoot.Right = node;

			// update
			SetHeight(node);
			SetHeight(newRoot);
			return newRoot;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected void SetHeight([NotNull] LinkedBinaryNode<T> node)
		{
			node.Height = 1 + Math.Max(node.Left?.Height ?? -1, node.Right?.Height ?? -1);
		}
	}

	public static class LinkedBinaryTreeExtension
	{
		public static void WriteTo<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, [NotNull] TextWriter writer)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			if (thisValue.Root == null) return;

			StringBuilder indent = new StringBuilder();
			Stack<(TNode Node, int Level)> stack = new Stack<(TNode Node, int Level)>(1);
			stack.Push((thisValue.Root, 0));

			while (stack.Count > 0)
			{
				(TNode node, int level) = stack.Pop();
				int n = Constants.INDENT * level;

				if (indent.Length > n) indent.Length = n;
				else if (indent.Length < n) indent.Append(' ', n - indent.Length);

				writer.Write(indent);

				if (node == null)
				{
					writer.WriteLine(Constants.NULL);
					continue;
				}

				writer.WriteLine(node.ToString(level));
				if (node.IsLeaf) continue;
				stack.Push((node.Right, level + 1));
				stack.Push((node.Left, level + 1));
			}
		}

		[NotNull]
		public static IReadOnlyList<T> BranchSums<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue)
			where TNode : LinkedBinaryNode<TNode, T>
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// AlgoExpert - Become An Expert In Algorithms
			/*
			 * Write a function that takes in a Binary Tree and returns a list of its branch sums
			 * (ordered from leftmost branch sum to rightmost branch sum). A branch sum is the sum
			 * of all values in a Binary Tree branch. A Binary Tree branch is a path of nodes in a
			 * tree that starts at the root node and ends at any leaf node. Each Binary Tree node has
			 * a value stored in a property called "value" and two children nodes stored in properties
			 * called "left" and "right," respectively. Children nodes can either be Binary Tree nodes
			 * themselves or the None (null) value.
			 */
			if (thisValue.Root == null) return Array.Empty<T>();

			T runningSum = default(T);
			List<T> sumsList = new List<T>();
			BranchSumsLocal(thisValue.Root, runningSum, sumsList);
			return sumsList;

			static void BranchSumsLocal(TNode root, T sum, List<T> sums)
			{
				// it's absolutely essential to add the new sum in a new variable
				T newSum = sum.Add(root.Value);

				if (root.IsLeaf)
				{
					sums.Add(newSum);
					return;
				}

				// pass the new sum variable
				if (root.Left != null) BranchSumsLocal(root.Left, newSum, sums);
				if (root.Right != null) BranchSumsLocal(root.Right, newSum, sums);
			}
		}

		public static T FindClosestValue<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, T value, T defaultValue = default(T))
			where TNode : LinkedBinaryNode<TNode, T>
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// AlgoExpert - Become An Expert In Algorithms
			/*
			 * Write a function that takes in a sorted array of integers as well as a target integer.
			 * The function should use the Binary Search algorithm to find if the target number is contained
			 * in the array and should return its index if it is, otherwise -1 (return defaultValue for generic types).
			 */
			if (thisValue.Root == null) return defaultValue;

			T inf = TypeHelper.MaximumOf<T>();
			T closest = inf;
			TNode current = thisValue.Root;

			while (current != null)
			{
				if (value.Subtract(closest).Abs().IsGreaterThan(value.Subtract(current.Value).Abs())) closest = current.Value;
				current = value.IsLessThan(current.Value)
							? current.Left
							: value.IsGreaterThan(current.Value)
								? current.Right
								: null;
			}

			if (closest.Equals(inf)) closest = defaultValue;
			return closest;
		}

		public static void Invert<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			// AlgoExpert - Become An Expert In Algorithms
			/*
			 * Write a function that takes in a Binary Tree and inverts it. In other words, the function
			 * should swap every left node in the tree for its corresponding (mirrored) right node. Each
			 * Binary Tree node has a value stored in a property called "value" and two children nodes stored
			 * in properties called "left" and "right," respectively. Children nodes can either be Binary Tree
			 * nodes themselves or the None (null) value.
			 */
			if (thisValue.Root == null) return;

			// LevelOrder traversal
			// Root-Left-Right (Queue)
			Queue<TNode> queue = new Queue<TNode>();
			//Start at the root
			queue.Enqueue(thisValue.Root);

			while (queue.Count > 0)
			{
				TNode current = queue.Dequeue();
				SwapChildren(current);
				if (current.Left != null) queue.Enqueue(current.Left);
				if (current.Right != null) queue.Enqueue(current.Right);
			}

			static void SwapChildren(TNode node)
			{
				TNode left = node.Left;
				node.Left = node.Right;
				node.Right = left;
			}
		}
	}
}
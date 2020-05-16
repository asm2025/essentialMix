using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using asm.Exceptions.Collections;
using asm.Extensions;
using asm.Patterns.Collections;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// Binary tree using the linked representation.
	/// </summary>
	/// <typeparam name="TNode">The node type. Must inherit from <see cref="LinkedBinaryTree{TNode, T}.NodeBase"/></typeparam>
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
	 * BFS (LevelOrder): FCIADGJBEHK => Root-Left-Right (Queue)
	 * DFS [PreOrder]:   FCABDEIGHJK => Root-Left-Right (Stack)
	 * DFS [InOrder]:    ABCDEFGHIJK => Left-Root-Right (Stack)
	 * DFS [PostOrder]:  BAEDCHGKJIF => Left-Right-Root (Stack)
	 */
	[DebuggerDisplay("Count = {Count}")]
	[ComVisible(false)]
	[Serializable]
	public abstract class LinkedBinaryTree<TNode, T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
		where TNode : LinkedBinaryTree<TNode, T>.NodeBase
	{
		[DebuggerDisplay("{Value}")]
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public abstract class NodeBase
		{
			private const int LEFT = 0;
			private const int RIGHT = 1;
			private const int PARENT = 2;

			private readonly TNode[] _nodes = new TNode[3];

			internal NodeBase(T value)
			{
				Value = value;
			}
			
			public TNode Parent
			{
				get => _nodes[PARENT];
				internal set
				{
					if (_nodes[PARENT] == value) return;

					// reset old parent
					if (_nodes[PARENT] != null)
					{
						/*
						* The comparison with this and parent.left/.right is essential because the node
						* could have moved to another parent. Don't use IsLeft or IsRight here.
						*/
						if (_nodes[PARENT].Left == this) _nodes[PARENT].Left = null;
						else if (_nodes[PARENT].Right == this) _nodes[PARENT].Right = null;
					}

					_nodes[PARENT] = value;
				}
			}

			public TNode Left
			{
				get => _nodes[LEFT];
				internal set
				{
					if (_nodes[LEFT] == value) return;
					// reset old left
					if (_nodes[LEFT]?._nodes[PARENT] == this) _nodes[LEFT]._nodes[PARENT] = null;
					_nodes[LEFT] = value;
					if (_nodes[LEFT] == null) return;
					_nodes[LEFT]._nodes[PARENT] = (TNode)this;
				}
			}

			public TNode Right
			{
				get => _nodes[RIGHT];
				internal set
				{
					if (_nodes[RIGHT] == value) return;
					// reset old right
					if (_nodes[RIGHT]?._nodes[PARENT] == this) _nodes[RIGHT]._nodes[PARENT] = null;
					_nodes[RIGHT] = value;
					if (_nodes[RIGHT] == null) return;
					_nodes[RIGHT]._nodes[PARENT] = (TNode)this;
				}
			}

			public T Value { get; set; }

			public bool IsRoot => _nodes[PARENT] == null;

			public bool IsLeft => _nodes[PARENT]?.Left == this;

			public bool IsRight => _nodes[PARENT]?.Right == this;

			public bool IsLeaf => _nodes[LEFT] == null && _nodes[RIGHT] == null;

			public bool IsNode => _nodes[LEFT] != null && _nodes[RIGHT] != null;

			public bool HasOneChild => (_nodes[LEFT] != null) ^ (_nodes[RIGHT] != null);

			public bool IsFull => !HasOneChild;

			/// <inheritdoc />
			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }

			[NotNull]
			protected internal virtual string ToString(int depth, bool diagnostic)
			{
				return diagnostic
							? $"{Value} :D{depth}"
							: Convert.ToString(Value);
			}

			[ItemNotNull]
			public IEnumerable<TNode> Ancestors()
			{
				TNode node = _nodes[PARENT];

				while (node != null)
				{
					yield return node;
					node = node._nodes[PARENT];
				}
			}

			public TNode Uncle()
			{
				// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
				return _nodes[PARENT]?._nodes[PARENT] == null
							? null // no parent or grand parent
							: _nodes[PARENT].IsLeft
								? _nodes[PARENT]._nodes[PARENT]._nodes[RIGHT] // uncle on the right
								: _nodes[PARENT]._nodes[PARENT]._nodes[LEFT];
			}

			public TNode Sibling()
			{
				// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
				return _nodes[PARENT] == null
							? null // no parent
							: IsLeft
								? _nodes[PARENT]._nodes[RIGHT] // sibling on the right
								: _nodes[PARENT]._nodes[LEFT];
			}

			[NotNull]
			public TNode Minimum()
			{
				TNode minimum = (TNode)this;

				while (minimum.Left != null) 
					minimum = minimum.Left;

				return minimum;
			}

			[NotNull]
			public TNode Maximum()
			{
				TNode maximum = (TNode)this;

				while (maximum.Right != null) 
					maximum = maximum.Right;

				return maximum;
			}

			public TNode Predecessor()
			{
				if (_nodes[LEFT] != null) return _nodes[LEFT].Maximum();

				TNode node = (TNode)this;
				TNode parent = _nodes[PARENT];

				while (parent != null && node == parent.Left)
				{
					node = parent;
					parent = parent._nodes[PARENT];
				}

				return parent;
			}

			public TNode Successor()
			{
				if (_nodes[RIGHT] != null) return _nodes[RIGHT].Minimum();

				TNode node = (TNode)this;
				TNode parent = _nodes[PARENT];

				while (parent != null && node == parent.Right)
				{
					node = parent;
					parent = parent._nodes[PARENT];
				}

				return parent;
			}

			public void Swap([NotNull] TNode other)
			{
				T tmp = other.Value;
				other.Value = Value;
				Value = tmp;
			}

			public static implicit operator T([NotNull] NodeBase node) { return node.Value; }
		}

		/// <summary>
		/// a semi recursive approach to traverse the tree
		/// </summary>
		internal sealed class Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly int _version;
			private readonly TNode _root;
			private readonly DynamicQueue<TNode> _queueOrStack;
			private readonly Func<bool> _moveNext;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] LinkedBinaryTree<TNode, T> tree, [NotNull] TNode root, TraverseMethod method, HorizontalFlow flow)
			{
				_tree = tree;
				_version = _tree._version;
				_root = root;

				switch (method)
				{
					case TraverseMethod.LevelOrder:
						_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.FIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => LevelOrderLR,
							HorizontalFlow.RightToLeft => LevelOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case TraverseMethod.PreOrder:
						_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => PreOrderLR,
							HorizontalFlow.RightToLeft => PreOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case TraverseMethod.InOrder:
						_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => InOrderLR,
							HorizontalFlow.RightToLeft => InOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case TraverseMethod.PostOrder:
						_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => PostOrderLR,
							HorizontalFlow.RightToLeft => PostOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(method), method, null);
				}
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (_current == null) throw new InvalidOperationException();
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

			public bool MoveNext() { return _moveNext(); }

			void IEnumerator.Reset()
			{
				_current = null;
				_started = _done = false;
				_queueOrStack.Clear();
			}

			/// <inheritdoc />
			public void Dispose()
			{
				_current = null;
				_queueOrStack.Clear();
			}

			private bool LevelOrderLR()
			{
				if (_version != _tree._version) throw new VersionChangedException();

				// Root-Left-Right (Queue)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queueOrStack.Enqueue(_root);
				}

				// visit the next queued node
				_current = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
				if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
				return true;
			}

			private bool LevelOrderRL()
			{
				if (_version != _tree._version) throw new VersionChangedException();

				// Root-Right-Left (Queue)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queueOrStack.Enqueue(_root);
				}

				// visit the next queued node
				_current = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
				if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
				return true;
			}

			private bool PreOrderLR()
			{
				if (_version != _tree._version) throw new VersionChangedException();

				// Root-Left-Right (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queueOrStack.Enqueue(_root);
				}

				// visit the next queued node
				_current = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
				if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
				return true;
			}

			private bool PreOrderRL()
			{
				if (_version != _tree._version) throw new VersionChangedException();

				// Root-Right-Left (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queueOrStack.Enqueue(_root);
				}

				// visit the next queued node
				_current = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
				if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
				return true;
			}

			private bool InOrderLR()
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
					// Navigate right
					_current = _current?.Right;
				}

				while (_current != null || _queueOrStack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current != null)
					{
						_queueOrStack.Enqueue(_current);
						// Navigate left
						_current = _current.Left;
					}
					else
					{
						// visit the next queued node
						_current = _queueOrStack.Dequeue();
						break; // break from the loop to visit this node
					}
				}

				_done = _current == null;
				return !_done;
			}

			private bool InOrderRL()
			{
				if (_version != _tree._version) throw new VersionChangedException();

				// Right-Root-Left (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_current = _root;
				}
				else
				{
					// Navigate left
					_current = _current?.Left;
				}

				while (_current != null || _queueOrStack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current != null)
					{
						_queueOrStack.Enqueue(_current);
						// Navigate right
						_current = _current.Right;
					}
					else
					{
						// visit the next queued node
						_current = _queueOrStack.Dequeue();
						break; // break from the loop to visit this node
					}
				}

				_done = _current == null;
				return !_done;
			}

			private bool PostOrderLR()
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

						// Navigate right
						if (_current.Right != null)
							_queueOrStack.Enqueue(_current.Right);

						_queueOrStack.Enqueue(_current);

						// Navigate left
						_current = _current.Left;
					}

					if (_version != _tree._version) throw new VersionChangedException();

					_current = _queueOrStack.Count > 0
									? _queueOrStack.Dequeue()
									: null;

					if (_current == null) continue;

					/*
					* if Current has a right child and is not processed yet,
					* then make sure right child is processed before root
					*/
					if (_current.Right != null && _queueOrStack.Count > 0 && _current.Right == _queueOrStack.Peek())
					{
						// remove right child from stack
						_queueOrStack.Dequeue();
						// push Current back to stack
						_queueOrStack.Enqueue(_current);
						// process right first
						_current = _current.Right;
						continue;
					}
		
					if (_current != null)
						break; // break from the loop to visit this node
				} while (_queueOrStack.Count > 0);

				_done = _current == null;
				return !_done;
			}

			private bool PostOrderRL()
			{
				if (_version != _tree._version) throw new VersionChangedException();

				// Right-Left-Root (Stack)
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

						// Navigate left
						if (_current.Left != null)
							_queueOrStack.Enqueue(_current.Left);

						_queueOrStack.Enqueue(_current);

						// Navigate right
						_current = _current.Right;
					}

					if (_version != _tree._version) throw new VersionChangedException();
					_current = _queueOrStack.Count > 0
									? _queueOrStack.Dequeue()
									: null;
					if (_current == null) continue;

					/*
					* if Current has a left child and is not processed yet,
					* then make sure left child is processed before root
					*/
					if (_current.Left != null && _queueOrStack.Count > 0 && _current.Left == _queueOrStack.Peek())
					{
						// remove left child from stack
						_queueOrStack.Dequeue();
						// push Current back to stack
						_queueOrStack.Enqueue(_current);
						// process left first
						_current = _current.Left;
						continue;
					}

					if (_current != null)
						break; // break from the loop to visit this node
				} while (_queueOrStack.Count > 0);

				_done = _current == null;
				return !_done;
			}
		}

		/// <summary>
		/// iterative approach to traverse the tree
		/// </summary>
		internal sealed class Iterator
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly TNode _root;
			private readonly TraverseMethod _method;

			internal Iterator([NotNull] LinkedBinaryTree<TNode, T> tree, [NotNull] TNode root, TraverseMethod method)
			{
				_tree = tree;
				_root = root;
				_method = method;
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<TNode, bool> visitCallback)
			{
				int version = _tree._version;

				switch (_method)
				{
					case TraverseMethod.LevelOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								LevelOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								LevelOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PreOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PreOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PreOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.InOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								InOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								InOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PostOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PostOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PostOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				void LevelOrderLR()
				{
					// Root-Left-Right (Queue)
					Queue<TNode> queue = new Queue<TNode>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = queue.Dequeue();
						if (!visitCallback(current)) break;

						// Queue the next nodes
						if (current.Left != null) queue.Enqueue(current.Left);
						if (current.Right != null) queue.Enqueue(current.Right);
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<TNode> queue = new Queue<TNode>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = queue.Dequeue();
						if (!visitCallback(current)) break;

						// Queue the next nodes
						if (current.Right != null) queue.Enqueue(current.Right);
						if (current.Left != null) queue.Enqueue(current.Left);
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = stack.Pop();
						if (!visitCallback(current)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.Right != null) stack.Push(current.Right);
						if (current.Left != null) stack.Push(current.Left);
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = stack.Pop();
						if (!visitCallback(current)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.Left != null) stack.Push(current.Left);
						if (current.Right != null) stack.Push(current.Right);
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Left;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current)) break;

							// Navigate right
							current = current.Right;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Right;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current)) break;

							// Navigate right
							current = current.Left;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<TNode> stack = new Stack<TNode>();
					TNode lastVisited = null;
					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Left;
							continue;
						}

						TNode peek = stack.Peek();
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
							current = peek;
							lastVisited = stack.Pop();
							if (!visitCallback(current)) break;
							current = null;
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<TNode> stack = new Stack<TNode>();
					TNode lastVisited = null;
					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Right;
							continue;
						}

						TNode peek = stack.Peek();
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
							current = peek;
							lastVisited = stack.Pop();
							if (!visitCallback(current)) break;
							current = null;
						}
					}
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<TNode, int, bool> visitCallback)
			{
				int version = _tree._version;

				switch (_method)
				{
					case TraverseMethod.LevelOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								LevelOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								LevelOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PreOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PreOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PreOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.InOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								InOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								InOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PostOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PostOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PostOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				void LevelOrderLR()
				{
					// Root-Left-Right (Queue)
					Queue<(int Depth, TNode TNode)> queue = new Queue<(int Depth, TNode TNode)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = queue.Dequeue();
						if (!visitCallback(node, depth)) break;

						// Queue the next nodes
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, TNode TNode)> queue = new Queue<(int Depth, TNode TNode)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = queue.Dequeue();
						if (!visitCallback(node, depth)) break;

						// Queue the next nodes
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = stack.Pop();
						if (!visitCallback(node, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Right != null) stack.Push((depth + 1, node.Right));
						if (node.Left != null) stack.Push((depth + 1, node.Left));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = stack.Pop();
						if (!visitCallback(node, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Left != null) stack.Push((depth + 1, node.Left));
						if (node.Right != null) stack.Push((depth + 1, node.Right));
					}		
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.TNode.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.TNode, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.TNode.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.TNode.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.TNode, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.TNode.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.TNode.Left);
							continue;
						}

						(int Depth, TNode TNode) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right TNode
						 * if yes, then navigate right.
						 */
						if (peek.TNode.Right != null && lastVisited != peek.TNode.Right)
						{
							// Navigate right
							current = (peek.Depth + 1, peek.TNode.Right);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().TNode;
							if (!visitCallback(current.TNode, current.Depth)) break;
							current = (-1, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.TNode.Right);
							continue;
						}

						(int Depth, TNode TNode) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left TNode
						 * if yes, then navigate left.
						 */
						if (peek.TNode.Left != null && lastVisited != peek.TNode.Left)
						{
							// Navigate left
							current = (peek.Depth + 1, peek.TNode.Left);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().TNode;
							if (!visitCallback(current.TNode, current.Depth)) break;
							current = (-1, null);
						}
					}
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<TNode> visitCallback)
			{
				int version = _tree._version;

				switch (_method)
				{
					case TraverseMethod.LevelOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								LevelOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								LevelOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PreOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PreOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PreOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.InOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								InOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								InOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PostOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PostOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PostOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				void LevelOrderLR()
				{
					// Root-Left-Right (Queue)
					Queue<TNode> queue = new Queue<TNode>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = queue.Dequeue();
						visitCallback(current);

						// Queue the next nodes
						if (current.Left != null) queue.Enqueue(current.Left);
						if (current.Right != null) queue.Enqueue(current.Right);
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<TNode> queue = new Queue<TNode>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = queue.Dequeue();
						visitCallback(current);

						// Queue the next nodes
						if (current.Right != null) queue.Enqueue(current.Right);
						if (current.Left != null) queue.Enqueue(current.Left);
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = stack.Pop();
						visitCallback(current);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.Right != null) stack.Push(current.Right);
						if (current.Left != null) stack.Push(current.Left);
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						TNode current = stack.Pop();
						visitCallback(current);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.Left != null) stack.Push(current.Left);
						if (current.Right != null) stack.Push(current.Right);
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Left;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current);

							// Navigate right
							current = current.Right;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<TNode> stack = new Stack<TNode>();

					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Right;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current);

							// Navigate right
							current = current.Left;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<TNode> stack = new Stack<TNode>();
					TNode lastVisited = null;
					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Left;
							continue;
						}

						TNode peek = stack.Peek();
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
							current = peek;
							lastVisited = stack.Pop();
							visitCallback(current);
							current = null;
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<TNode> stack = new Stack<TNode>();
					TNode lastVisited = null;
					// Start at the root
					TNode current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Right;
							continue;
						}

						TNode peek = stack.Peek();
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
							current = peek;
							lastVisited = stack.Pop();
							visitCallback(current);
							current = null;
						}
					}
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<TNode, int> visitCallback)
			{
				int version = _tree._version;

				switch (_method)
				{
					case TraverseMethod.LevelOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								LevelOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								LevelOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PreOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PreOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PreOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.InOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								InOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								InOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					case TraverseMethod.PostOrder:
						switch (flow)
						{
							case HorizontalFlow.LeftToRight:
								PostOrderLR();
								break;
							case HorizontalFlow.RightToLeft:
								PostOrderRL();
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				void LevelOrderLR()
				{
					// Root-Left-Right (Queue)
					Queue<(int Depth, TNode TNode)> queue = new Queue<(int Depth, TNode TNode)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = queue.Dequeue();
						visitCallback(node, depth);

						// Queue the next nodes
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, TNode TNode)> queue = new Queue<(int Depth, TNode TNode)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = queue.Dequeue();
						visitCallback(node, depth);

						// Queue the next nodes
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = stack.Pop();
						visitCallback(node, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Right != null) stack.Push((depth + 1, node.Right));
						if (node.Left != null) stack.Push((depth + 1, node.Left));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode node) = stack.Pop();
						visitCallback(node, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Left != null) stack.Push((depth + 1, node.Left));
						if (node.Right != null) stack.Push((depth + 1, node.Right));
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.TNode.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.TNode, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.TNode.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();

					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.TNode.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.TNode, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.TNode.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.TNode.Left);
							continue;
						}

						(int Depth, TNode TNode) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right TNode
						 * if yes, then navigate right.
						 */
						if (peek.TNode.Right != null && lastVisited != peek.TNode.Right)
						{
							// Navigate right
							current = (peek.Depth + 1, peek.TNode.Right);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().TNode;
							visitCallback(current.TNode, current.Depth);
							current = (-1, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode TNode)> stack = new Stack<(int Depth, TNode TNode)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode TNode) current = (0, _root);

					while (current.TNode != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.TNode != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.TNode.Right);
							continue;
						}

						(int Depth, TNode TNode) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left TNode
						 * if yes, then navigate left.
						 */
						if (peek.TNode.Left != null && lastVisited != peek.TNode.Left)
						{
							// Navigate left
							current = (peek.Depth + 1, peek.TNode.Left);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().TNode;
							visitCallback(current.TNode, current.Depth);
							current = (-1, null);
						}
					}
				}
			}
		}

		/// <summary>
		/// iterative approach with level awareness. This is a different way than <see cref="TraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		internal sealed class LevelIterator
		{
			private readonly LinkedBinaryTree<TNode, T> _tree;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue = new Queue<TNode>();

			private int _level = -1;

			internal LevelIterator([NotNull] LinkedBinaryTree<TNode, T> tree, [NotNull] TNode root)
			{
				_tree = tree;
				_root = root;
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
			{
				int version = _tree._version;

				switch (flow)
				{
					case HorizontalFlow.LeftToRight:
						IterateLR();
						break;
					case HorizontalFlow.RightToLeft:
						IterateRL();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
				}

				void IterateLR()
				{
					// Root-Left-Right (Queue)
					_queue.Clear();

					// Start at the root
					_queue.Enqueue(_root);
					_level = 0;

					while (_queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();
						if (!levelCallback(_level, _queue)) break;

						int count = _queue.Count;
						_level++;

						for (int i = 0; i < count; i++)
						{
							// visit the next queued node
							TNode current = _queue.Dequeue();

							// Queue the next nodes
							if (current.Left != null) _queue.Enqueue(current.Left);
							if (current.Right != null) _queue.Enqueue(current.Right);
						}
					}

					_level = -1;
				}

				void IterateRL()
				{
					// Root-Right-Left (Queue)
					_queue.Clear();

					// Start at the root
					_queue.Enqueue(_root);
					_level = 0;

					while (_queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();
						if (!levelCallback(_level, _queue)) break;

						int count = _queue.Count;
						_level++;

						for (int i = 0; i < count; i++)
						{
							// visit the next queued node
							TNode current = _queue.Dequeue();

							// Queue the next nodes
							if (current.Right != null) _queue.Enqueue(current.Right);
							if (current.Left != null) _queue.Enqueue(current.Left);
						}
					}

					_level = -1;
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
			{
				int version = _tree._version;

				switch (flow)
				{
					case HorizontalFlow.LeftToRight:
						IterateLR();
						break;
					case HorizontalFlow.RightToLeft:
						IterateRL();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(flow), flow, null);
				}

				void IterateLR()
				{
					// Root-Left-Right (Queue)
					_queue.Clear();

					// Start at the root
					_queue.Enqueue(_root);
					_level = 0;

					while (_queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();
						levelCallback(_level, _queue);

						int count = _queue.Count;
						_level++;

						for (int i = 0; i < count; i++)
						{
							// visit the next queued node
							TNode current = _queue.Dequeue();

							// Queue the next nodes
							if (current.Left != null) _queue.Enqueue(current.Left);
							if (current.Right != null) _queue.Enqueue(current.Right);
						}
					}

					_level = -1;
				}

				void IterateRL()
				{
					// Root-Right-Left (Queue)
					_queue.Clear();

					// Start at the root
					_queue.Enqueue(_root);
					_level = 0;

					while (_queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();
						levelCallback(_level, _queue);

						int count = _queue.Count;
						_level++;

						for (int i = 0; i < count; i++)
						{
							// visit the next queued node
							TNode current = _queue.Dequeue();

							// Queue the next nodes
							if (current.Right != null) _queue.Enqueue(current.Right);
							if (current.Left != null) _queue.Enqueue(current.Left);
						}
					}

					_level = -1;
				}
			}
		}

		private SerializationInfo siInfo; //A temporary variable which we need during deserialization.        
		private object _syncRoot;
		protected internal int _version;

		/// <inheritdoc />
		protected LinkedBinaryTree()
			: this(Comparer<T>.Default)
		{
		}

		protected LinkedBinaryTree(IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;
		}

		protected LinkedBinaryTree(SerializationInfo info, StreamingContext context)
		{
			siInfo = info;
			Comparer = Comparer<T>.Default;
		}

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
		bool ICollection<T>.IsReadOnly => false;

		[NotNull]
		public IComparer<T> Comparer { get; private set; }

		public abstract bool AutoBalance { get; }

		public TNode Root { get; protected internal set; }

		public int Count { get; protected internal set; }

		public bool IsFull => Root == null || Root.IsFull;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Root?.ToString() ?? string.Empty; }

		public void OnDeserialization(object sender)
		{
			if (siInfo == null) return; //Somebody had a dependency on this Dictionary and fixed us up before the ObjectManager got to it.
			Comparer = (IComparer<T>)siInfo.GetValue(nameof(Comparer), typeof(IComparer<T>));
			Root = (TNode)siInfo.GetValue(nameof(Root), typeof(TNode));
			Count = siInfo.GetInt32(nameof(Count));
			siInfo = null;
		}

		[SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Customized serialization for LinkedList.
			// We need to do this because it will be too expensive to Serialize each node.
			// This will give us the flexibility to change internal implementation freely in future.
			if (info == null) throw new ArgumentNullException(nameof(info));
			siInfo.AddValue(nameof(Comparer), Comparer, typeof(IComparer<T>));
			siInfo.AddValue(nameof(Root), Root, typeof(TNode));
			siInfo.AddValue(nameof(Count), Count);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

		[NotNull]
		public IEnumerator<T> GetEnumerator()
		{
			return (IEnumerator<T>)Enumerate(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight);
		}

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerable<T> Enumerate(TNode root, TraverseMethod method, HorizontalFlow flow)
		{
			return root == null
						? Enumerable.Empty<T>()
						: new Enumerator(this, root, method, flow);
		}

		#region Enumerate overloads
		[NotNull]
		public IEnumerable<T> Enumerate(TNode root)
		{
			return Enumerate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(TNode root, HorizontalFlow flow)
		{
			return Enumerate(root, TraverseMethod.InOrder, flow);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(TNode root, TraverseMethod method)
		{
			return Enumerate(root, method, HorizontalFlow.LeftToRight);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<TNode, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback function
		public void Iterate(TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(TNode root, TraverseMethod method, [NotNull] Func<TNode, bool> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node with depth awareness that can cancel the loop</param>
		public void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<TNode, int, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback with depth function
		public void Iterate(TNode root, [NotNull] Func<TNode, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Func<TNode, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(TNode root, TraverseMethod method, [NotNull] Func<TNode, int, bool> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, [NotNull] Action<TNode> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback action
		public void Iterate(TNode root, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(TNode root, TraverseMethod method, [NotNull] Action<TNode> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node with depth awareness</param>
		public void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, Action<TNode, int> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback with depth action
		public void Iterate(TNode root, [NotNull] Action<TNode, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Action<TNode, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(TNode root, TraverseMethod method, [NotNull] Action<TNode, int> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback function to handle the nodes of the level and can cancel the loop.</param>
		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
		{
			if (root == null) return;
			new LevelIterator(this, root).Iterate(flow, levelCallback);
		}

		#region LevelIterate overloads - visitCallback function
		public void Iterate(TNode root, [NotNull] Func<int, IReadOnlyCollection<TNode>, bool> levelCallback)
		{
			Iterate(root, HorizontalFlow.LeftToRight, levelCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback action to handle the nodes of the level.</param>
		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
		{
			if (root == null) return;
			new LevelIterator(this, root).Iterate(flow, levelCallback);
		}

		#region LevelIterate overloads - visitCallback function
		public void Iterate(TNode root, [NotNull] Action<int, IReadOnlyCollection<TNode>> levelCallback)
		{
			Iterate(root, HorizontalFlow.LeftToRight, levelCallback);
		}
		#endregion

		/// <inheritdoc />
		public bool Contains(T value) { return Find(value) != null; }

		/// <summary>
		/// Finds the node with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node or null if no match is found</returns>
		public TNode Find(T value)
		{
			TNode current = FindNearestLeaf(value);
			if (current == null || Comparer.IsEqual(current.Value, value)) return current;
			if (current.Left != null && Comparer.IsEqual(current.Left.Value, value)) return current.Left;
			if (current.Right != null && Comparer.IsEqual(current.Right.Value, value)) return current.Right;
			return null;
		}

		/// <summary>
		/// Finds the node with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <param name="method">The <see cref="TraverseMethod"/> to use in the search.</param>
		/// <returns>The found node or null if no match is found</returns>
		public TNode Find(T value, TraverseMethod method)
		{
			TNode node = null;
			Iterate(Root, method, HorizontalFlow.LeftToRight, e =>
			{
				if (Comparer.Compare(e.Value, value) != 0) return true;
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
			return GeNodesAtLevel(level)?.FirstOrDefault(e => Comparer.IsEqual(e.Value, value));
		}

		/// <summary>
		/// Finds the closest parent node relative to the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node or null if no match is found</returns>
		public abstract TNode FindNearestLeaf(T value);

		/// <inheritdoc />
		public abstract void Add(T value);

		/// <inheritdoc />
		public bool Remove(T value)
		{
			TNode node = Find(value);
			return node != null && Remove(node);
		}

		public abstract bool Remove([NotNull] TNode node);

		/// <inheritdoc />
		public void Clear()
		{
			Root = null;
			Count = 0;
		}

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

		public bool IsBalanced() { return IsBalanced(Root); }
		public abstract bool IsBalanced(TNode node);

		/// <summary>
		/// Balances the tree if it supports it.
		/// </summary>
		public abstract void Balance();

		public IReadOnlyList<TNode> GeNodesAtLevel(int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Root == null) return Array.Empty<TNode>();

			IReadOnlyList<TNode> list = null;
			Iterate(Root, HorizontalFlow.LeftToRight, (lvl, nodes) =>
			{
				if (lvl < level) return true;
				if (lvl == level) list = nodes.ToArray();
				return false;
			});
			return list ?? throw new ArgumentOutOfRangeException(nameof(level));
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			array.Length.ValidateRange(arrayIndex, Count);
			int lo = arrayIndex, hi = lo + Count;
			Iterate(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, node =>
			{
				array[lo++] = node.Value;
				return lo < hi;
			});
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));

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
			Type sourceType = typeof(TNode);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;
			int lo = index, hi = lo + Count;
			Iterate(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, node =>
			{
				objects[lo++] = node;
				return lo < hi;
			});
		}

		[NotNull]
		public T[] ToArray(TraverseMethod method = TraverseMethod.InOrder, HorizontalFlow flow = HorizontalFlow.LeftToRight)
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
					Iterate(Root, method, flow, node =>
					{
						array[index++] = node.Value;
						return index < Count;
					});
					return array;
			}
		}
	}

	/// <inheritdoc />
	[Serializable]
	public abstract class LinkedBinaryTree<T> : LinkedBinaryTree<LinkedBinaryTree<T>.Node, T>
	{
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public sealed class Node : NodeBase
		{
			internal Node(T value)
				: base(value)
			{
			}

			/// <inheritdoc />
			protected internal override string ToString(int depth, bool diagnostic)
			{
				return diagnostic
						   ? $"{Value} :D{depth}H{Height}B{BalanceFactor}"
						   : Convert.ToString(Value);
			}

			public int Height { get; internal set; }

			public int BalanceFactor => (Left?.Height ?? -1) - (Right?.Height ?? -1);
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
		protected LinkedBinaryTree(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public int GetHeight() { return Root?.Height ?? 0; }

		/*
		* https://www.geeksforgeeks.org/avl-tree-set-1-insertion/
		*     y                               x
		*    / \     Right Rotation(y) ->    /  \
		*   x   T3                          T1   y 
		*  / \                                  / \
		* T1  T2     <- Left Rotation(x)       T2  T3
		*
		* A reference to the drawing only, not the code
		*/
		[NotNull]
		protected Node RotateLeft([NotNull] Node node /* x */)
		{
			bool isLeft = node.IsLeft;
			Node oldParent = node.Parent;
			Node newRoot /* y */ = node.Right;
			Node oldLeft /* T2 */ = newRoot.Left;

			// Perform rotation
			newRoot.Left = node;
			node.Right = oldLeft;

			// update nodes
			SetHeight(node);
			SetHeight(newRoot);

			// connect the new root with the old parent
			if (oldParent != null)
			{
				if (isLeft) oldParent.Left = newRoot;
				else oldParent.Right = newRoot;
			}
			else
			{
				Root = newRoot;
			}

			_version++;
			// Return new root
			return newRoot;
		}

		[NotNull]
		protected Node RotateRight([NotNull] Node node /* y */)
		{
			bool isLeft = node.IsLeft;
			Node oldParent = node.Parent;
			Node newRoot /* x */ = node.Left;
			Node oldRight /* T2 */ = newRoot.Right;

			// Perform rotation
			newRoot.Right = node;
			node.Left = oldRight;

			// update nodes
			SetHeight(node);
			SetHeight(newRoot);

			// connect the new root with the old parent
			if (oldParent != null)
			{
				if (isLeft) oldParent.Left = newRoot;
				else oldParent.Right = newRoot;
			}
			else
			{
				Root = newRoot;
			}

			_version++;
			// Return new root
			return newRoot;
		}

		protected internal static void SetHeight([NotNull] Node node)
		{
			node.Height = 1 + Math.Max(node.Left?.Height ?? -1, node.Right?.Height ?? -1);
		}
	}

	public static class LinkedBinaryTreeExtension
	{
		public static string ToString<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, Orientation orientation, bool diagnosticInfo = false)
			where TNode : LinkedBinaryTree<TNode, T>.NodeBase
		{
			return ToString(thisValue, thisValue.Root, orientation, diagnosticInfo);
		}
		public static string ToString<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, TNode node, Orientation orientation, bool diagnosticInfo = false)
			where TNode : LinkedBinaryTree<TNode, T>.NodeBase
		{
			if (node == null) return string.Empty;
			return orientation switch
			{
				Orientation.Horizontal => Horizontally(thisValue, node, diagnosticInfo),
				Orientation.Vertical => Vertically(thisValue, node, diagnosticInfo),
				_ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
			};

			static string Horizontally(LinkedBinaryTree<TNode, T> tree, TNode node, bool diagnostic)
			{
				const string STR_BLANK = "    ";
				const string STR_EXT = "│   ";
				const string STR_CONNECTOR = "─── ";
				const string STR_CONNECTOR_L = "└── ";
				const string STR_CONNECTOR_R = "┌── ";

				StringBuilder sb = new StringBuilder();
				Stack<string> connectors = new Stack<string>();

				tree.Iterate(node, TraverseMethod.InOrder, HorizontalFlow.RightToLeft, (e, depth) =>
				{
					connectors.Push(e.ToString(depth, diagnostic));

					if (e.IsRight) connectors.Push(STR_CONNECTOR_R);
					else if (e.IsLeft) connectors.Push(STR_CONNECTOR_L);
					else connectors.Push(STR_CONNECTOR);

					while (e.Parent != null)
					{
						if (e.IsLeft && e.Parent.IsRight || e.IsRight && e.Parent.IsLeft) connectors.Push(STR_EXT);
						else connectors.Push(STR_BLANK);

						e = e.Parent;
					}

					while (connectors.Count > 1) 
						sb.Append(connectors.Pop());

					sb.AppendLine(connectors.Pop());
				});

				return sb.ToString();
			}

			static string Vertically(LinkedBinaryTree<TNode, T> tree, TNode node, bool diagnostic)
			{
				const char C_BLANK = ' ';
				const char C_EXT = '─';
				const char C_CONNECTOR_L = '┌';
				const char C_CONNECTOR_R = '┐';

				int distance = 0;
				IDictionary<int, StringBuilder> lines = new Dictionary<int, StringBuilder>();
				tree.Iterate(node, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, (e, depth) =>
				{
					StringBuilder line = lines.GetOrAdd(depth);

					if (line.Length > 0 && line[line.Length - 1] == C_CONNECTOR_L) line.Append(C_EXT, distance - line.Length);
					else line.Append(C_BLANK, distance - line.Length);

					if (depth > 0)
					{
						StringBuilder prevLine = lines.GetOrAdd(depth - 1);

						if (e.IsLeft)
						{
							prevLine.Append(C_BLANK, distance - prevLine.Length);
							if (line.Length > 0) prevLine.Append(C_BLANK);
							prevLine.Append(C_CONNECTOR_L);
						}
						else
						{
							prevLine.Append(C_BLANK);
							prevLine.Append(C_EXT, distance - prevLine.Length + 1);
							prevLine.Append(C_CONNECTOR_R);
						}
					}

					if (line.Length > 0) line.Append(C_BLANK);
					line.Append(e.ToString(depth, diagnostic));
					distance = line.Length;
				});

				return string.Join(Environment.NewLine, lines.OrderBy(e => e.Key).Select(e => e.Value));
			}
		}
	}
}
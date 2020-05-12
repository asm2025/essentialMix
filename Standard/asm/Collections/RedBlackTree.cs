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
using asm.Patterns.Direction;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// Red-Black tree implementation using the linked representation.
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
	 * BFS (LevelOrder): FCIADGJBEHK => Root-Left-Right (Queue)
	 * DFS [PreOrder]:   FCABDEIGHJK => Root-Left-Right (Stack)
	 * DFS [InOrder]:    ABCDEFGHIJK => Left-Root-Right (Stack)
	 * DFS [PostOrder]:  BAEDCHGKJIF => Left-Right-Root (Stack)
	 */
	[DebuggerDisplay("Count = {Count}")]
	[ComVisible(false)]
	[Serializable]
	public sealed class RedBlackTree<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
	{
		[DebuggerDisplay("{Value}")]
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public sealed class Node
		{
			private const int LEFT = 0;
			private const int RIGHT = 1;
			private const int PARENT = 2;

			private readonly Node[] _nodes = new Node[3];

			internal Node(T value)
			{
				Value = value;
			}

			public Node Parent
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
						// access the fields directly to avoid StackOverflowException
						if (_nodes[PARENT]._nodes[LEFT] == this)
						{
							_nodes[PARENT]._nodes[LEFT] = null;
						}
						else if (_nodes[PARENT]._nodes[RIGHT] == this)
						{
							_nodes[PARENT]._nodes[RIGHT] = null;
						}
					}

					_nodes[PARENT] = value;
				}
			}

			public Node Left
			{
				get => _nodes[LEFT];
				internal set
				{
					if (_nodes[LEFT] == value) return;
					
					// reset old left, access the fields directly to avoid StackOverflowException
					if (_nodes[LEFT] != null && _nodes[LEFT]._nodes[PARENT] == this)
					{
						_nodes[LEFT]._nodes[PARENT] = null;
					}

					_nodes[LEFT] = value;
					if (_nodes[LEFT] != null) _nodes[LEFT]._nodes[PARENT] = this;
				}
			}

			public Node Right
			{
				get => _nodes[RIGHT];
				internal set
				{
					if (_nodes[RIGHT] == value) return;
					
					// reset old right, access the fields directly to avoid StackOverflowException
					if (_nodes[RIGHT] != null && _nodes[RIGHT]._nodes[PARENT] == this)
					{
						_nodes[RIGHT]._nodes[PARENT] = null;
					}

					_nodes[RIGHT] = value;
					if (_nodes[RIGHT] != null) _nodes[RIGHT]._nodes[PARENT] = this;
				}
			}

			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			public Node Uncle =>
				_nodes[PARENT]?._nodes[PARENT] == null
					? null // no parent or grand parent
					: _nodes[PARENT].IsLeft
						? _nodes[PARENT]._nodes[PARENT]._nodes[RIGHT] // uncle on the right
						: _nodes[PARENT]._nodes[PARENT]._nodes[LEFT];

			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			public Node Sibling =>
				_nodes[PARENT] == null
					? null // no parent
					: IsLeft
						? _nodes[PARENT]._nodes[RIGHT] // sibling on the right
						: _nodes[PARENT]._nodes[LEFT];

			public T Value { get; set; }

			/// <summary>
			/// True means Red and False = no color or Black
			/// </summary>
			public bool Color { get; internal set; } = true;

			public int Depth { get; internal set; }

			public bool IsRoot => _nodes[PARENT] == null;

			public bool IsLeft => _nodes[PARENT]?._nodes[LEFT] == this;

			public bool IsRight => _nodes[PARENT]?._nodes[RIGHT] == this;

			public bool IsLeaf => _nodes[LEFT] == null && _nodes[RIGHT] == null;

			public bool IsNode => _nodes[LEFT] != null && _nodes[RIGHT] != null;

			public bool HasOneChild => (_nodes[LEFT] != null) ^ (_nodes[RIGHT] != null);

			public bool IsFull => !HasOneChild;

			/// <inheritdoc />
			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }

			[ItemNotNull]
			public IEnumerable<Node> Ancestors()
			{
				Node node = Parent;

				while (node != null)
				{
					yield return node;
					node = node.Parent;
				}
			}

			public static implicit operator T([NotNull] Node node) { return node.Value; }
		}

		/// <summary>
		/// a semi recursive approach to traverse the tree
		/// </summary>
		internal sealed class Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		{
			private readonly RedBlackTree<T> _tree;
			private readonly int _version;
			private readonly Node _root;
			private readonly DynamicQueue<Node> _queueOrStack;
			private readonly bool _left;
			private readonly bool _right;
			private readonly Func<bool> _moveNext;

			private Node _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] RedBlackTree<T> tree, [NotNull] Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction)
			{
				_tree = tree;
				_version = _tree._version;
				_root = root;
				(_left, _right) = direction.GetDirections();

				switch (method)
				{
					case TraverseMethod.LevelOrder:
						_queueOrStack = new DynamicQueue<Node>(DequeuePriority.FIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => LevelOrderLR,
							HorizontalFlow.RightToLeft => LevelOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case TraverseMethod.PreOrder:
						_queueOrStack = new DynamicQueue<Node>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => PreOrderLR,
							HorizontalFlow.RightToLeft => PreOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case TraverseMethod.InOrder:
						_queueOrStack = new DynamicQueue<Node>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => InOrderLR,
							HorizontalFlow.RightToLeft => InOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case TraverseMethod.PostOrder:
						_queueOrStack = new DynamicQueue<Node>(DequeuePriority.LIFO);
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
			public void Dispose() { }

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
				if ((!_current.IsRoot || _left) && _current.Left != null) _queueOrStack.Enqueue(_current.Left);
				if ((!_current.IsRoot || _right) && _current.Right != null) _queueOrStack.Enqueue(_current.Right);
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
				if ((!_current.IsRoot || _right) && _current.Right != null) _queueOrStack.Enqueue(_current.Right);
				if ((!_current.IsRoot || _left) && _current.Left != null) _queueOrStack.Enqueue(_current.Left);
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
				if ((!_current.IsRoot || _right) && _current.Right != null) _queueOrStack.Enqueue(_current.Right);
				if ((!_current.IsRoot || _left) && _current.Left != null) _queueOrStack.Enqueue(_current.Left);
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
				if ((!_current.IsRoot || _left) && _current.Left != null) _queueOrStack.Enqueue(_current.Left);
				if ((!_current.IsRoot || _right) && _current.Right != null) _queueOrStack.Enqueue(_current.Right);
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
				else if (_current != null)
				{
					// Navigate right
					_current = _current.IsRoot
									? _right
										? _current.Right
										: null
									: _current.Right;
				}

				while (_current != null || _queueOrStack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current != null)
					{
						_queueOrStack.Enqueue(_current);
						// Navigate left
						_current = _current.IsRoot
										? _left
											? _current.Left
											: null
										: _current.Left;
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
				else if (_current != null)
				{
					// Navigate left
					_current = _current.IsRoot
									? _left
										? _current.Left
										: null
									: _current.Left;
				}

				while (_current != null || _queueOrStack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current != null)
					{
						_queueOrStack.Enqueue(_current);
						// Navigate right
						_current = _current.IsRoot
										? _right
											? _current.Right
											: null
										: _current.Right;
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
						if (_current.Right != null && (!_current.IsRoot || _right))
							_queueOrStack.Enqueue(_current.Right);

						_queueOrStack.Enqueue(_current);

						// Navigate left
						_current = !_current.IsRoot || _left
										? _current.Left
										: null;
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
						if (_current.Left != null && (!_current.IsRoot || _left))
							_queueOrStack.Enqueue(_current.Left);

						_queueOrStack.Enqueue(_current);

						// Navigate right
						_current = !_current.IsRoot || _right
										? _current.Right
										: null;
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
			private readonly RedBlackTree<T> _tree;
			private readonly Node _root;
			private readonly TraverseMethod _method;

			internal Iterator([NotNull] RedBlackTree<T> tree, [NotNull] Node root, TraverseMethod method)
			{
				_tree = tree;
				_root = root;
				_method = method;
			}

			public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<Node, bool> visitCallback)
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
					(bool left, bool right) = direction.GetDirections();
					Queue<Node> queue = new Queue<Node>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = queue.Dequeue();
						if (!visitCallback(current)) break;

						// Queue the next nodes
						if ((!current.IsRoot || left) && current.Left != null) queue.Enqueue(current.Left);
						if ((!current.IsRoot || right) && current.Right != null) queue.Enqueue(current.Right);
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					(bool left, bool right) = direction.GetDirections();
					Queue<Node> queue = new Queue<Node>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = queue.Dequeue();
						if (!visitCallback(current)) break;

						// Queue the next nodes
						if ((!current.IsRoot || right) && current.Right != null) queue.Enqueue(current.Right);
						if ((!current.IsRoot || left) && current.Left != null) queue.Enqueue(current.Left);
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = stack.Pop();
						if (!visitCallback(current)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!current.IsRoot || right) && current.Right != null) stack.Push(current.Right);
						if ((!current.IsRoot || left) && current.Left != null) stack.Push(current.Left);
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = stack.Pop();
						if (!visitCallback(current)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!current.IsRoot || left) && current.Left != null) stack.Push(current.Left);
						if ((!current.IsRoot || right) && current.Right != null) stack.Push(current.Right);
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.IsRoot
										? left
											? current.Left
											: null
										: current.Left;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current)) break;

							// Navigate right
							current = current.IsRoot
										? right
											? current.Right
											: null
										: current.Right;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.IsRoot
										? right
											? current.Right
											: null
										: current.Right;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current)) break;

							// Navigate right
							current = current.IsRoot
										? left
											? current.Left
											: null
										: current.Left;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<Node> stack = new Stack<Node>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.IsRoot
										? left
											? current.Left
											: null
										: current.Left;
							continue;
						}

						Node peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right node?
						 * if yes, then navigate right.
						 */
						if (peek.Right != null && lastVisited != peek.Right)
						{
							// Navigate right
							current = peek.IsRoot
										? right
											? peek.Right
											: null
										: peek.Right;
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
					Stack<Node> stack = new Stack<Node>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.IsRoot
										? right
											? current.Right
											: null
										: current.Right;
							continue;
						}

						Node peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left node?
						 * if yes, then navigate left.
						 */
						if (peek.Left != null && lastVisited != peek.Left)
						{
							// Navigate left
							current = peek.IsRoot
										? left
											? peek.Left
											: null
										: peek.Left;
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

			public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<Node> visitCallback)
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
					(bool left, bool right) = direction.GetDirections();
					Queue<Node> queue = new Queue<Node>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = queue.Dequeue();
						visitCallback(current);

						// Queue the next nodes
						if ((!current.IsRoot || left) && current.Left != null) queue.Enqueue(current.Left);
						if ((!current.IsRoot || right) && current.Right != null) queue.Enqueue(current.Right);
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					(bool left, bool right) = direction.GetDirections();
					Queue<Node> queue = new Queue<Node>();

					// Start at the root
					queue.Enqueue(_root);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = queue.Dequeue();
						visitCallback(current);

						// Queue the next nodes
						if ((!current.IsRoot || right) && current.Right != null) queue.Enqueue(current.Right);
						if ((!current.IsRoot || left) && current.Left != null) queue.Enqueue(current.Left);
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = stack.Pop();
						visitCallback(current);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!current.IsRoot || right) && current.Right != null) stack.Push(current.Right);
						if ((!current.IsRoot || left) && current.Left != null) stack.Push(current.Left);
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					stack.Push(_root);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						Node current = stack.Pop();
						visitCallback(current);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!current.IsRoot || left) && current.Left != null) stack.Push(current.Left);
						if ((!current.IsRoot || right) && current.Right != null) stack.Push(current.Right);
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.IsRoot
										? left
											? current.Left
											: null
										: current.Left;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current);

							// Navigate right
							current = current.IsRoot
										? right
											? current.Right
											: null
										: current.Right;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<Node> stack = new Stack<Node>();

					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.IsRoot
										? right
											? current.Right
											: null
										: current.Right;
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current);

							// Navigate right
							current = current.IsRoot
										? left
											? current.Left
											: null
										: current.Left;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<Node> stack = new Stack<Node>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.IsRoot
										? left
											? current.Left
											: null
										: current.Left;
							continue;
						}

						Node peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right node?
						 * if yes, then navigate right.
						 */
						if (peek.Right != null && lastVisited != peek.Right)
						{
							// Navigate right
							current = peek.IsRoot
										? right
											? peek.Right
											: null
										: peek.Right;
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
					Stack<Node> stack = new Stack<Node>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					Node current = _root;

					while (current != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.IsRoot
										? right
											? current.Right
											: null
										: current.Right;
							continue;
						}

						Node peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left node?
						 * if yes, then navigate left.
						 */
						if (peek.Left != null && lastVisited != peek.Left)
						{
							// Navigate left
							current = peek.IsRoot
										? left
											? peek.Left
											: null
										: peek.Left;
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
		}

		/// <summary>
		/// iterative approach with level awareness. this is a different way than TraverseMethod.LevelOrder in that each level's nodes are brought all at once
		/// </summary>
		internal sealed class LevelIterator
		{
			private readonly RedBlackTree<T> _tree;
			private readonly Node _root;
			private readonly Queue<Node> _queue = new Queue<Node>();

			private int _level = -1;

			internal LevelIterator([NotNull] RedBlackTree<T> tree, [NotNull] Node root)
			{
				_tree = tree;
				_root = root;
			}

			public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
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
					(bool left, bool right) = direction.GetDirections();
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
							Node current = _queue.Dequeue();

							// Queue the next nodes
							if ((!current.IsRoot || left) && current.Left != null) _queue.Enqueue(current.Left);
							if ((!current.IsRoot || right) && current.Right != null) _queue.Enqueue(current.Right);
						}
					}

					_level = -1;
				}

				void IterateRL()
				{
					// Root-Right-Left (Queue)
					(bool left, bool right) = direction.GetDirections();
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
							Node current = _queue.Dequeue();

							// Queue the next nodes
							if ((!current.IsRoot || right) && current.Right != null) _queue.Enqueue(current.Right);
							if ((!current.IsRoot || left) && current.Left != null) _queue.Enqueue(current.Left);
						}
					}

					_level = -1;
				}
			}

			public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
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
					(bool left, bool right) = direction.GetDirections();
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
							Node current = _queue.Dequeue();

							// Queue the next nodes
							if ((!current.IsRoot || left) && current.Left != null) _queue.Enqueue(current.Left);
							if ((!current.IsRoot || right) && current.Right != null) _queue.Enqueue(current.Right);
						}
					}

					_level = -1;
				}

				void IterateRL()
				{
					// Root-Right-Left (Queue)
					(bool left, bool right) = direction.GetDirections();
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
							Node current = _queue.Dequeue();

							// Queue the next nodes
							if ((!current.IsRoot || right) && current.Right != null) _queue.Enqueue(current.Right);
							if ((!current.IsRoot || left) && current.Left != null) _queue.Enqueue(current.Left);
						}
					}

					_level = -1;
				}
			}
		}

		private SerializationInfo siInfo; //A temporary variable which we need during deserialization.        
		private object _syncRoot;
		internal int _version;

		/// <inheritdoc />
		public RedBlackTree()
			: this(Comparer<T>.Default)
		{
		}

		public RedBlackTree(IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;
		}

		/// <inheritdoc />
		public RedBlackTree(T value)
			: this(value, Comparer<T>.Default)
		{
		}

		public RedBlackTree(T value, IComparer<T> comparer)
			: this(comparer)
		{
			Add(value);
		}

		public RedBlackTree([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		public RedBlackTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: this(comparer)
		{
			Add(collection);
		}

		internal RedBlackTree(SerializationInfo info, StreamingContext context)
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

		public Node Root { get; internal set; }

		public int Count { get; internal set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Root?.ToString() ?? string.Empty; }

		public void OnDeserialization(object sender)
		{
			if (siInfo == null) return; //Somebody had a dependency on this Dictionary and fixed us up before the ObjectManager got to it.
			Comparer = (IComparer<T>)siInfo.GetValue(nameof(Comparer), typeof(IComparer<T>));
			Root = (Node)siInfo.GetValue(nameof(Root), typeof(Node));
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
			siInfo.AddValue(nameof(Root), Root, typeof(Node));
			siInfo.AddValue(nameof(Count), Count);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

		[NotNull]
		public IEnumerator<T> GetEnumerator()
		{
			return GetEnumerator(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default).GetEnumerator();
		}

		[NotNull]
		public IEnumerable<T> GetEnumerator(TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction)
		{
			return GetEnumerator(Root, method, flow, direction);
		}
		[NotNull]
		public IEnumerable<T> GetEnumerator(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction)
		{
			return root == null
						? Enumerable.Empty<T>()
						: new Enumerator(this, root, method, flow, direction);
		}

		public void Iterate(TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Func<Node, bool> visitCallback)
		{
			Iterate(Root, method, flow, direction, visitCallback);
		}
		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Func<Node, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, direction, visitCallback);
		}

		public void Iterate(TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Action<Node> visitCallback)
		{
			Iterate(Root, method, flow, direction, visitCallback);
		}
		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Action<Node> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, direction, visitCallback);
		}

		public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
		{
			Iterate(Root, flow, direction, levelCallback);
		}
		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
		{
			if (root == null) return;
			new LevelIterator(this, root).Iterate(flow, direction, levelCallback);
		}

		public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
		{
			Iterate(Root, flow, direction, levelCallback);
		}
		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
		{
			if (root == null) return;
			new LevelIterator(this, root).Iterate(flow, direction, levelCallback);
		}

		/// <inheritdoc />
		public bool Contains(T value) { return Find(value) != null; }

		/// <summary>
		/// Finds the node with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node or null if no match is found</returns>
		public Node Find(T value)
		{
			Node current = Successor(value);
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
		public Node Find(T value, TraverseMethod method)
		{
			Node node = null;
			Iterate(Root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
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
		public Node Find(T value, int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			return GeNodesAtLevel(level)?.FirstOrDefault(e => Comparer.IsEqual(e.Value, value));
		}

		public Node Successor(T value)
		{
			Node current = null, next = Root;

			while (next != null)
			{
				current = next;
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
				// handle duplicates
				if (next != null && Comparer.IsEqual(next.Value, value)) continue;
				// OK, found it if are equal
				if (Comparer.IsEqual(current.Value, value)) break;
			}

			return current;
		}

		/// <inheritdoc />
		public void Add(T value)
		{
			Node parent = Successor(value);

			if (parent == null)
			{
				// no parent means there is no root currently
				Root = new Node(value) { Color = false };
				Count++;
				_version++;
				return;
			}

			Node node = new Node(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			// update nodes
			Update(node);
			Count++;
			_version++;
			if (IsBalanced(node)) return;
			FixRedRed(node);
		}

		public void Add([NotNull] IEnumerable<T> collection)
		{
			foreach (T value in collection) 
				Add(value);
		}

		/// <inheritdoc />
		public bool Remove(T value)
		{
			Node node = Find(value);
			return node != null && Remove(node);
		}

		public bool Remove([NotNull] Node node)
		{
			// A node that replaces the deleted node
			Node child;

			// case 1: node has no right child
			if (node.Right == null)
			{
				child = node.Left;
			}
			// case 2: node has a right child which doesn't have a left child
			else if (node.Right.Left == null)
			{
				// move the left to the right child's left
				node.Right.Left = node.Left;
				child = node.Right;
			}
			// case 3: node has a right child that has a left child
			else
			{
				// find the right child's left most child
				child = node.Right.Left;

				while (child.Left != null)
				{
					child = child.Left;
				}
			}

			Node parent = node.Parent;
			bool dblBlack = (child == null || !child.Color) && !node.Color;

			if (child == null)
			{
				// node is leaf
				if (parent == null)
				{
					// node is root
					Root = null;
				}
			}

			if (child != null)
			{
				if (parent == null) child.Parent = null;
				// update nodes
				Update(child);
			}
			else if (parent != null)
			{
				// update nodes
				Update(parent);
			}

			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Root = null;
			Count = 0;
		}

		public IReadOnlyList<Node> GeNodesAtLevel(int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Root == null) return Array.Empty<Node>();

			IReadOnlyList<Node> list = null;
			Iterate(Root, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, (lvl, nodes) =>
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
			Iterate(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, node =>
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
			Type sourceType = typeof(Node);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;
			int lo = index, hi = lo + Count;
			Iterate(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, node =>
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
					Iterate(Root, method, flow, HorizontalDirectionFlags.Default, node =>
					{
						array[index++] = node.Value;
						return index < Count;
					});
					return array;
			}
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
		public bool Validate(Node node)
		{
			if (node == null || node.IsLeaf) return true;

			bool isValid = true;
			Iterate(TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
			{
				if (e.IsLeft)
				{
					if (Comparer.IsGreaterThanOrEqual(e.Value, e.Parent.Value)) isValid = false;
					if (isValid && e.Parent.IsRight && Comparer.IsLessThan(e.Value, e.Parent.Parent.Value)) isValid = false;
				}

				if (isValid && e.IsRight)
				{
					if (Comparer.IsLessThan(e.Value, e.Parent.Value)) isValid = false;
					if (isValid && e.Parent.IsLeft && Comparer.IsGreaterThanOrEqual(e.Value, e.Parent.Parent.Value)) isValid = false;
				}

				return isValid;
			});
			return isValid;
		}

		public bool IsBalanced() { return IsBalanced(Root); }
		public bool IsBalanced(Node node)
		{
			if (node == null) return true;

			// node is red => its parent must be either null or black and all its direct children must be either null or black.
			if (node.Color)
			{
				return (node.Parent == null || !node.Parent.Color)
						&& (node.Left == null || !node.Left.Color) 
						&& (node.Right == null || !node.Right.Color);
			}

			// node is black
			// left is null or black
			return (node.Left == null || !node.Left.Color
					// or left is red and all its direct children are either null or black
					|| (node.Left.Left == null || !node.Left.Left.Color) && (node.Left.Right == null || !node.Left.Right.Color))
					// and right is null or black
					&& (node.Right == null || !node.Right.Color
						// or right is red and all its direct children are either null or black
						|| (node.Right.Left == null || !node.Right.Left.Color) && (node.Right.Right == null || !node.Right.Right.Color));
		}

		/// <summary>
		/// Balances the tree if it supports it.
		/// <para><strong>RedBlackTree implementation of balancing the tree</strong></para>
		/// </summary>
		public void Balance()
		{
			if (Root == null || Root.IsLeaf) return;

			// find all unbalanced nodes
			Queue<Node> unbalancedNodes = new Queue<Node>();
			Iterate(Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
			{
				if (IsBalanced(e)) return;
				unbalancedNodes.Enqueue(e);
			});

			while (unbalancedNodes.Count > 0)
			{
				Node node = unbalancedNodes.Dequeue();
				// check again if status changed
				if (IsBalanced(node)) continue;
				FixRedRed(node);
			}
		}

		private void FixRedRed(Node node)
		{
			/*
			 * balance the tree
			 * There are 2 cases with 3 sub-cases for each unbalanced node
			 *
			 * 1. parent is left
			 *	1.1 uncle (parent.Parent.Right) is Red => Recolor only
			 *	1.2 node is right => Rotate left
			 *	1.3 node is left => Rotate right
			 * 2. parent is right
			 *	2.1 uncle (parent.Parent.Right) is Red => Recolor only
			 *	2.2 node is left => Rotate right
			 *	2.3 node is right => Rotate left
			 *
			 *     y                               x
			 *    / \     Right Rotation(y) ->    /  \
			 *   x   T3                          T1   y 
			 *  / \                                  / \
			 * T1  T2     <- Left Rotation(x)       T2  T3
			 */
			if (node == null || IsBalanced(node)) return;

			while (!node.IsRoot && node.Color && node.Parent.Color)
			{
				Node parent = node.Parent;
				Node grandParent = parent.Parent;

				// 1. parent is left
				if (parent.IsLeft)
				{
					Node uncle = grandParent.Right;

					// 1.1 uncle is Red
					if (uncle != null && uncle.Color)
					{
						grandParent.Color = true;
						parent.Color = false;
						uncle.Color = false;
						node = grandParent;
					}
					else
					{
						// 1.2 node is right
						if (node.IsRight)
						{
							RotateLeft(parent);
							node = parent;
							parent = node.Parent;
						}

						// 1.3 node is left
						RotateRight(grandParent);
						parent.SwapColor(grandParent);
						node = parent;
					}
				}
				else
				{
					// 2. parent is right - because !node.IsRoot && !parent.IsLeft
					Node uncle = grandParent.Left;

					// 2.1 uncle is Red
					if (uncle != null && uncle.Color)
					{
						grandParent.Color = true;
						parent.Color = false;
						uncle.Color = false;
						node = grandParent;
					}
					else
					{
						// 2.2 node is left
						if (node.IsLeft)
						{
							RotateRight(parent);
							node = parent;
							parent = node.Parent;
						}

						// 2.3 node is right
						RotateLeft(grandParent);
						parent.SwapColor(grandParent);
						node = parent;
					}
				}
			}

			if (node.IsRoot)
			{
				Root = node;
				_version++;
			}

			Root.Color = false;
		}

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
		private Node RotateLeft([NotNull] Node sourceNode /* x */)
		{
			bool isLeft = sourceNode.IsLeft;
			Node oldParent = sourceNode.Parent;
			Node newRoot /* y */ = sourceNode.Right;
			Node oldLeft /* T2 */ = newRoot.Left;

			// Perform rotation
			newRoot.Left = sourceNode;
			sourceNode.Right = oldLeft;

			if (oldParent != null)
			{
				if (isLeft) oldParent.Left = newRoot;
				else oldParent.Right = newRoot;
			}

			// update nodes
			Update(newRoot);
			_version++;
			// Return new root
			return newRoot;
		}

		[NotNull]
		private Node RotateRight([NotNull] Node sourceNode /* y */)
		{
			bool isLeft = sourceNode.IsLeft;
			Node oldParent = sourceNode.Parent;
			Node newRoot /* x */ = sourceNode.Left;
			Node oldRight /* T2 */ = newRoot.Right;

			// Perform rotation
			newRoot.Right = sourceNode;
			sourceNode.Left = oldRight;

			if (oldParent != null)
			{
				if (isLeft) oldParent.Left = newRoot;
				else oldParent.Right = newRoot;
			}

			// update nodes
			Update(newRoot);
			_version++;
			// Return new root
			return newRoot;
		}

		private void Update(Node node)
		{
			if (node == null) return;

			if (node.IsLeaf)
				node.Depth = 1 + (node.Parent?.Depth ?? -1);
			else
				Iterate(node, TraverseMethod.LevelOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e => e.Depth = 1 + (e.Parent?.Depth ?? -1));
		}
	}

	public static class RedBlackTreeNodeExtension
	{
		public static void Swap<T>([NotNull] this RedBlackTree<T>.Node thisValue, [NotNull] RedBlackTree<T>.Node other)
		{
			T tmp = other.Value;
			other.Value = thisValue.Value;
			thisValue.Value = tmp;
		}

		public static void SwapColor<T>([NotNull] this RedBlackTree<T>.Node thisValue, [NotNull] RedBlackTree<T>.Node other)
		{
			bool tmp = other.Color;
			other.Color = thisValue.Color;
			thisValue.Color = tmp;
		}
	}

	public static class RedBlackTreeExtension
	{
		public static string ToString<T>([NotNull] this RedBlackTree<T> thisValue, Orientation orientation, bool diagnosticInfo = false, Func<RedBlackTree<T>.Node, string> onFormatNode = null) { return ToString(thisValue, thisValue.Root, orientation, diagnosticInfo, onFormatNode); }
		public static string ToString<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node node, Orientation orientation, bool diagnosticInfo = false, Func<RedBlackTree<T>.Node, string> onFormatNode = null)
		{
			if (node == null) return string.Empty;
			if (node.IsLeaf) return Format(node, diagnosticInfo, onFormatNode);
			return orientation switch
			{
				Orientation.Horizontal => Horizontally(thisValue, node, diagnosticInfo, onFormatNode),
				Orientation.Vertical => Vertically(thisValue, node, diagnosticInfo, onFormatNode),
				_ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
			};

			static string Format(RedBlackTree<T>.Node node, bool diagnosticInfo, Func<RedBlackTree<T>.Node, string> formatNode)
			{
				if (node == null) return string.Empty;
				string nodeStr = formatNode?.Invoke(node) ?? node.ToString();
				return diagnosticInfo
							? $"{node} :D{node.Depth}{(node.Color ? 'R' : 'B')}"
							: node.ToString();
			}

			static string Horizontally(RedBlackTree<T> tree, RedBlackTree<T>.Node node, bool diagnostic, Func<RedBlackTree<T>.Node, string> formatNode)
			{
				const string STR_BLANK = "    ";
				const string STR_EXT = "│   ";
				const string STR_CONNECTOR = "─── ";
				const string STR_CONNECTOR_L = "└── ";
				const string STR_CONNECTOR_R = "┌── ";

				StringBuilder sb = new StringBuilder();
				Stack<string> connectors = new Stack<string>();

				tree.Iterate(node, TraverseMethod.InOrder, HorizontalFlow.RightToLeft, HorizontalDirectionFlags.Default, e =>
				{
					connectors.Push(Format(e, diagnostic, formatNode));

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
					{
						sb.Append(connectors.Pop());
					}

					sb.AppendLine(connectors.Pop());
				});

				return sb.ToString();
			}

			static string Vertically(RedBlackTree<T> tree, RedBlackTree<T>.Node node, bool diagnostic, Func<RedBlackTree<T>.Node, string> formatNode)
			{
				const char C_BLANK = ' ';
				const char C_EXT = '─';
				const char C_CONNECTOR_L = '┌';
				const char C_CONNECTOR_R = '┐';

				int distance = 0;
				IDictionary<int, StringBuilder> lines = new Dictionary<int, StringBuilder>();
				tree.Iterate(node, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
				{
					StringBuilder line = lines.GetOrAdd(e.Depth);

					if (line.Length > 0 && line[line.Length - 1] == C_CONNECTOR_L) line.Append(C_EXT, distance - line.Length);
					else line.Append(C_BLANK, distance - line.Length);

					if (e.Depth > 0)
					{
						StringBuilder prevLine = lines.GetOrAdd(e.Depth - 1);

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
					line.Append(Format(e, diagnostic, formatNode));
					distance = line.Length;
				});

				return string.Join(Environment.NewLine, lines.OrderBy(e => e.Key).Select(e => e.Value));
			}
		}

		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalDirectionFlags direction)
		{
			return thisValue.GetEnumerator(TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction);
		}
		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalDirectionFlags direction)
		{
			return thisValue.GetEnumerator(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction);
		}

		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow)
		{
			return thisValue.GetEnumerator(TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default);
		}
		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow)
		{
			return thisValue.GetEnumerator(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default);
		}

		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, HorizontalDirectionFlags direction)
		{
			return thisValue.GetEnumerator(TraverseMethod.InOrder, flow, direction);
		}
		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, HorizontalDirectionFlags direction)
		{
			return thisValue.GetEnumerator(root, TraverseMethod.InOrder, flow, direction);
		}

		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method)
		{
			return thisValue.GetEnumerator(method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default);
		}
		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method)
		{
			return thisValue.GetEnumerator(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default);
		}

		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, HorizontalDirectionFlags direction)
		{
			return thisValue.GetEnumerator(method, HorizontalFlow.LeftToRight, direction);
		}
		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, HorizontalDirectionFlags direction)
		{
			return thisValue.GetEnumerator(root, method, HorizontalFlow.LeftToRight, direction);
		}

		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, HorizontalFlow flow)
		{
			return thisValue.GetEnumerator(method, flow, HorizontalDirectionFlags.Default);
		}
		[NotNull]
		public static IEnumerable<T> Enumerate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, HorizontalFlow flow)
		{
			return thisValue.GetEnumerator(root, method, flow, HorizontalDirectionFlags.Default);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalDirectionFlags direction, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalDirectionFlags direction, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, flow, direction, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, flow, direction, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<RedBlackTree<T>.Node, bool> visitCallback)
		{
			thisValue.Iterate(root, method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalDirectionFlags direction, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalDirectionFlags direction, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(TraverseMethod.InOrder, flow, direction, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, TraverseMethod.InOrder, flow, direction, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, TraverseMethod method, HorizontalFlow flow, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, TraverseMethod method, HorizontalFlow flow, [NotNull] Action<RedBlackTree<T>.Node> visitCallback)
		{
			thisValue.Iterate(root, method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] Func<int, IReadOnlyCollection<RedBlackTree<T>.Node>, bool> levelCallback)
		{
			thisValue.Iterate(HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, levelCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, [NotNull] Func<int, IReadOnlyCollection<RedBlackTree<T>.Node>, bool> levelCallback)
		{
			thisValue.Iterate(root, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, levelCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<RedBlackTree<T>.Node>, bool> levelCallback)
		{
			thisValue.Iterate(HorizontalFlow.LeftToRight, direction, levelCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<RedBlackTree<T>.Node>, bool> levelCallback)
		{
			thisValue.Iterate(root, HorizontalFlow.LeftToRight, direction, levelCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<RedBlackTree<T>.Node>, bool> levelCallback)
		{
			thisValue.Iterate(flow, HorizontalDirectionFlags.Default, levelCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<RedBlackTree<T>.Node>, bool> levelCallback)
		{
			thisValue.Iterate(root, flow, HorizontalDirectionFlags.Default, levelCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] Action<int, IReadOnlyCollection<RedBlackTree<T>.Node>> levelCallback)
		{
			thisValue.Iterate(HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, levelCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, [NotNull] Action<int, IReadOnlyCollection<RedBlackTree<T>.Node>> levelCallback)
		{
			thisValue.Iterate(root, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, levelCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<RedBlackTree<T>.Node>> levelCallback)
		{
			thisValue.Iterate(HorizontalFlow.LeftToRight, direction, levelCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<RedBlackTree<T>.Node>> levelCallback)
		{
			thisValue.Iterate(root, HorizontalFlow.LeftToRight, direction, levelCallback);
		}

		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<RedBlackTree<T>.Node>> levelCallback)
		{
			thisValue.Iterate(flow, HorizontalDirectionFlags.Default, levelCallback);
		}
		public static void Iterate<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node root, HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<RedBlackTree<T>.Node>> levelCallback)
		{
			thisValue.Iterate(root, flow, HorizontalDirectionFlags.Default, levelCallback);
		}

		// todo
		///// <summary>
		///// Fill a <see cref="RedBlackTree{T}"/> from the LevelOrder <see cref="collection"/>.
		///// <para>
		///// LevelOrder => Root-Left-Right (Queue)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="collection"></param>
		//public static void FromLevelOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		//{
		//	IReadOnlyList<T> list = new Lister<T>(collection);
		//	// try simple cases first
		//	if (FromSimpleList(thisValue, list)) return;

		//	int index = 0;
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(list[index++]);
		//	thisValue.Root = node;

		//	IComparer<T> comparer = thisValue.Comparer;
		//	Queue<RedBlackTree<T>.Node> queue = new Queue<RedBlackTree<T>.Node>();
		//	queue.Enqueue(node);

		//	// not all queued items will be parents, it's expected the queue will contain enough nodes
		//	while (index < list.Count)
		//	{
		//		int oldIndex = index;
		//		RedBlackTree<T>.Node root = queue.Dequeue();

		//		// add left node
		//		if (comparer.IsLessThan(list[index], root.Value))
		//		{
		//			node = new RedBlackTree<T>.Node(list[index]);
		//			root.Left = node;
		//			queue.Enqueue(node);
		//			index++;
		//		}

		//		// add right node
		//		if (index < list.Count && comparer.IsGreaterThanOrEqual(list[index], root.Value))
		//		{
		//			node = new RedBlackTree<T>.Node(list[index]);
		//			root.Right = node;
		//			queue.Enqueue(node);
		//			index++;
		//		}

		//		if (oldIndex == index) index++;
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();
		//}

		///// <summary>
		///// Constructs a <see cref="RedBlackTree{T}"/> from the PreOrder <see cref="collection"/>.
		///// <para>
		///// PreOrder => Root-Left-Right (Stack)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="collection"></param>
		//public static void FromPreOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		//{
		//	// https://www.geeksforgeeks.org/construct-bst-from-given-preorder-traversal-set-2/
		//	IReadOnlyList<T> list = new Lister<T>(collection);
		//	// try simple cases first
		//	if (FromSimpleList(thisValue, list)) return;

		//	// first node of PreOrder will be root of tree
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(list[0]);
		//	thisValue.Root = node;

		//	Stack<RedBlackTree<T>.Node> stack = new Stack<RedBlackTree<T>.Node>();
		//	// Push root of the BST to the stack i.e, first element of the array.
		//	stack.Push(node);

		//	/*
		//	 * Keep popping nodes while the stack is not empty.
		//	 * When the value is greater than stack’s top value, make it the right
		//	 * child of the last popped node and push it to the stack.
		//	 * If the next value is less than the stack’s top value, make it the left
		//	 * child of the stack’s top node and push it to the stack.
		//	 */
		//	IComparer<T> comparer = thisValue.Comparer;

		//	// Traverse from second node
		//	for (int i = 1; i < list.Count; i++)
		//	{
		//		RedBlackTree<T>.Node root = null;

		//		// Keep popping nodes while top of stack is greater.
		//		while (stack.Count > 0 && comparer.IsGreaterThan(list[i], stack.Peek().Value))
		//			root = stack.Pop();

		//		node = new RedBlackTree<T>.Node(list[i]);

		//		if (root != null) root.Right = node;
		//		else if (stack.Count > 0) stack.Peek().Left = node;

		//		stack.Push(node);
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();
		//}

		///// <summary>
		///// Constructs a <see cref="RedBlackTree{T}"/> from the InOrder <see cref="collection"/>.
		///// <para>
		///// Note that it is not possible to construct a unique binary tree from InOrder collection alone.
		///// </para>
		///// <para>
		///// InOrder => Left-Root-Right (Stack)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="collection"></param>
		//public static void FromInOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		//{
		//	IReadOnlyList<T> list = new Lister<T>(collection);
		//	// try simple cases first
		//	if (FromSimpleList(thisValue, list)) return;

		//	int start = 0;
		//	int end = list.Count - 1;
		//	int index = IndexMid(start, end);
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(list[index]);
		//	thisValue.Root = node;

		//	Queue<(int Index, int Start, int End, RedBlackTree<T>.Node Node)> queue = new Queue<(int Index, int Start, int End, RedBlackTree<T>.Node Node)>();
		//	queue.Enqueue((index, start, end, node));

		//	while (queue.Count > 0)
		//	{
		//		(int Index, int Start, int End, RedBlackTree<T>.Node Node) tuple = queue.Dequeue();

		//		// get the next left index
		//		start = tuple.Start;
		//		end = tuple.Index - 1;
		//		int nodeIndex = IndexMid(start, end);

		//		// add left node
		//		if (nodeIndex > -1)
		//		{
		//			node = new RedBlackTree<T>.Node(list[nodeIndex]);
		//			tuple.Node.Left = node;
		//			queue.Enqueue((nodeIndex, start, end, node));
		//		}

		//		// get the next right index
		//		start = tuple.Index + 1;
		//		end = tuple.End;
		//		nodeIndex = IndexMid(start, end);

		//		// add right node
		//		if (nodeIndex > -1)
		//		{
		//			node = new RedBlackTree<T>.Node(list[nodeIndex]);
		//			tuple.Node.Right = node;
		//			queue.Enqueue((nodeIndex, start, end, node));
		//		}
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();

		//	static int IndexMid(int start, int end)
		//	{
		//		return start > end
		//					? -1
		//					: start + (end - start) / 2;
		//	}
		//}

		///// <summary>
		///// Constructs a <see cref="RedBlackTree{T}"/> from the PostOrder <see cref="collection"/>.
		///// <para>
		///// PostOrder => Left-Right-Root (Stack)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="collection"></param>
		//public static void FromPostOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> collection)
		//{
		//	// https://www.geeksforgeeks.org/construct-a-bst-from-given-postorder-traversal-using-stack/
		//	IReadOnlyList<T> list = new Lister<T>(collection);
		//	// try simple cases first
		//	if (FromSimpleList(thisValue, list)) return;

		//	// last node of PostOrder will be root of tree
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(list[list.Count - 1]);
		//	thisValue.Root = node;

		//	Stack<RedBlackTree<T>.Node> stack = new Stack<RedBlackTree<T>.Node>();
		//	// Push root of the BST to the stack i.e, last element of the array.
		//	stack.Push(node);

		//	/*
		//	 * The idea is to traverse the array in reverse.
		//	 * If next element is > the element at the top of the stack then,
		//	 * set this element as the right child of the element at the top
		//	 * of the stack and also push it to the stack.
		//	 * Else if, next element is < the element at the top of the stack then,
		//	 * start popping all the elements from the stack until either the stack
		//	 * is empty or the current element becomes > the element at the top of
		//	 * the stack.
		//	 * Make this element left child of the last popped node and repeat until
		//	 * the array is traversed completely.
		//	 */
		//	IComparer<T> comparer = thisValue.Comparer;

		//	// Traverse from second last node
		//	for (int i = list.Count - 2; i >= 0; i--)
		//	{
		//		RedBlackTree<T>.Node root = null;

		//		// Keep popping nodes while top of stack is greater.
		//		while (stack.Count > 0 && comparer.IsLessThan(list[i], stack.Peek().Value))
		//			root = stack.Pop();

		//		node = new RedBlackTree<T>.Node(list[i]);

		//		if (root != null) root.Left = node;
		//		else if (stack.Count > 0) stack.Peek().Right = node;

		//		stack.Push(node);
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();
		//}

		///// <summary>
		///// Constructs a <see cref="RedBlackTree{T}"/> from <see cref="inOrderCollection"/> and <see cref="levelOrderCollection"/>.
		///// <para>
		///// InOrder => Left-Root-Right (Stack)
		///// </para>
		///// <para>
		///// LevelOrder => Root-Left-Right (Queue)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="inOrderCollection"></param>
		///// <param name="levelOrderCollection"></param>
		//public static void FromInOrderAndLevelOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> levelOrderCollection)
		//{
		//	IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
		//	// Root-Left-Right
		//	IReadOnlyList<T> levelOrder = new Lister<T>(levelOrderCollection);
		//	if (inOrder.Count != levelOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(levelOrderCollection));
		//	if (levelOrder.Count == 1 && inOrder.Count == 1 && !thisValue.Comparer.IsEqual(inOrder[0], levelOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(levelOrderCollection));
		//	// try simple cases first
		//	if (FromSimpleList(thisValue, levelOrder)) return;

		//	/*
		//	 * using the facts that:
		//	 * 1. LevelOrder is organized in the form Root-Left-Right.
		//	 * 2. InOrder is organized in the form Left-Root-Right,
		//	 * from 1 and 2, the LevelOrder list can be used to identify
		//	 * the root and other elements locations in the InOrder list.
		//	 */
		//	// the lookup will enhance the speed of looking for the index of the item to O(1)
		//	IDictionary<T, int> lookup = new Dictionary<T, int>(thisValue.Comparer.AsEqualityComparer());

		//	// add all InOrder items to the lookup
		//	for (int i = 0; i < inOrder.Count; i++)
		//	{
		//		T key = inOrder[i];
		//		if (lookup.ContainsKey(key)) continue;
		//		lookup.Add(key, i);
		//	}

		//	int index = 0;
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(levelOrder[index++]);
		//	thisValue.Root = node;

		//	Queue<(int Start, int End, RedBlackTree<T>.Node Node)> queue = new Queue<(int Start, int End, RedBlackTree<T>.Node Node)>();
		//	queue.Enqueue((0, inOrder.Count - 1, node));

		//	while (index < levelOrder.Count && queue.Count > 0)
		//	{
		//		(int Start, int End, RedBlackTree<T>.Node Node) tuple = queue.Dequeue();

		//		// get the root index (the current node index in the InOrder collection)
		//		int rootIndex = lookup[tuple.Node.Value];
		//		// find out the index of the next entry of LevelOrder in the InOrder collection
		//		int levelIndex = lookup[levelOrder[index]];

		//		// add left node
		//		if (levelIndex >= tuple.Start && levelIndex <= rootIndex - 1)
		//		{
		//			node = new RedBlackTree<T>.Node(inOrder[levelIndex]);
		//			tuple.Node.Left = node;
		//			queue.Enqueue((tuple.Start, rootIndex - 1, node));
		//			index++;
		//			// index and node changed, so will need to get the next entry of LevelOrder in the InOrder collection
		//			levelIndex = index < levelOrder.Count
		//							? lookup[levelOrder[index]]
		//							: -1;
		//		}

		//		// add right node
		//		if (levelIndex >= rootIndex + 1 && levelIndex <= tuple.End)
		//		{
		//			node = new RedBlackTree<T>.Node(inOrder[levelIndex]);
		//			tuple.Node.Right = node;
		//			queue.Enqueue((rootIndex + 1, tuple.End, node));
		//			index++;
		//		}
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();
		//}

		///// <summary>
		///// Constructs a <see cref="RedBlackTree{T}"/> from <see cref="inOrderCollection"/> and <see cref="preOrderCollection"/>.
		///// <para>
		///// InOrder => Left-Root-Right (Stack)
		///// </para>
		///// <para>
		///// PreOrder => Root-Left-Right (Stack)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="inOrderCollection"></param>
		///// <param name="preOrderCollection"></param>
		//public static void FromInOrderAndPreOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> preOrderCollection)
		//{
		//	IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
		//	IReadOnlyList<T> preOrder = new Lister<T>(preOrderCollection);
		//	if (inOrder.Count != preOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(preOrderCollection));
		//	if (preOrder.Count == 1 && inOrder.Count == 1 && !thisValue.Comparer.IsEqual(inOrder[0], preOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(preOrderCollection));
		//	// try simple cases first
		//	if (FromSimpleList(thisValue, preOrder)) return;

		//	/*
		//	 * https://stackoverflow.com/questions/48352513/construct-binary-tree-given-its-inorder-and-preorder-traversals-without-recursio#48364040
		//	 * 
		//	 * The idea is to keep tree nodes in a stack from PreOrder traversal, till their counterpart is not found in InOrder traversal.
		//	 * Once a counterpart is found, all children in the left sub-tree of the node must have been already visited.
		//	 */
		//	int preIndex = 0;
		//	int inIndex = 0;
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(preOrder[preIndex++]);
		//	thisValue.Root = node;

		//	IComparer<T> comparer = thisValue.Comparer;
		//	Stack<RedBlackTree<T>.Node> stack = new Stack<RedBlackTree<T>.Node>();
		//	stack.Push(node);

		//	while (stack.Count > 0)
		//	{
		//		RedBlackTree<T>.Node root = stack.Peek();

		//		if (comparer.IsEqual(root.Value, inOrder[inIndex]))
		//		{
		//			stack.Pop();
		//			inIndex++;

		//			// if all the elements in inOrder have been visited, we are done
		//			if (inIndex == inOrder.Count) break;
		//			// if there are still some unvisited nodes in the left, skip
		//			if (stack.Count > 0 && comparer.IsEqual(stack.Peek().Value, inOrder[inIndex])) continue;

		//			/*
		//			 * As top node in stack, still has not encountered its counterpart
		//			 * in inOrder, so next element in preOrder must be right child of
		//			 * the removed node
		//			 */
		//			node = new RedBlackTree<T>.Node(preOrder[preIndex++]);
		//			root.Right = node;
		//			stack.Push(node);
		//		}
		//		else
		//		{
		//			/*
		//			 * Top node in the stack has not encountered its counterpart
		//			 * in inOrder, so next element in preOrder must be left child
		//			 * of this node
		//			 */
		//			node = new RedBlackTree<T>.Node(preOrder[preIndex++]);
		//			root.Left = node;
		//			stack.Push(node);
		//		}
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();
		//}

		///// <summary>
		///// Constructs a <see cref="RedBlackTree{T}"/> from <see cref="inOrderCollection"/> and <see cref="postOrderCollection"/>.
		///// <para>
		///// InOrder => Left-Root-Right (Stack)
		///// </para>
		///// <para>
		///// PostOrder => Left-Right-Root (Stack)
		///// </para>
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="thisValue"></param>
		///// <param name="inOrderCollection"></param>
		///// <param name="postOrderCollection"></param>
		//public static void FromInOrderAndPostOrder<T>([NotNull] this RedBlackTree<T> thisValue, [NotNull] IEnumerable<T> inOrderCollection, [NotNull] IEnumerable<T> postOrderCollection)
		//{
		//	IReadOnlyList<T> inOrder = new Lister<T>(inOrderCollection);
		//	IReadOnlyList<T> postOrder = new Lister<T>(postOrderCollection);
		//	if (inOrder.Count != postOrder.Count) ThrowNotFormingATree(nameof(inOrderCollection), nameof(postOrderCollection));
		//	if (postOrder.Count == 1 && inOrder.Count == 1 && !thisValue.Comparer.IsEqual(inOrder[0], postOrder[0])) ThrowNotFormingATree(nameof(inOrderCollection), nameof(postOrderCollection));
		//	if (FromSimpleList(thisValue, postOrder)) return;

		//	// the lookup will enhance the speed of looking for the index of the item to O(1)
		//	IDictionary<T, int> lookup = new Dictionary<T, int>(thisValue.Comparer.AsEqualityComparer());

		//	// add all InOrder items to the lookup
		//	for (int i = 0; i < inOrder.Count; i++)
		//	{
		//		T key = inOrder[i];
		//		if (lookup.ContainsKey(key)) continue;
		//		lookup.Add(key, i);
		//	}

		//	// Traverse postOrder in reverse
		//	int postIndex = postOrder.Count - 1;
		//	RedBlackTree<T>.Node node = new RedBlackTree<T>.Node(postOrder[postIndex--]);
		//	thisValue.Root = node;

		//	Stack<RedBlackTree<T>.Node> stack = new Stack<RedBlackTree<T>.Node>();
		//	// Push root of the BST to the stack i.e, last element of the array.
		//	stack.Push(node);

		//	IComparer<T> comparer = thisValue.Comparer;

		//	while (postIndex >= 0 && stack.Count > 0)
		//	{
		//		RedBlackTree<T>.Node root = stack.Peek();
		//		// get the root index (the current node index in the InOrder collection)
		//		int rootIndex = lookup[root.Value];
		//		// find out the index of the next entry of PostOrder in the InOrder collection
		//		int index = lookup[postOrder[postIndex]];

		//		// add right node
		//		if (index > rootIndex)
		//		{
		//			node = new RedBlackTree<T>.Node(inOrder[index]);
		//			root.Right = node;
		//			stack.Push(node);
		//			postIndex--;

		//			// index and node changed, so will need to get the next entry of PostOrder in the InOrder collection
		//			index = postIndex > -1
		//						? lookup[postOrder[postIndex]]
		//						: -1;
		//		}

		//		// add left node
		//		if (index > -1 && index < rootIndex)
		//		{
		//			if (comparer.IsLessThan(postOrder[postIndex], root.Value))
		//			{
		//				// Keep popping nodes while top of stack is greater.
		//				while (stack.Count > 0 && comparer.IsLessThan(postOrder[postIndex], stack.Peek().Value))
		//					root = stack.Pop();
		//			}

		//			node = new RedBlackTree<T>.Node(inOrder[index]);
		//			root.Left = node;
		//			stack.Push(node);
		//			postIndex--;
		//		}
		//	}

		//	Update(thisValue);
		//	thisValue.Balance();
		//}

		private static bool FromSimpleList<T>([NotNull] RedBlackTree<T> tree, [NotNull] IReadOnlyList<T> list)
		{
			bool result;
			tree.Clear();

			switch (list.Count)
			{
				case 0:
					result = true;
					break;
				case 1:
					tree.Root = new RedBlackTree<T>.Node(list[0]) { Color = false };
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
}
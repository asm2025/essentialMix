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
						if (_nodes[PARENT]._nodes[LEFT] == this) _nodes[PARENT]._nodes[LEFT] = null;
						else if (_nodes[PARENT]._nodes[RIGHT] == this) _nodes[PARENT]._nodes[RIGHT] = null;
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
					// reset old left
					if (_nodes[LEFT]?._nodes[PARENT] == this) _nodes[LEFT]._nodes[PARENT] = null;
					_nodes[LEFT] = value;
					if (_nodes[LEFT] == null) return;
					_nodes[LEFT]._nodes[PARENT] = this;
				}
			}

			public Node Right
			{
				get => _nodes[RIGHT];
				internal set
				{
					if (_nodes[RIGHT] == value) return;
					// reset old right
					if (_nodes[RIGHT]?._nodes[PARENT] == this) _nodes[RIGHT]._nodes[PARENT] = null;
					_nodes[RIGHT] = value;
					if (_nodes[RIGHT] == null) return;
					_nodes[RIGHT]._nodes[PARENT] = this;
				}
			}

			public T Value { get; set; }

			/// <summary>
			/// True means Red and False = no color or Black
			/// </summary>
			public bool Color { get; internal set; } = true;

			public bool IsRoot => _nodes[PARENT] == null;

			public bool IsLeft => _nodes[PARENT]?._nodes[LEFT] == this;

			public bool IsRight => _nodes[PARENT]?._nodes[RIGHT] == this;

			public bool IsLeaf => _nodes[LEFT] == null && _nodes[RIGHT] == null;

			public bool IsNode => _nodes[LEFT] != null && _nodes[RIGHT] != null;

			public bool HasOneChild => (_nodes[LEFT] != null) ^ (_nodes[RIGHT] != null);

			public bool IsFull => !HasOneChild;

			public bool HasRedParent => _nodes[PARENT] != null && _nodes[PARENT].Color;

			public bool HasRedLeft => _nodes[LEFT] != null && _nodes[LEFT].Color;

			public bool HasRedRight => _nodes[RIGHT] != null && _nodes[RIGHT].Color;

			/// <inheritdoc />
			[NotNull]
			public override string ToString() { return Convert.ToString(Value); }

			[ItemNotNull]
			public IEnumerable<Node> Ancestors()
			{
				Node node = _nodes[PARENT];

				while (node != null)
				{
					yield return node;
					node = node._nodes[PARENT];
				}
			}

			public Node Uncle()
			{
				// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
				return _nodes[PARENT]?._nodes[PARENT] == null
							? null // no parent or grand parent
							: _nodes[PARENT].IsLeft
								? _nodes[PARENT]._nodes[PARENT]._nodes[RIGHT] // uncle on the right
								: _nodes[PARENT]._nodes[PARENT]._nodes[LEFT];
			}

			public Node Sibling()
			{
				// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
				return _nodes[PARENT] == null
							? null // no parent
							: IsLeft
								? _nodes[PARENT]._nodes[RIGHT] // sibling on the right
								: _nodes[PARENT]._nodes[LEFT];
			}
			
			public void Swap([NotNull] Node other)
			{
				T tmp = other.Value;
				other.Value = Value;
				Value = tmp;
			}
			
			public void SwapColor([NotNull] Node other)
			{
				bool tmp = other.Color;
				other.Color = Color;
				Color = tmp;
			}

			[NotNull]
			public Node Minimum()
			{
				Node minimum = this;

				while (minimum._nodes[LEFT] != null) 
					minimum = minimum._nodes[LEFT];

				return minimum;
			}

			[NotNull]
			public Node Maximum()
			{
				Node maximum = this;

				while (maximum._nodes[RIGHT] != null) 
					maximum = maximum._nodes[RIGHT];

				return maximum;
			}

			public Node Predecessor()
			{
				if (_nodes[LEFT] != null) return _nodes[LEFT].Maximum();

				Node node = this;
				Node parent = _nodes[PARENT];

				while (parent != null && node == parent._nodes[LEFT])
				{
					node = parent;
					parent = parent._nodes[PARENT];
				}

				return parent;
			}

			public Node Successor()
			{
				if (_nodes[RIGHT] != null) return _nodes[RIGHT].Minimum();

				Node node = this;
				Node parent = _nodes[PARENT];

				while (parent != null && node == parent._nodes[RIGHT])
				{
					node = parent;
					parent = parent._nodes[PARENT];
				}

				return parent;
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

			public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<Node, int, bool> visitCallback)
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
					Queue<(int Depth, Node Node)> queue = new Queue<(int Depth, Node Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = queue.Dequeue();
						if (!visitCallback(node, depth)) break;

						// Queue the next nodes
						if ((!node.IsRoot || left) && node.Left != null) queue.Enqueue((depth + 1, node.Left));
						if ((!node.IsRoot || right) && node.Right != null) queue.Enqueue((depth + 1, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					(bool left, bool right) = direction.GetDirections();
					Queue<(int Depth, Node Node)> queue = new Queue<(int Depth, Node Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = queue.Dequeue();
						if (!visitCallback(node, depth)) break;

						// Queue the next nodes
						if ((!node.IsRoot || right) && node.Right != null) queue.Enqueue((depth + 1, node.Right));
						if ((!node.IsRoot || left) && node.Left != null) queue.Enqueue((depth + 1, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = stack.Pop();
						if (!visitCallback(node, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!node.IsRoot || right) && node.Right != null) stack.Push((depth + 1, node.Right));
						if ((!node.IsRoot || left) && node.Left != null) stack.Push((depth + 1, node.Left));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = stack.Pop();
						if (!visitCallback(node, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!node.IsRoot || left) && node.Left != null) stack.Push((depth + 1, node.Left));
						if ((!node.IsRoot || right) && node.Right != null) stack.Push((depth + 1, node.Right));
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Node.IsRoot
										? left
											? (current.Depth + 1, current.Node.Left)
											: (-1, null)
										: (current.Depth + 1, current.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.Node, current.Depth)) break;

							// Navigate right
							current = current.Node.IsRoot
										? right
											? (current.Depth + 1, current.Node.Right)
											: (-1, null)
										: (current.Depth + 1, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Node.IsRoot
										? right
											? (current.Depth + 1, current.Node.Right)
											: (-1, null)
										: (current.Depth + 1, current.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.Node, current.Depth)) break;

							// Navigate right
							current = current.Node.IsRoot
										? left
											? (current.Depth + 1, current.Node.Left)
											: (-1, null)
										: (current.Depth + 1, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Node.IsRoot
										? left
											? (current.Depth + 1, current.Node.Left)
											: (-1, null)
										: (current.Depth + 1, current.Node.Left);
							continue;
						}

						(int Depth, Node Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right Node
						 * if yes, then navigate right.
						 */
						if (peek.Node.Right != null && lastVisited != peek.Node.Right)
						{
							// Navigate right
							current = peek.Node.IsRoot
										? right
											? (peek.Depth + 1, peek.Node.Right)
											: (-1, null)
										: (peek.Depth + 1, peek.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							if (!visitCallback(current.Node, current.Depth)) break;
							current = (-1, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Node.IsRoot
										? right
											? (current.Depth + 1, current.Node.Right)
											: (-1, null)
										: (current.Depth + 1, current.Node.Right);
							continue;
						}

						(int Depth, Node Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left Node
						 * if yes, then navigate left.
						 */
						if (peek.Node.Left != null && lastVisited != peek.Node.Left)
						{
							// Navigate left
							current = peek.Node.IsRoot
										? left
											? (peek.Depth + 1, peek.Node.Left)
											: (-1, null)
										: (peek.Depth + 1, peek.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							if (!visitCallback(current.Node, current.Depth)) break;
							current = (-1, null);
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

			public void Iterate(HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<Node, int> visitCallback)
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
					Queue<(int Depth, Node Node)> queue = new Queue<(int Depth, Node Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = queue.Dequeue();
						visitCallback(node, depth);

						// Queue the next nodes
						if ((!node.IsRoot || left) && node.Left != null) queue.Enqueue((depth + 1, node.Left));
						if ((!node.IsRoot || right) && node.Right != null) queue.Enqueue((depth + 1, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					(bool left, bool right) = direction.GetDirections();
					Queue<(int Depth, Node Node)> queue = new Queue<(int Depth, Node Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = queue.Dequeue();
						visitCallback(node, depth);

						// Queue the next nodes
						if ((!node.IsRoot || right) && node.Right != null) queue.Enqueue((depth + 1, node.Right));
						if ((!node.IsRoot || left) && node.Left != null) queue.Enqueue((depth + 1, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = stack.Pop();
						visitCallback(node, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!node.IsRoot || right) && node.Right != null) stack.Push((depth + 1, node.Right));
						if ((!node.IsRoot || left) && node.Left != null) stack.Push((depth + 1, node.Left));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, Node node) = stack.Pop();
						visitCallback(node, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if ((!node.IsRoot || left) && node.Left != null) stack.Push((depth + 1, node.Left));
						if ((!node.IsRoot || right) && node.Right != null) stack.Push((depth + 1, node.Right));
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Node.IsRoot
										? left
											? (current.Depth + 1, current.Node.Left)
											: (-1, null)
										: (current.Depth + 1, current.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.Node, current.Depth);

							// Navigate right
							current = current.Node.IsRoot
										? right
											? (current.Depth + 1, current.Node.Right)
											: (-1, null)
										: (current.Depth + 1, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					(bool left, bool right) = direction.GetDirections();
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();

					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Node.IsRoot
										? right
											? (current.Depth + 1, current.Node.Right)
											: (-1, null)
										: (current.Depth + 1, current.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.Node, current.Depth);

							// Navigate right
							current = current.Node.IsRoot
										? left
											? (current.Depth + 1, current.Node.Left)
											: (-1, null)
										: (current.Depth + 1, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = current.Node.IsRoot
										? left
											? (current.Depth + 1, current.Node.Left)
											: (-1, null)
										: (current.Depth + 1, current.Node.Left);
							continue;
						}

						(int Depth, Node Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right Node
						 * if yes, then navigate right.
						 */
						if (peek.Node.Right != null && lastVisited != peek.Node.Right)
						{
							// Navigate right
							current = peek.Node.IsRoot
										? right
											? (peek.Depth + 1, peek.Node.Right)
											: (-1, null)
										: (peek.Depth + 1, peek.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							visitCallback(current.Node, current.Depth);
							current = (-1, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, Node Node)> stack = new Stack<(int Depth, Node Node)>();
					(bool left, bool right) = direction.GetDirections();
					Node lastVisited = null;
					// Start at the root
					(int Depth, Node Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = current.Node.IsRoot
										? right
											? (current.Depth + 1, current.Node.Right)
											: (-1, null)
										: (current.Depth + 1, current.Node.Right);
							continue;
						}

						(int Depth, Node Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left Node
						 * if yes, then navigate left.
						 */
						if (peek.Node.Left != null && lastVisited != peek.Node.Left)
						{
							// Navigate left
							current = peek.Node.IsRoot
										? left
											? (peek.Depth + 1, peek.Node.Left)
											: (-1, null)
										: (peek.Depth + 1, peek.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							visitCallback(current.Node, current.Depth);
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
			foreach (T value in collection) 
				Add(value);
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
			return (IEnumerator<T>)Enumerate(Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default);
		}

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <returns></returns>
		[NotNull]
		public IEnumerable<T> Enumerate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction)
		{
			return root == null
						? Enumerable.Empty<T>()
						: new Enumerator(this, root, method, flow, direction);
		}

		#region Enumerate overloads
		[NotNull]
		public IEnumerable<T> Enumerate(Node root, HorizontalDirectionFlags direction)
		{
			return Enumerate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(Node root, HorizontalFlow flow)
		{
			return Enumerate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction)
		{
			return Enumerate(root, TraverseMethod.InOrder, flow, direction);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(Node root, TraverseMethod method)
		{
			return Enumerate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(Node root, TraverseMethod method, HorizontalDirectionFlags direction)
		{
			return Enumerate(root, method, HorizontalFlow.LeftToRight, direction);
		}

		[NotNull]
		public IEnumerable<T> Enumerate(Node root, TraverseMethod method, HorizontalFlow flow)
		{
			return Enumerate(root, method, flow, HorizontalDirectionFlags.Default);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Func<Node, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, direction, visitCallback);
		}

		#region Iterate overloads - visitCallback function
		public void Iterate(Node root, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalDirectionFlags direction, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<Node, bool> visitCallback)
		{
			Iterate(root, method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <param name="visitCallback">callback function to handle the node with depth awareness that can cancel the loop</param>
		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Func<Node, int, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, direction, visitCallback);
		}

		#region Iterate overloads - visitCallback with depth function
		public void Iterate(Node root, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalDirectionFlags direction, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<Node, int, bool> visitCallback)
		{
			Iterate(root, method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Action<Node> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, direction, visitCallback);
		}

		#region Iterate overloads - visitCallback action
		public void Iterate(Node root, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalDirectionFlags direction, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, [NotNull] Action<Node> visitCallback)
		{
			Iterate(root, method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <param name="visitCallback">callback action to handle the node with depth awareness</param>
		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, HorizontalDirectionFlags direction, Action<Node, int> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, direction, visitCallback);
		}

		#region Iterate overloads - visitCallback with depth action
		public void Iterate(Node root, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalDirectionFlags direction, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalDirectionFlags direction, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, direction, visitCallback);
		}

		public void Iterate(Node root, TraverseMethod method, HorizontalFlow flow, [NotNull] Action<Node, int> visitCallback)
		{
			Iterate(root, method, flow, HorizontalDirectionFlags.Default, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <param name="levelCallback">callback function to handle the nodes of the level and can cancel the loop.</param>
		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
		{
			if (root == null) return;
			new LevelIterator(this, root).Iterate(flow, direction, levelCallback);
		}

		#region LevelIterate overloads - visitCallback function
		public void Iterate(Node root, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
		{
			Iterate(root, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, levelCallback);
		}

		public void Iterate(Node root, HorizontalDirectionFlags direction, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
		{
			Iterate(root, HorizontalFlow.LeftToRight, direction, levelCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<Node>, bool> levelCallback)
		{
			Iterate(root, flow, HorizontalDirectionFlags.Default, levelCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="TraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="direction">left branch, right branch or default which will traverse both branches</param>
		/// <param name="levelCallback">callback action to handle the nodes of the level.</param>
		public void Iterate(Node root, HorizontalFlow flow, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
		{
			if (root == null) return;
			new LevelIterator(this, root).Iterate(flow, direction, levelCallback);
		}

		#region LevelIterate overloads - visitCallback function
		public void Iterate(Node root, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
		{
			Iterate(root, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, levelCallback);
		}

		public void Iterate(Node root, HorizontalDirectionFlags direction, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
		{
			Iterate(root, HorizontalFlow.LeftToRight, direction, levelCallback);
		}

		public void Iterate(Node root, HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<Node>> levelCallback)
		{
			Iterate(root, flow, HorizontalDirectionFlags.Default, levelCallback);
		}
		#endregion

		/// <inheritdoc />
		public bool Contains(T value) { return Find(value) != null; }

		/// <summary>
		/// Finds the node with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node or null if no match is found</returns>
		public Node Find(T value)
		{
			Node current = FindNearestLeaf(value);
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

		public Node FindNearestLeaf(T value)
		{
			Node parent = null, next = Root;

			while (next != null)
			{
				parent = next;
				int cmp = Comparer.Compare(value, next.Value);
				next = cmp == 0
							? null
							: cmp < 0
								? next.Left
								: next.Right;
			}

			return parent;
		}

		/// <inheritdoc />
		public void Add(T value)
		{
			// find a parent
			Node parent = FindNearestLeaf(value);

			if (parent == null)
			{
				// no parent means there is no root currently
				Root = new Node(value)
				{
					Color = false
				};
				Count++;
				_version++;
				return;
			}

			// duplicate values can make life miserable for us here because it will never be balanced!
			if (Comparer.IsEqual(value, parent.Value)) throw new DuplicateKeyException();

			Node node = new Node(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			Count++;
			_version++;
			if (IsBalanced(node)) return;
			Balance(node);
		}

		/// <inheritdoc />
		public bool Remove(T value)
		{
			Node node = Find(value);
			return node != null && Remove(node);
		}

		public bool Remove([NotNull] Node node)
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			Node newNode;

			// case 1: node has both left and right children
			while (node.IsNode)
			{
				newNode = node.Right.Minimum();
				newNode.Swap(node);
				node = newNode;
			}

			newNode = node.Left ?? node.Right;
			bool dblBlack = !node.Color && (newNode == null || !newNode.Color);
			Node parent = node.Parent;

			// case 2: node is a leaf
			if (newNode == null)
			{
				if (node.IsRoot)
				{
					Root = null;
				}
				else
				{
					if (dblBlack)
					{
						// newNode and node are both black, node is a leaf => fix double black at node
						Balance(node);
					}
					else
					{
						Node sibling = node.Sibling();
						if (sibling != null) sibling.Color = true;
					}

					if (node.IsLeft) parent.Left = null;
					else parent.Right = null;
				}
			}
			// case 3: node has either left or right child
			else
			{
				if (node == Root)
				{
					node.Value = newNode.Value;
					node.Left = node.Right = null;
				}
				else
				{
					// Detach the node from tree and move newNode up
					if (node.IsLeft) parent.Left = newNode;
					else parent.Right = newNode;

					if (dblBlack) Balance(newNode);
					else newNode.Color = false;
				}
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
			if (node == null) return true;

			bool isValid = true;
			Iterate(node, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, e =>
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
						&& !node.HasRedLeft
						&& !node.HasRedRight;
			}

			// node is black
			// has no red left
			return !node.HasRedLeft
					// or left is red and all its direct children are either null or black
					|| !node.Left.HasRedLeft && !node.Left.HasRedRight
					// and has no red right
					&& (!node.HasRedRight
					// or right is red and all its direct children are either null or black
					|| !node.Right.HasRedLeft && !node.Right.HasRedRight);
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
				Balance(node);
			}
		}

		private void Balance(Node node)
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			if (node?.Parent == null) return;

			// fix red-red
			if (node.Color && node.Parent.Color)
			{
				/*
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
				while (node != null && !node.IsRoot && node.Color && node.Parent.Color /* node.Parent will never be null because IsRoot is true if reached here */)
				{
					Node parent = node.Parent;
					Node grandParent = parent.Parent;
					Node uncle = node.Uncle();

					if (uncle != null && uncle.Color)
					{
						// uncle red, perform recoloring and go up
						parent.Color = uncle.Color = false;
						if (grandParent != null) grandParent.Color = true;
						node = grandParent;
						continue;
					}

					if (grandParent == null) break;

					// Else perform LR, LL, RL, RR
					if (parent.IsLeft)
					{
						if (node.IsLeft)
						{
							// Left-right case
							parent.SwapColor(grandParent);
						}
						else
						{
							RotateLeft(parent);
							node.SwapColor(grandParent);
						}

						// Left-left and left-right
						RotateRight(grandParent);
					}
					else
					{
						if (node.IsLeft)
						{
							// Right-left case
							RotateRight(parent);
							node.SwapColor(grandParent);
						}
						else
						{
							parent.SwapColor(grandParent);
						}

						// Right-right and right-left
						RotateLeft(grandParent);
					}

					break;
				}
			}
			// fix double black
			else
			{
				while (node != null && !node.IsRoot && node.Color)
				{
					Node sibling = node.Sibling(), parent = node.Parent;

					while (sibling == null && parent != null)
					{
						// No sibling, double black pushed up
						node = parent;
						sibling = node.Sibling();
						parent = node.Parent;
					}

					if (parent == null || sibling == null) return;

					if (sibling.HasRedLeft)
					{
						if (sibling.IsLeft)
						{
							// Left-left case
							sibling.Left.Color = sibling.Color;
							sibling.Color = parent.Color;
							RotateRight(parent);
						}
						else
						{
							// Right-left case
							sibling.Left.Color = parent.Color;
							RotateRight(sibling);
							RotateLeft(parent);
						}

						node = null;
					}
					else if (sibling.HasRedRight)
					{
						if (sibling.IsLeft)
						{
							// Left-right case
							sibling.Right.Color = parent.Color;
							RotateLeft(sibling);
							RotateRight(parent);
						}
						else
						{
							// Right-right case
							sibling.Right.Color = sibling.Color;
							sibling.Color = parent.Color;
							RotateLeft(parent);
						}

						node = null;
					}
					else
					{
						// 2 black children
						sibling.Color = true;

						if (parent.Color)
						{
							parent.Color = false;
							node = null;
						}
						else
						{
							node = parent;
						}
					}
				}
			}

			if (node != null && node.IsRoot)
			{
				Root = node;
				_version++;
			}

			Root.Color = false;
		}

		/*
		* https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
		*     y                               x
		*    / \     Right Rotation(y) ->    /  \
		*   x   T3                          T1   y 
		*  / \                                  / \
		* T1  T2     <- Left Rotation(x)       T2  T3
		*
		* A reference to the drawing only, not the code
		*/
		[NotNull]
		private Node RotateLeft([NotNull] Node node /* x */)
		{
			bool isLeft = node.IsLeft;
			Node oldParent /* y */ = node.Parent;
			Node newRoot /* y */ = node.Right;
			Node oldLeft /* T2 */ = newRoot.Left;

			// Perform rotation
			newRoot.Left = node;
			node.Right = oldLeft;

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
		private Node RotateRight([NotNull] Node node /* y */)
		{
			bool isLeft = node.IsLeft;
			Node oldParent /* y */ = node.Parent;
			Node newRoot /* x */ = node.Left;
			Node oldRight /* T2 */ = newRoot.Right;

			// Perform rotation
			newRoot.Right = node;
			node.Left = oldRight;

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
	}

	public static class RedBlackTreeExtension
	{
		public static string ToString<T>([NotNull] this RedBlackTree<T> thisValue, Orientation orientation, bool diagnosticInfo = false) { return ToString(thisValue, thisValue.Root, orientation, diagnosticInfo); }
		public static string ToString<T>([NotNull] this RedBlackTree<T> thisValue, RedBlackTree<T>.Node node, Orientation orientation, bool diagnosticInfo = false)
		{
			if (node == null) return string.Empty;
			if (node.IsLeaf) return Format(node, diagnosticInfo);
			return orientation switch
			{
				Orientation.Horizontal => Horizontally(thisValue, node, diagnosticInfo),
				Orientation.Vertical => Vertically(thisValue, node, diagnosticInfo),
				_ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
			};

			static string Format(RedBlackTree<T>.Node node, bool diagnostic)
			{
				if (node == null) return string.Empty;
				return diagnostic
							? $"{node} {(node.Color ? 'R' : 'B')}"
							: node.ToString();
			}

			static string Horizontally(RedBlackTree<T> tree, RedBlackTree<T>.Node node, bool diagnostic)
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
					connectors.Push(Format(e, diagnostic));

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

			static string Vertically(RedBlackTree<T> tree, RedBlackTree<T>.Node node, bool diagnostic)
			{
				const char C_BLANK = ' ';
				const char C_EXT = '─';
				const char C_CONNECTOR_L = '┌';
				const char C_CONNECTOR_R = '┐';

				int distance = 0;
				IDictionary<int, StringBuilder> lines = new Dictionary<int, StringBuilder>();
				tree.Iterate(node, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, HorizontalDirectionFlags.Default, (e, depth) =>
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
					line.Append(Format(e, diagnostic));
					distance = line.Length;
				});

				return string.Join(Environment.NewLine, lines.OrderBy(e => e.Key).Select(e => e.Value));
			}
		}
	}
}
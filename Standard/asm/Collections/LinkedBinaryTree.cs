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
	/// <see href="https://en.wikipedia.org/wiki/Binary_tree">BinaryTree</see> using the linked representation.
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
	[ComVisible(false)]
	[Serializable]
	public abstract class LinkedBinaryTree<TNode, T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
		where TNode : LinkedBinaryNode<TNode, T>
	{
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
					// Left-Right-Root (Stack)
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
					// Right-Left-Root (Stack)
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
					// Left-Right-Root (Stack)
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
					// Right-Left-Root (Stack)
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
					Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

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
					Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

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
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

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
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

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
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.Node, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.Node, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node.Left);
							continue;
						}

						(int Depth, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right TNode
						 * if yes, then navigate right.
						 */
						if (peek.Node.Right != null && lastVisited != peek.Node.Right)
						{
							// Navigate right
							current = (peek.Depth + 1, peek.Node.Right);
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
					// Right-Left-Root (Stack)
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
							continue;
						}

						(int Depth, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left TNode
						 * if yes, then navigate left.
						 */
						if (peek.Node.Left != null && lastVisited != peek.Node.Left)
						{
							// Navigate left
							current = (peek.Depth + 1, peek.Node.Left);
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
					Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

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
					Queue<(int Depth, TNode Node)> queue = new Queue<(int Depth, TNode Node)>();

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
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

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
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

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
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.Node, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();

					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.Node, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node.Left);
							continue;
						}

						(int Depth, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right TNode
						 * if yes, then navigate right.
						 */
						if (peek.Node.Right != null && lastVisited != peek.Node.Right)
						{
							// Navigate right
							current = (peek.Depth + 1, peek.Node.Right);
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
					// Right-Left-Root (Stack)
					Stack<(int Depth, TNode Node)> stack = new Stack<(int Depth, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Node) current = (0, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
							continue;
						}

						(int Depth, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left TNode
						 * if yes, then navigate left.
						 */
						if (peek.Node.Left != null && lastVisited != peek.Node.Left)
						{
							// Navigate left
							current = (peek.Depth + 1, peek.Node.Left);
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

			public void Iterate(HorizontalFlow flow, [NotNull] Action<TNode, TNode, int> visitCallback)
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
					Queue<(int Depth, TNode Parent, TNode Node)> queue = new Queue<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					queue.Enqueue((0, null, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = queue.Dequeue();
						visitCallback(node, parent, depth);

						// Queue the next nodes
						if (node.Left != null) queue.Enqueue((depth + 1, node, node.Left));
						if (node.Right != null) queue.Enqueue((depth + 1, node, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, TNode Parent, TNode Node)> queue = new Queue<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					queue.Enqueue((0, null, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = queue.Dequeue();
						visitCallback(node, parent, depth);

						// Queue the next nodes
						if (node.Right != null) queue.Enqueue((depth + 1, parent, node.Right));
						if (node.Left != null) queue.Enqueue((depth + 1, parent, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					stack.Push((0, null, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = stack.Pop();
						visitCallback(node, parent, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Right != null) stack.Push((depth + 1, node, node.Right));
						if (node.Left != null) stack.Push((depth + 1, node, node.Left));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					stack.Push((0, null, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = stack.Pop();
						visitCallback(node, parent, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Left != null) stack.Push((depth + 1, node, node.Left));
						if (node.Right != null) stack.Push((depth + 1, node, node.Right));
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node, current.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.Node, current.Parent, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							visitCallback(current.Node, current.Parent, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node, current.Node.Left);
							continue;
						}

						(int Depth, TNode Parent, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right TNode
						 * if yes, then navigate right.
						 */
						if (peek.Node.Right != null && lastVisited != peek.Node.Right)
						{
							// Navigate right
							current = (peek.Depth + 1, peek.Node, peek.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							visitCallback(current.Node, current.Parent, current.Depth);
							current = (-1, null, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Right);
							continue;
						}

						(int Depth, TNode Parent, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left TNode
						 * if yes, then navigate left.
						 */
						if (peek.Node.Left != null && lastVisited != peek.Node.Left)
						{
							// Navigate left
							current = (peek.Depth + 1, peek.Node, peek.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							visitCallback(current.Node, current.Parent, current.Depth);
							current = (-1, null, null);
						}
					}
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<TNode, TNode, int, bool> visitCallback)
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
					Queue<(int Depth, TNode Parent, TNode Node)> queue = new Queue<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					queue.Enqueue((0, null, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = queue.Dequeue();
						if (!visitCallback(node, parent, depth)) break;

						// Queue the next nodes
						if (node.Left != null) queue.Enqueue((depth + 1, node, node.Left));
						if (node.Right != null) queue.Enqueue((depth + 1, node, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, TNode Parent, TNode Node)> queue = new Queue<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					queue.Enqueue((0, null, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = queue.Dequeue();
						if (!visitCallback(node, parent, depth)) break;

						// Queue the next nodes
						if (node.Right != null) queue.Enqueue((depth + 1, node, node.Right));
						if (node.Left != null) queue.Enqueue((depth + 1, node, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					stack.Push((0, null, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = stack.Pop();
						if (!visitCallback(node, parent, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Right != null) stack.Push((depth + 1, node, node.Right));
						if (node.Left != null) stack.Push((depth + 1, node, node.Left));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					stack.Push((0, null, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, TNode parent, TNode node) = stack.Pop();
						if (!visitCallback(node, parent, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (node.Left != null) stack.Push((depth + 1, node, node.Left));
						if (node.Right != null) stack.Push((depth + 1, node, node.Right));
					}		
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node, current.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.Node, current.Parent, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();

					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = stack.Pop();
							if (!visitCallback(current.Node, current.Parent, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate left
							current = (current.Depth + 1, current.Node, current.Node.Left);
							continue;
						}

						(int Depth, TNode Parent, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right TNode
						 * if yes, then navigate right.
						 */
						if (peek.Node.Right != null && lastVisited != peek.Node.Right)
						{
							// Navigate right
							current = (peek.Depth + 1, peek.Node, peek.Node.Right);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							if (!visitCallback(current.Node, current.Parent, current.Depth)) break;
							current = (-1, null, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<(int Depth, TNode Parent, TNode Node)> stack = new Stack<(int Depth, TNode Parent, TNode Node)>();
					TNode lastVisited = null;
					// Start at the root
					(int Depth, TNode Parent, TNode Node) current = (0, null, _root);

					while (current.Node != null || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Node != null)
						{
							stack.Push(current);
							// Navigate right
							current = (current.Depth + 1, current.Node, current.Node.Right);
							continue;
						}

						(int Depth, TNode Parent, TNode Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left TNode
						 * if yes, then navigate left.
						 */
						if (peek.Node.Left != null && lastVisited != peek.Node.Left)
						{
							// Navigate left
							current = (peek.Depth + 1, peek.Node, peek.Node.Left);
						}
						else
						{
							// visit the next queued node
							current = peek;
							lastVisited = stack.Pop().Node;
							if (!visitCallback(current.Node, current.Parent, current.Depth)) break;
							current = (-1, null, null);
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

		public TNode Root { get; protected set; }

		public int Count { get; protected set; }

		public abstract bool AutoBalance { get; }

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

		public virtual bool Equals<TTree>(TTree other)
			where TTree : LinkedBinaryTree<TNode, T>
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms Part 2
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() || Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
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
		/// Iterate over nodes with a callback action and depth parameter
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node with depth awareness</param>
		public virtual void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, Action<TNode, int> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback with action + depth
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
		/// Iterate over nodes with a callback function and depth parameter
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node with depth awareness that can cancel the loop</param>
		public virtual void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<TNode, int, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback function + depth
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
		/// Iterate over nodes with a callback action and parent + depth parameters
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node with parent + depth awareness</param>
		public virtual void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, Action<TNode, TNode, int> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback with action + parent + depth
		public void Iterate(TNode root, [NotNull] Action<TNode, TNode, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Action<TNode, TNode, int> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(TNode root, TraverseMethod method, [NotNull] Action<TNode, TNode, int> visitCallback)
		{
			Iterate(root, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function and parent + depth parameters
		/// </summary>
		/// <param name="root">The starting node</param>
		/// <param name="method">The traverse method <see cref="TraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node with parent and depth awareness that can cancel the loop</param>
		public virtual void Iterate(TNode root, TraverseMethod method, HorizontalFlow flow, [NotNull] Func<TNode, TNode, int, bool> visitCallback)
		{
			if (root == null) return;
			new Iterator(this, root, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback function + parent + depth
		public void Iterate(TNode root, [NotNull] Func<TNode, TNode, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(TNode root, HorizontalFlow flow, [NotNull] Func<TNode, TNode, int, bool> visitCallback)
		{
			Iterate(root, TraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(TNode root, TraverseMethod method, [NotNull] Func<TNode, TNode, int, bool> visitCallback)
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

		[NotNull]
		public abstract TNode NewNode(T value);

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
		}

		public virtual T Minimum()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			if (Root == null) return default(T);

			/*
			 * This tree might not be a valid binary search tree. So a traversal is need to search the entire tree.
			 * In the overriden method of the BinarySearchTree (and any similar type of tree), this implementation
			 * just grabs the root's left most node's value.
			 */
			T minimum = Root.Value;
			
			if (Root.Left != null)
			{
				Iterate(Root.Left, TraverseMethod.PreOrder, HorizontalFlow.LeftToRight, e =>
				{
					if (Comparer.IsLessThan(minimum, e.Value)) return;
					minimum = e.Value;
				});
			}
			
			if (Root.Right != null)
			{
				Iterate(Root.Right, TraverseMethod.PreOrder, HorizontalFlow.LeftToRight, e =>
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
			 * This tree might not be a valid binary search tree. So a traversal is need to search the entire tree.
			 * In the overriden method of the BinarySearchTree (and any similar type of tree), this implementation
			 * just grabs the root's right most node's value.
			 */
			T maximum = Root.Value;
			
			if (Root.Left != null)
			{
				Iterate(Root.Left, TraverseMethod.PreOrder, HorizontalFlow.LeftToRight, e =>
				{
					if (Comparer.IsGreaterThan(maximum, e.Value)) return;
					maximum = e.Value;
				});
			}
			
			if (Root.Right != null)
			{
				Iterate(Root.Right, TraverseMethod.PreOrder, HorizontalFlow.LeftToRight, e =>
				{
					if (Comparer.IsGreaterThan(maximum, e.Value)) return;
					maximum = e.Value;
				});
			}

			return maximum;
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

		public bool IsBalanced() { return IsBalanced(Root); }
		public abstract bool IsBalanced(TNode node);

		/// <summary>
		/// Balances the tree if it supports it.
		/// </summary>
		public abstract void Balance();

		public IReadOnlyCollection<TNode> GeNodesAtLevel(int level)
		{
			if (level < 0) throw new ArgumentOutOfRangeException(nameof(level));
			if (Root == null) return null;

			IReadOnlyCollection<TNode> collection = null;
			Iterate(Root, HorizontalFlow.LeftToRight, (lvl, nodes) =>
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
			TNode node = NewNode(list[index++]);
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
					node = NewNode(list[index]);
					root.Left = node;
					Count++;
					queue.Enqueue(node);
					index++;
				}

				// add right node
				if (index < list.Count && comparer.IsGreaterThanOrEqual(list[index], root.Value))
				{
					node = NewNode(list[index]);
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
			TNode node = NewNode(list[0]);
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
				while (stack.Count > 0 && comparer.IsGreaterThan(list[i], stack.Peek()))
					root = stack.Pop();

				node = NewNode(list[i]);

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
			TNode node = NewNode(list[index]);
			Root = node;
			Count++;

			Queue<(int Index, int Start, int End, TNode TNode)> queue = new Queue<(int Index, int Start, int End, TNode TNode)>();
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
					node = NewNode(list[nodeIndex]);
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
					node = NewNode(list[nodeIndex]);
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
			TNode node = NewNode(list[list.Count - 1]);
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
				while (stack.Count > 0 && comparer.IsLessThan(list[i], stack.Peek()))
					root = stack.Pop();

				node = NewNode(list[i]);

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
			TNode node = NewNode(levelOrder[index++]);
			Root = node;
			Count++;

			Queue<(int Start, int End, TNode TNode)> queue = new Queue<(int Start, int End, TNode TNode)>();
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
					node = NewNode(inOrder[levelIndex]);
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
					node = NewNode(inOrder[levelIndex]);
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
			TNode node = NewNode(preOrder[preIndex++]);
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
					node = NewNode(preOrder[preIndex++]);
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
					node = NewNode(preOrder[preIndex++]);
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
			TNode node = NewNode(postOrder[postIndex--]);
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
					node = NewNode(inOrder[index]);
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

					node = NewNode(inOrder[index]);
					root.Left = node;
					Count++;
					stack.Push(node);
					postIndex--;
				}
			}
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
					Root = NewNode(list[0]);
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
	[Serializable]
	public abstract class LinkedBinaryTree<T> : LinkedBinaryTree<LinkedBinaryNode<T>, T>
	{
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

		/// <inheritdoc />
		public override LinkedBinaryNode<T> NewNode(T value) { return new LinkedBinaryNode<T>(value); }

		/// <inheritdoc />
		public override int GetHeight() { return Root?.Height ?? 0; }

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
		protected LinkedBinaryNode<T> RotateLeft([NotNull] LinkedBinaryNode<T> node /* x */)
		{
			LinkedBinaryNode<T> newRoot /* y */ = node.Right;
			LinkedBinaryNode<T> oldLeft /* T2 */ = newRoot.Left;

			// Perform rotation
			newRoot.Left = node;
			node.Right = oldLeft;

			// update nodes
			SetHeight(node);
			SetHeight(newRoot);

			_version++;
			// Return new root
			return newRoot;
		}

		[NotNull]
		protected LinkedBinaryNode<T> RotateRight([NotNull] LinkedBinaryNode<T> node /* y */)
		{
			LinkedBinaryNode<T> newRoot /* x */ = node.Left;
			LinkedBinaryNode<T> oldRight /* T2 */ = newRoot.Right;

			// Perform rotation
			newRoot.Right = node;
			node.Left = oldRight;

			// update nodes
			SetHeight(node);
			SetHeight(newRoot);

			_version++;
			// Return new root
			return newRoot;
		}

		protected void SetHeight([NotNull] LinkedBinaryNode<T> node)
		{
			node.Height = 1 + Math.Max(node.Left?.Height ?? -1, node.Right?.Height ?? -1);
		}
	}

	public static class LinkedBinaryTreeExtension
	{
		public static string ToString<TNode, T>([NotNull] this LinkedBinaryTree<TNode, T> thisValue, Orientation orientation, bool diagnosticInfo = false)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			if (thisValue.Root == null) return string.Empty;
			return orientation switch
			{
				Orientation.Horizontal => Horizontally(thisValue, diagnosticInfo),
				Orientation.Vertical => Vertically(thisValue, diagnosticInfo),
				_ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
			};

			static string Horizontally(LinkedBinaryTree<TNode, T> tree, bool diagnostic)
			{
				const string STR_BLANK = "    ";
				const string STR_EXT = "│   ";
				const string STR_CONNECTOR = "─── ";
				const string STR_CONNECTOR_R = "└── ";
				const string STR_CONNECTOR_L = "┌── ";

				/*
				 * Will use a little bit of a strange structure and InOrder traversal.
				 * For each node, a list of all its parent is needed up to the root.
				 * Because BinarySearchTree (or AVLTree) don't usually store a parent
				 * node pointer, and apparently it's needed only in this situation, will
				 * store this list unless a better alternative is found.
				 */
				StringBuilder sb = new StringBuilder();
				Stack<string> connectors = new Stack<string>();

				// Left-Root-Right (Stack)
				Stack<(TNode Node, IList<TNode> Parents)> stack = new Stack<(TNode Node, IList<TNode> Parents)>();
				int version = tree._version;
				// Start at the root
				(TNode Node, IList<TNode> Parents) current = (tree.Root, null);

				while (current.Node != null || stack.Count > 0)
				{
					if (version != tree._version) throw new VersionChangedException();

					if (current.Node != null)
					{
						stack.Push(current);

						// Navigate left
						if (current.Node.Left != null)
						{
							IList<TNode> parents = new List<TNode>(current.Parents ?? Enumerable.Empty<TNode>())
							{
								current.Node
							};
							current = (current.Node.Left, parents);
						}
						else
						{
							current = (null, null);
						}
					}
					else
					{
						// visit the next queued node
						current = stack.Pop();
						connectors.Push(current.Node.ToString(current.Parents?.Count ?? 0, diagnostic));

						TNode parent = null;
						if (current.Parents != null && current.Parents.Count > 0) parent = current.Parents[current.Parents.Count - 1];

						BinaryNodeType nodeType = current.Node.NodeType(parent);

						switch (nodeType)
						{
							case BinaryNodeType.Root:
								connectors.Push(STR_CONNECTOR);
								break;
							case BinaryNodeType.Left:
								connectors.Push(STR_CONNECTOR_L);
								break;
							case BinaryNodeType.Right:
								connectors.Push(STR_CONNECTOR_R);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						if (current.Parents != null)
						{
							IList<TNode> parents = current.Parents;
							TNode node = current.Node;

							for (int i = parents.Count - 1; i >= 0; i--)
							{
								parent = parents[i];
								TNode grandParent = i > 0
														? parents[i - 1]
														: null;
								nodeType = node.NodeType(parent);
								BinaryNodeType parentNodeType = grandParent == null
																	? BinaryNodeType.Root
																	: parent.NodeType(grandParent);
								
								/*
								 * if (node is left and its parent is right) or (node is right and its parent is left), add STR_EXT
								 */
								if (nodeType == BinaryNodeType.Left && parentNodeType == BinaryNodeType.Right ||
									nodeType == BinaryNodeType.Right && parentNodeType == BinaryNodeType.Left)
								{
									connectors.Push(STR_EXT);
								}
								else
								{
									connectors.Push(STR_BLANK);
								}

								node = parent;
							}
						}

						while (connectors.Count > 1) 
							sb.Append(connectors.Pop());

						sb.AppendLine(connectors.Pop());

						// Navigate right
						if (current.Node.Right != null)
						{
							IList<TNode> parents = new List<TNode>(current.Parents ?? Enumerable.Empty<TNode>())
							{
								current.Node
							};
							current = (current.Node.Right, parents);
						}
						else
						{
							current = (null, null);
						}
					}
				}

				return sb.ToString();
			}

			static string Vertically(LinkedBinaryTree<TNode, T> tree, bool diagnostic)
			{
				const char C_BLANK = ' ';
				const char C_EXT = '─';
				const char C_CONNECTOR_L = '┌';
				const char C_CONNECTOR_R = '┐';

				int distance = 0;
				IDictionary<int, StringBuilder> lines = new Dictionary<int, StringBuilder>();
				tree.Iterate(tree.Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, (node, parent, depth) =>
				{
					StringBuilder line = lines.GetOrAdd(depth);

					if (line.Length > 0 && line[line.Length - 1] == C_CONNECTOR_L) line.Append(C_EXT, distance - line.Length);
					else line.Append(C_BLANK, distance - line.Length);

					if (depth > 0)
					{
						StringBuilder prevLine = lines.GetOrAdd(depth - 1);

						if (ReferenceEquals(parent.Left /* parent is guaranteed not to be null because depth > 0 */, node))
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
					line.Append(node.ToString(depth, diagnostic));
					distance = line.Length;
				});

				return string.Join(Environment.NewLine, lines.OrderBy(e => e.Key).Select(e => e.Value));
			}
		}
	}
}
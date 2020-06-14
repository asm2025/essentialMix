using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using asm.Exceptions.Collections;
using asm.Extensions;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
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
	[Serializable]
	public abstract class ArrayBinaryTree<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
	{
		/// <summary>
		/// a semi recursive approach to traverse the tree
		/// </summary>
		public sealed class Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _version;
			private readonly int _index;
			private readonly DynamicQueue<int> _queueOrStack;
			private readonly Func<bool> _moveNext;

			private ArrayBinaryNode<T> _current;
			private bool _started;
			private bool _done;

			internal Enumerator([NotNull] ArrayBinaryTree<T> tree, int index, BinaryTreeTraverseMethod method, HorizontalFlow flow)
			{
				_tree = tree;
				_version = _tree._version;
				_index = index;
				_current = new ArrayBinaryNode<T>(tree);
				_done = _tree.Count == 0 || !_index.InRangeRx(0, _tree.Count);

				switch (method)
				{
					case BinaryTreeTraverseMethod.LevelOrder:
						_queueOrStack = new DynamicQueue<int>(DequeuePriority.FIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => LevelOrderLR,
							HorizontalFlow.RightToLeft => LevelOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case BinaryTreeTraverseMethod.PreOrder:
						_queueOrStack = new DynamicQueue<int>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => PreOrderLR,
							HorizontalFlow.RightToLeft => PreOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case BinaryTreeTraverseMethod.InOrder:
						_queueOrStack = new DynamicQueue<int>(DequeuePriority.LIFO);
						_moveNext = flow switch
						{
							HorizontalFlow.LeftToRight => InOrderLR,
							HorizontalFlow.RightToLeft => InOrderRL,
							_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
						};
						break;
					case BinaryTreeTraverseMethod.PostOrder:
						_queueOrStack = new DynamicQueue<int>(DequeuePriority.LIFO);
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
					if (_current.Index < 0) throw new InvalidOperationException();
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
				if (_version != _tree._version) throw new VersionChangedException();
				_current.Index = -1;
				_started = false;
				_queueOrStack.Clear();
				_done = _tree.Count == 0 || !_index.InRangeRx(0, _tree.Count);
			}

			/// <inheritdoc />
			public void Dispose()
			{
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
					_queueOrStack.Enqueue(_index);
				}

				// visit the next queued node
				_current.Index = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: -1;

				if (_current.Index < 0)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.LeftIndex > -1) _queueOrStack.Enqueue(_current.LeftIndex);
				if (_current.RightIndex > -1) _queueOrStack.Enqueue(_current.RightIndex);
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
					_queueOrStack.Enqueue(_index);
				}

				// visit the next queued node
				_current.Index = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: -1;

				if (_current.Index < 0)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.RightIndex > -1) _queueOrStack.Enqueue(_current.RightIndex);
				if (_current.LeftIndex > -1) _queueOrStack.Enqueue(_current.LeftIndex);
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
					_queueOrStack.Enqueue(_index);
				}

				// visit the next queued node
				_current.Index = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: -1;

				if (_current.Index < 0)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.RightIndex > -1) _queueOrStack.Enqueue(_current.RightIndex);
				if (_current.LeftIndex > -1) _queueOrStack.Enqueue(_current.LeftIndex);
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
					_queueOrStack.Enqueue(_index);
				}

				// visit the next queued node
				_current.Index = _queueOrStack.Count > 0
								? _queueOrStack.Dequeue()
								: -1;

				if (_current.Index < 0)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.LeftIndex > -1) _queueOrStack.Enqueue(_current.LeftIndex);
				if (_current.RightIndex > -1) _queueOrStack.Enqueue(_current.RightIndex);
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
					_current.Index = _index;
				}
				else
				{
					// Navigate right
					_current.Index = _current.RightIndex;
				}

				while (_current.Index > -1 || _queueOrStack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current.Index > -1)
					{
						_queueOrStack.Enqueue(_current.Index);
						// Navigate left
						_current.Index = _current.LeftIndex;
					}
					else
					{
						// visit the next queued node
						_current.Index = _queueOrStack.Dequeue();
						break; // break from the loop to visit this node
					}
				}

				_done = _current.Index < 0;
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
					_current.Index = _index;
				}
				else
				{
					// Navigate left
					_current.Index = _current.LeftIndex;
				}

				while (_current.Index > -1 || _queueOrStack.Count > 0)
				{
					if (_version != _tree._version) throw new VersionChangedException();

					if (_current.Index > -1)
					{
						_queueOrStack.Enqueue(_current.Index);
						// Navigate right
						_current.Index = _current.RightIndex;
					}
					else
					{
						// visit the next queued node
						_current.Index = _queueOrStack.Dequeue();
						break; // break from the loop to visit this node
					}
				}

				_done = _current.Index < 0;
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

						// Navigate right
						if (_current.RightIndex > -1) _queueOrStack.Enqueue(_current.RightIndex);

						_queueOrStack.Enqueue(_current.Index);

						// Navigate left
						_current.Index = _current.LeftIndex;
					}

					if (_version != _tree._version) throw new VersionChangedException();

					_current.Index = _queueOrStack.Count > 0
									? _queueOrStack.Dequeue()
									: -1;

					if (_current.Index < 0) continue;

					/*
					* if Current has a right child and is not processed yet,
					* then make sure right child is processed before root
					*/
					if (_current.RightIndex > -1 && _queueOrStack.Count > 0 && _current.RightIndex == _queueOrStack.Peek())
					{
						// remove right child from stack
						_queueOrStack.Dequeue();
						// push Current back to stack
						_queueOrStack.Enqueue(_current.Index);
						// process right first
						_current.Index = _current.RightIndex;
						continue;
					}
		
					if (_current.Index > -1)
						break; // break from the loop to visit this node
				} while (_queueOrStack.Count > 0);

				_done = _current.Index < 0;
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

						// Navigate left
						if (_current.LeftIndex > -1) _queueOrStack.Enqueue(_current.LeftIndex);

						_queueOrStack.Enqueue(_current.Index);

						// Navigate right
						_current.Index = _current.RightIndex;
					}

					if (_version != _tree._version) throw new VersionChangedException();
					_current.Index = _queueOrStack.Count > 0
									? _queueOrStack.Dequeue()
									: -1;
					if (_current.Index < 0) continue;

					/*
					* if Current has a left child and is not processed yet,
					* then make sure left child is processed before root
					*/
					if (_current.LeftIndex > -1 && _queueOrStack.Count > 0 && _current.LeftIndex == _queueOrStack.Peek())
					{
						// remove left child from stack
						_queueOrStack.Dequeue();
						// push Current back to stack
						_queueOrStack.Enqueue(_current.Index);
						// process left first
						_current.Index = _current.LeftIndex;
						continue;
					}

					if (_current.Index > -1)
						break; // break from the loop to visit this node
				} while (_queueOrStack.Count > 0);

				_done = _current.Index < 0;
				return !_done;
			}
		}

		/// <summary>
		/// iterative approach to traverse the tree
		/// </summary>
		public sealed class Iterator
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _index;
			private readonly BinaryTreeTraverseMethod _method;

			internal Iterator([NotNull] ArrayBinaryTree<T> tree, int index, BinaryTreeTraverseMethod method)
			{
				_tree = tree;
				_index = index;
				_method = method;
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<int> visitCallback)
			{
				if (!_index.InRangeRx(0, _tree.Count)) return;

				int version = _tree._version;

				switch (_method)
				{
					case BinaryTreeTraverseMethod.LevelOrder:
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
					case BinaryTreeTraverseMethod.PreOrder:
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
					case BinaryTreeTraverseMethod.InOrder:
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
					case BinaryTreeTraverseMethod.PostOrder:
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
					Queue<int> queue = new Queue<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue(current.Index);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = queue.Dequeue();
						visitCallback(current.Index);

						// Queue the next nodes
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<int> queue = new Queue<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue(current.Index);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = queue.Dequeue();
						visitCallback(current.Index);

						// Queue the next nodes
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<int> stack = new Stack<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push(current.Index);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = stack.Pop();
						visitCallback(current.Index);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.RightIndex > -1) stack.Push(current.RightIndex);
						if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<int> stack = new Stack<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push(current.Index);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = stack.Pop();
						visitCallback(current.Index);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
						if (current.RightIndex > -1) stack.Push(current.RightIndex);
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate left
							current.Index = current.LeftIndex;
						}
						else
						{
							// visit the next queued node
							current.Index = stack.Pop();
							visitCallback(current.Index);

							// Navigate right
							current.Index = current.RightIndex;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate right
							current.Index = current.RightIndex;
						}
						else
						{
							// visit the next queued node
							current.Index = stack.Pop();
							visitCallback(current.Index);

							// Navigate right
							current.Index = current.LeftIndex;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int lastVisited = -1;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate left
							current.Index = current.LeftIndex;
							continue;
						}

						int peek = stack.Peek();
						int right = _tree.RightIndex(peek);
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right node?
						 * if yes, then navigate right.
						 */
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

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int lastVisited = -1;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate right
							current.Index = current.RightIndex;
							continue;
						}

						int peek = stack.Peek();
						int left = _tree.LeftIndex(peek);
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left node?
						 * if yes, then navigate left.
						 */
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
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<int, bool> visitCallback)
			{
				if (!_index.InRangeRx(0, _tree.Count)) return;

				int version = _tree._version;

				switch (_method)
				{
					case BinaryTreeTraverseMethod.LevelOrder:
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
					case BinaryTreeTraverseMethod.PreOrder:
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
					case BinaryTreeTraverseMethod.InOrder:
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
					case BinaryTreeTraverseMethod.PostOrder:
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
					Queue<int> queue = new Queue<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue(current.Index);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = queue.Dequeue();
						if (!visitCallback(current.Index)) break;

						// Queue the next nodes
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<int> queue = new Queue<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue(current.Index);

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = queue.Dequeue();
						if (!visitCallback(current.Index)) break;

						// Queue the next nodes
						if (current.RightIndex > -1) queue.Enqueue(current.RightIndex);
						if (current.LeftIndex > -1) queue.Enqueue(current.LeftIndex);
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<int> stack = new Stack<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push(current.Index);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = stack.Pop();
						if (!visitCallback(current.Index)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.RightIndex > -1) stack.Push(current.RightIndex);
						if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<int> stack = new Stack<int>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push(current.Index);

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						current.Index = stack.Pop();
						if (!visitCallback(current.Index)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.LeftIndex > -1) stack.Push(current.LeftIndex);
						if (current.RightIndex > -1) stack.Push(current.RightIndex);
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate left
							current.Index = current.LeftIndex;
						}
						else
						{
							// visit the next queued node
							current.Index = stack.Pop();
							if (!visitCallback(current.Index)) break;

							// Navigate right
							current.Index = current.RightIndex;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate right
							current.Index = current.RightIndex;
						}
						else
						{
							// visit the next queued node
							current.Index = stack.Pop();
							if (!visitCallback(current.Index)) break;

							// Navigate right
							current.Index = current.LeftIndex;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int lastVisited = -1;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate left
							current.Index = current.LeftIndex;
							continue;
						}

						int peek = stack.Peek();
						int right = _tree.RightIndex(peek);
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right node?
						 * if yes, then navigate right.
						 */
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

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<int> stack = new Stack<int>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int lastVisited = -1;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push(current.Index);
							// Navigate right
							current.Index = current.RightIndex;
							continue;
						}

						int peek = stack.Peek();
						int left = _tree.LeftIndex(peek);
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left node?
						 * if yes, then navigate left.
						 */
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
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<int, int> visitCallback)
			{
				if (!_index.InRangeRx(0, _tree.Count)) return;

				int version = _tree._version;

				switch (_method)
				{
					case BinaryTreeTraverseMethod.LevelOrder:
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
					case BinaryTreeTraverseMethod.PreOrder:
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
					case BinaryTreeTraverseMethod.InOrder:
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
					case BinaryTreeTraverseMethod.PostOrder:
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
					Queue<(int Depth, int Index)> queue = new Queue<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue((0, current.Index));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = queue.Dequeue();
						visitCallback(current.Index, depth);

						// Queue the next nodes
						if (current.LeftIndex > -1) queue.Enqueue((depth + 1, current.LeftIndex));
						if (current.RightIndex > -1) queue.Enqueue((depth + 1, current.RightIndex));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, int Index)> queue = new Queue<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue((0, current.Index));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = queue.Dequeue();
						visitCallback(current.Index, depth);

						// Queue the next nodes
						if (current.RightIndex > -1) queue.Enqueue((depth + 1, current.RightIndex));
						if (current.LeftIndex > -1) queue.Enqueue((depth + 1, current.LeftIndex));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push((0, current.Index));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = stack.Pop();
						visitCallback(current.Index, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.RightIndex > -1) stack.Push((depth + 1, current.RightIndex));
						if (current.LeftIndex > -1) stack.Push((depth + 1, current.LeftIndex));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push((0, current.Index));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = stack.Pop();
						visitCallback(current.Index, depth);

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.LeftIndex > -1) stack.Push((depth + 1, current.LeftIndex));
						if (current.RightIndex > -1) stack.Push((depth + 1, current.RightIndex));
					}
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate left
							current.Index = current.LeftIndex;
							if (current.Index > -1) depth++;
						}
						else
						{
							// visit the next queued node
							(depth, current.Index) = stack.Pop();
							visitCallback(current.Index, depth);

							// Navigate right
							current.Index = current.RightIndex;
							if (current.Index > -1) depth++;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate right
							current.Index = current.RightIndex;
							if (current.Index > -1) depth++;
						}
						else
						{
							// visit the next queued node
							(depth, current.Index) = stack.Pop();
							visitCallback(current.Index, depth);

							// Navigate left
							current.Index = current.LeftIndex;
							if (current.Index > -1) depth++;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int lastVisited = -1;
					int depth = 0;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate left
							current.Index = current.LeftIndex;
							if (current.Index > -1) depth++;
							continue;
						}

						(int Depth, int Index) peek = stack.Peek();
						int right = _tree.RightIndex(peek.Index);
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right ArrayBinaryNode<T>
						 * if yes, then navigate right.
						 */
						if (right > -1 && lastVisited != right)
						{
							// Navigate right
							current.Index = right;
							depth = peek.Depth + 1;
						}
						else
						{
							// visit the next queued node
							current.Index = peek.Index;
							(depth, lastVisited) = stack.Pop();
							visitCallback(current.Index, depth);
							current.Index = -1;
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;
					int lastVisited = -1;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate right
							current.Index = current.RightIndex;
							if (current.Index > -1) depth++;
							continue;
						}

						(int Depth, int Index) peek = stack.Peek();
						int left = _tree.LeftIndex(peek.Index);
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left ArrayBinaryNode<T>
						 * if yes, then navigate left.
						 */
						if (left > -1 && lastVisited != left)
						{
							// Navigate left
							current.Index = left;
							depth = peek.Depth + 1;
						}
						else
						{
							// visit the next queued node
							current.Index = peek.Index;
							(depth, lastVisited) = stack.Pop();
							visitCallback(current.Index, depth);
							current.Index = -1;
						}
					}
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<int, int, bool> visitCallback)
			{
				if (!_index.InRangeRx(0, _tree.Count)) return;

				int version = _tree._version;

				switch (_method)
				{
					case BinaryTreeTraverseMethod.LevelOrder:
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
					case BinaryTreeTraverseMethod.PreOrder:
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
					case BinaryTreeTraverseMethod.InOrder:
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
					case BinaryTreeTraverseMethod.PostOrder:
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
					Queue<(int Depth, int Index)> queue = new Queue<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue((0, current.Index));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = queue.Dequeue();
						if (!visitCallback(current.Index, depth)) break;

						// Queue the next nodes
						if (current.LeftIndex > -1) queue.Enqueue((depth + 1, current.LeftIndex));
						if (current.RightIndex > -1) queue.Enqueue((depth + 1, current.RightIndex));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, int Index)> queue = new Queue<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					queue.Enqueue((0, current.Index));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = queue.Dequeue();
						if (!visitCallback(current.Index, depth)) break;

						// Queue the next nodes
						if (current.RightIndex > -1) queue.Enqueue((depth + 1, current.RightIndex));
						if (current.LeftIndex > -1) queue.Enqueue((depth + 1, current.LeftIndex));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);

					// Start at the root
					stack.Push((0, current.Index));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						int depth;
						(depth, current.Index) = stack.Pop();
						if (!visitCallback(current.Index, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.RightIndex > -1) stack.Push((depth + 1, current.RightIndex));
						if (current.LeftIndex > -1) stack.Push((depth + 1, current.LeftIndex));
					}
				}

				void PreOrderRL()
				{
					// Root-Right-Left (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;

					// Start at the root
					stack.Push((depth, current.Index));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(depth, current.Index) = stack.Pop();
						if (!visitCallback(current.Index, depth)) break;

						/*
						* The stack works backwards (LIFO).
						* It means whatever we want to
						* appear first, we must add last.
						*/
						// Queue the next nodes
						if (current.LeftIndex > -1) stack.Push((depth + 1, current.LeftIndex));
						if (current.RightIndex > -1) stack.Push((depth + 1, current.RightIndex));
					}		
				}

				void InOrderLR()
				{
					// Left-Root-Right (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate left
							current.Index = current.LeftIndex;
							if (current.Index > -1) depth++;
						}
						else
						{
							// visit the next queued node
							(depth, current.Index) = stack.Pop();
							if (!visitCallback(current.Index, depth)) break;

							// Navigate right
							current.Index = current.RightIndex;
							if (current.Index > -1) depth++;
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate right
							current.Index = current.RightIndex;
							if (current.Index > -1) depth++;
						}
						else
						{
							// visit the next queued node
							(depth, current.Index) = stack.Pop();
							if (!visitCallback(current.Index, depth)) break;

							// Navigate left
							current.Index = current.LeftIndex;
							if (current.Index > -1) depth++;
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int lastVisited = -1;
					int depth = 0;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate left
							current.Index = current.LeftIndex;
							if (current.Index > -1) depth++;
							continue;
						}

						(int Depth, int Index) peek = stack.Peek();
						int right = _tree.RightIndex(peek.Index);
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right ArrayBinaryNode<T>
						 * if yes, then navigate right.
						 */
						if (right > -1 && lastVisited != right)
						{
							// Navigate right
							current.Index = right;
							depth = peek.Depth + 1;
						}
						else
						{
							// visit the next queued node
							current.Index = peek.Index;
							(depth, lastVisited) = stack.Pop();
							if (!visitCallback(current.Index, depth)) break;
							current.Index = -1;
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<(int Depth, int Index)> stack = new Stack<(int Depth, int Index)>();
					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					int depth = 0;
					int lastVisited = -1;

					while (current.Index > -1 || stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						if (current.Index > -1)
						{
							stack.Push((depth, current.Index));
							// Navigate right
							current.Index = current.RightIndex;
							if (current.Index > -1) depth++;
							continue;
						}

						(int Depth, int Index) peek = stack.Peek();
						int left = _tree.LeftIndex(peek.Index);
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left ArrayBinaryNode<T>
						 * if yes, then navigate left.
						 */
						if (left > -1 && lastVisited != left)
						{
							// Navigate left
							current.Index = left;
							depth = peek.Depth + 1;
						}
						else
						{
							// visit the next queued node
							current.Index = peek.Index;
							(depth, lastVisited) = stack.Pop();
							if (!visitCallback(current.Index, depth)) break;
							current.Index = -1;
						}
					}
				}
			}
		}

		/// <summary>
		/// iterative approach with level awareness. This is a different way than <see cref="BinaryTreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		public sealed class LevelIterator
		{
			private readonly ArrayBinaryTree<T> _tree;
			private readonly int _index;
			private readonly Queue<int> _queue = new Queue<int>();

			private int _level = -1;

			internal LevelIterator([NotNull] ArrayBinaryTree<T> tree, int index)
			{
				_tree = tree;
				_index = index;
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
			{
				if (!_index.InRangeRx(0, _tree.Count)) return;

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
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					_queue.Enqueue(current.Index);
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
							current.Index = _queue.Dequeue();

							// Queue the next nodes
							if (current.LeftIndex > -1) _queue.Enqueue(current.LeftIndex);
							if (current.RightIndex > -1) _queue.Enqueue(current.RightIndex);
						}
					}

					_level = -1;
				}

				void IterateRL()
				{
					// Root-Right-Left (Queue)
					_queue.Clear();

					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					_queue.Enqueue(current.Index);
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
							current.Index = _queue.Dequeue();

							// Queue the next nodes
							if (current.RightIndex > -1) _queue.Enqueue(current.RightIndex);
							if (current.LeftIndex > -1) _queue.Enqueue(current.LeftIndex);
						}
					}

					_level = -1;
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
			{
				if (!_index.InRangeRx(0, _tree.Count)) return;

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
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					_queue.Enqueue(current.Index);
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
							current.Index = _queue.Dequeue();

							// Queue the next nodes
							if (current.LeftIndex > -1) _queue.Enqueue(current.LeftIndex);
							if (current.RightIndex > -1) _queue.Enqueue(current.RightIndex);
						}
					}

					_level = -1;
				}

				void IterateRL()
				{
					// Root-Right-Left (Queue)
					_queue.Clear();

					// Start at the root
					ArrayBinaryNode<T> current = new ArrayBinaryNode<T>(_tree, _index);
					_queue.Enqueue(current.Index);
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
							current.Index = _queue.Dequeue();

							// Queue the next nodes
							if (current.RightIndex > -1) _queue.Enqueue(current.RightIndex);
							if (current.LeftIndex > -1) _queue.Enqueue(current.LeftIndex);
						}
					}

					_level = -1;
				}
			}
		}

		protected internal int _version;
		
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
		public IComparer<T> Comparer { get; private set; }

		public string Label { get; set; }

		[field: ContractPublicPropertyName("Count")]
		public int Count { get; protected set; }

		[NotNull]
		protected internal T[] Items { get; private set; }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

		[NotNull]
		public IEnumerator<T> GetEnumerator()
		{
			return Enumerate(0, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight);
		}

		public virtual bool Equals(ArrayBinaryTree<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Count != other.Count || !Comparer.Equals(other.Comparer)) return false;

			for (int i = 0; i < Count; i++)
			{
				if (Comparer.IsEqual(Items[i], other.Items[i])) continue;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Enumerate nodes' values in a semi recursive approach
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <returns></returns>
		[NotNull]
		public Enumerator Enumerate(int index, BinaryTreeTraverseMethod method, HorizontalFlow flow)
		{
			return new Enumerator(this, index, method, flow);
		}

		#region Enumerate overloads
		[NotNull]
		public Enumerator Enumerate(int index)
		{
			return Enumerate(index, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight);
		}

		[NotNull]
		public Enumerator Enumerate(int index, HorizontalFlow flow)
		{
			return Enumerate(index, BinaryTreeTraverseMethod.InOrder, flow);
		}

		[NotNull]
		public Enumerator Enumerate(int index, BinaryTreeTraverseMethod method)
		{
			return Enumerate(index, method, HorizontalFlow.LeftToRight);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method <see cref="BinaryTreeTraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate(int index, BinaryTreeTraverseMethod method, HorizontalFlow flow, [NotNull] Action<int> visitCallback)
		{
			if (index < 0) return;
			new Iterator(this, index, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback action
		public void Iterate(int index, [NotNull] Action<int> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(int index, HorizontalFlow flow, [NotNull] Action<int> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(int index, BinaryTreeTraverseMethod method, [NotNull] Action<int> visitCallback)
		{
			Iterate(index, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method <see cref="BinaryTreeTraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate(int index, BinaryTreeTraverseMethod method, HorizontalFlow flow, [NotNull] Func<int, bool> visitCallback)
		{
			if (index < 0) return;
			new Iterator(this, index, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback function
		public void Iterate(int index, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(int index, HorizontalFlow flow, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(int index, BinaryTreeTraverseMethod method, [NotNull] Func<int, bool> visitCallback)
		{
			Iterate(index, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback action and depth parameter
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method <see cref="BinaryTreeTraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback action to handle the node with depth awareness</param>
		public void Iterate(int index, BinaryTreeTraverseMethod method, HorizontalFlow flow, [NotNull] Action<int, int> visitCallback)
		{
			if (index < 0) return;
			new Iterator(this, index, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback with action + depth
		public void Iterate(int index, [NotNull] Action<int, int> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(int index, HorizontalFlow flow, [NotNull] Action<int, int> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(int index, BinaryTreeTraverseMethod method, [NotNull] Action<int, int> visitCallback)
		{
			Iterate(index, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes with a callback function and depth parameter
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="method">The traverse method <see cref="BinaryTreeTraverseMethod"/></param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="visitCallback">callback function to handle the node with depth awareness that can cancel the loop</param>
		public void Iterate(int index, BinaryTreeTraverseMethod method, HorizontalFlow flow, [NotNull] Func<int, int, bool> visitCallback)
		{
			if (index < 0) return;
			new Iterator(this, index, method).Iterate(flow, visitCallback);
		}

		#region Iterate overloads - visitCallback function + depth
		public void Iterate(int index, [NotNull] Func<int, int, bool> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight, visitCallback);
		}

		public void Iterate(int index, HorizontalFlow flow, [NotNull] Func<int, int, bool> visitCallback)
		{
			Iterate(index, BinaryTreeTraverseMethod.InOrder, flow, visitCallback);
		}

		public void Iterate(int index, BinaryTreeTraverseMethod method, [NotNull] Func<int, int, bool> visitCallback)
		{
			Iterate(index, method, HorizontalFlow.LeftToRight, visitCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="BinaryTreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback function to handle the nodes of the level and can cancel the loop.</param>
		public void Iterate(int index, HorizontalFlow flow, [NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
		{
			if (index < 0) return;
			new LevelIterator(this, index).Iterate(flow, levelCallback);
		}

		#region LevelIterate overloads - visitCallback function
		public void Iterate(int index, [NotNull] Func<int, IReadOnlyCollection<int>, bool> levelCallback)
		{
			Iterate(index, HorizontalFlow.LeftToRight, levelCallback);
		}
		#endregion

		/// <summary>
		/// Iterate over nodes on a level by level basis with a callback function.
		/// This is a different way than <see cref="BinaryTreeTraverseMethod.LevelOrder"/> in that each level's nodes are brought as a collection.
		/// </summary>
		/// <param name="index">The starting node</param>
		/// <param name="flow">Left-to-right or right-to-left</param>
		/// <param name="levelCallback">callback action to handle the nodes of the level.</param>
		public void Iterate(int index, HorizontalFlow flow, [NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
		{
			if (index < 0) return;
			new LevelIterator(this, index).Iterate(flow, levelCallback);
		}

		#region LevelIterate overloads - visitCallback function
		public void Iterate(int index, [NotNull] Action<int, IReadOnlyCollection<int>> levelCallback)
		{
			Iterate(index, HorizontalFlow.LeftToRight, levelCallback);
		}
		#endregion

		public ArrayBinaryNode<T> NewNode(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			return new ArrayBinaryNode<T>(this, index);
		}

		public bool Contains(T value)
		{
			return Count > 0 && Array.FindIndex(Items, e => Comparer.IsEqual(e, value)) > -1;
		}

		public bool Exists([NotNull] Predicate<T> match) { return Count > 0 && Array.FindIndex(Items, match) != -1; }

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter)
		{
			for (int i = 0; i < Count; i++)
			{
				yield return converter(Items[i]);
			}
		}

		public virtual T Minimum()
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

		public virtual T Maximum()
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
			Iterate(0, HorizontalFlow.LeftToRight, (lvl, indexes) =>
			{
				if (lvl < level) return true;
				if (lvl == level) collection = indexes;
				return false;
			});
			return collection;
		}

		public void CopyTo([NotNull] T[] array) { CopyTo(array, 0); }
		public void CopyTo([NotNull] T[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
		public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
		{
			Count.ValidateRange(arrayIndex, ref count);
			array.Length.ValidateRange(arrayIndex, ref count);
			// Delegate rest of error checking to Array.Copy.
			Array.Copy(Items, 0, array, arrayIndex, count);
		}

		[NotNull]
		public T[] ToArray(BinaryTreeTraverseMethod method = BinaryTreeTraverseMethod.InOrder, HorizontalFlow flow = HorizontalFlow.LeftToRight)
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
					Iterate(0, method, flow, e => array[index++] = Items[e]);
					return array;
			}
		}

		public IEnumerable<T> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			int last = index + count;

			for (int i = index; i < last; i++)
			{
				yield return Items[i];
			}
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

		public void TrimExcess()
		{
			int threshold = (int)(Items.Length * 0.9);
			if (Count >= threshold) return;
			Capacity = Count;
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
						: (index - (1 + index.F())) / 2);
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

		//	/*
		//	* https://www.geeksforgeeks.org/avl-tree-set-1-insertion/
		//	*     y                               x
		//	*    / \     Right Rotation(y) ->    /  \
		//	*   x   T3                          T1   y 
		//	*  / \                                  / \
		//	* T1  T2     <- Left Rotation(x)       T2  T3
		//	*
		//	* A reference to the drawing only, not the code
		//	*/
		//	[NotNull]
		//	protected LinkedBinaryNode<T> RotateLeft([NotNull] LinkedBinaryNode<T> node /* x */)
		//	{
		//		LinkedBinaryNode<T> newRoot /* y */ = node.Right;
		//		LinkedBinaryNode<T> oldLeft /* T2 */ = newRoot.Left;

		//		// Perform rotation
		//		newRoot.Left = node;
		//		node.Right = oldLeft;

		//		// update nodes
		//		SetHeight(node);
		//		SetHeight(newRoot);

		//		_version++;
		//		// Return new root
		//		return newRoot;
		//	}

		//	[NotNull]
		//	protected LinkedBinaryNode<T> RotateRight([NotNull] LinkedBinaryNode<T> node /* y */)
		//	{
		//		LinkedBinaryNode<T> newRoot /* x */ = node.Left;
		//		LinkedBinaryNode<T> oldRight /* T2 */ = newRoot.Right;

		//		// Perform rotation
		//		newRoot.Right = node;
		//		node.Left = oldRight;

		//		// update nodes
		//		SetHeight(node);
		//		SetHeight(newRoot);

		//		_version++;
		//		// Return new root
		//		return newRoot;
		//	}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private int NormalizeIndex(int value)
		{
			return !value.InRangeRx(0, Count)
						? -1
						: value;
		}
	}

	public static class ArrayBinaryTreeExtension
	{
		public static void WriteTo<T>([NotNull] this ArrayBinaryTree<T> thisValue, [NotNull] TextWriter writer, Orientation orientation, bool diagnosticInfo = false)
		{
			if (thisValue.Count == 0) return;

			switch (orientation)
			{
				case Orientation.Horizontal:
					Horizontally(thisValue, writer, diagnosticInfo);
					break;
				case Orientation.Vertical:
					Vertically(thisValue, writer, diagnosticInfo);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
			}

			static void Horizontally(ArrayBinaryTree<T> tree, TextWriter writer, bool diagnostic)
			{
				const string STR_BLANK = "    ";
				const string STR_EXT = "│   ";
				const string STR_CONNECTOR = "─── ";
				const string STR_CONNECTOR_L = "└── ";
				const string STR_CONNECTOR_R = "┌── ";

				Stack<string> connectors = new Stack<string>();
				ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(tree);
				tree.Iterate(0, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.RightToLeft, (e, depth) =>
				{
					node.Index = e;
					connectors.Push(diagnostic
										? node.ToString(depth)
										: node.ToString());

					if (node.IsRight) connectors.Push(STR_CONNECTOR_R);
					else if (node.IsLeft) connectors.Push(STR_CONNECTOR_L);
					else connectors.Push(STR_CONNECTOR);

					while (node.ParentIndex > -1)
					{
						if (node.IsLeft && node.ParentIsRight || node.IsRight && node.ParentIsLeft) connectors.Push(STR_EXT);
						else connectors.Push(STR_BLANK);

						node.Index = node.ParentIndex;
					}

					while (connectors.Count > 1)
						writer.Write(connectors.Pop());

					writer.WriteLine(connectors.Pop());
				});
			}

			static void Vertically(ArrayBinaryTree<T> tree, TextWriter writer, bool diagnostic)
			{
				const char C_BLANK = ' ';
				const char C_EXT = '─';
				const char C_CONNECTOR_L = '┌';
				const char C_CONNECTOR_R = '┐';

				int distance = 0;
				ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(tree);
				IDictionary<int, StringBuilder> lines = new Dictionary<int, StringBuilder>();
				tree.Iterate(0, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight, (e, depth) =>
				{
					StringBuilder line = lines.GetOrAdd(depth);
					node.Index = e;

					if (line.Length > 0 && line[line.Length - 1] == C_CONNECTOR_L) line.Append(C_EXT, distance - line.Length);
					else line.Append(C_BLANK, distance - line.Length);

					if (depth > 0)
					{
						StringBuilder prevLine = lines.GetOrAdd(depth - 1);

						if (node.IsLeft)
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
					line.Append(diagnostic
									? node.ToString(depth)
									: node.ToString());
					distance = line.Length;
				});

				foreach (StringBuilder sb in lines.OrderBy(e => e.Key)
												.Select(e => e.Value))
				{
					writer.WriteLine(sb.ToString());
				}
			}
		}
	}
}
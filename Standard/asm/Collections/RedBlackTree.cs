using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using asm.Exceptions.Collections;
using asm.Extensions;
using asm.Patterns.Collections;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Red%E2%80%93black_tree">RedBlackTree (RBT)</see> implementation using the linked representation.</para>
	/// </summary>
	/// <typeparam name="T">The element type of the tree</typeparam>
	[Serializable]
	public sealed class RedBlackTree<T> : LinkedBinaryTree<RedBlackNode<T>, T>
	{
		/// <summary>
		/// iterative approach to traverse the tree
		/// </summary>
		internal sealed class WithParentIterator
		{
			private readonly RedBlackTree<T> _tree;
			private readonly RedBlackNode<T> _root;
			private readonly TraverseMethod _method;

			internal WithParentIterator([NotNull] RedBlackTree<T> tree, [NotNull] RedBlackNode<T> root, TraverseMethod method)
			{
				_tree = tree;
				_root = root;
				_method = method;
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Action<RedBlackNode<T>, RedBlackNode<T>, int> visitCallback)
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
					Queue<(int Depth, RedBlackNode<T> Node)> queue = new Queue<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = queue.Dequeue();
						visitCallback(node, node.Parent, depth);

						// Queue the next nodes
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, RedBlackNode<T> Node)> queue = new Queue<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = queue.Dequeue();
						visitCallback(node, node.Parent, depth);

						// Queue the next nodes
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = stack.Pop();
						visitCallback(node, node.Parent, depth);

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
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = stack.Pop();
						visitCallback(node, node.Parent, depth);

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
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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
							visitCallback(current.Node, current.Node.Parent, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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
							visitCallback(current.Node, current.Node.Parent, current.Depth);

							// Navigate right
							current = (current.Depth + 1, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();
					RedBlackNode<T> lastVisited = null;
					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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

						(int Depth, RedBlackNode<T> Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right RedBlackNode<T>
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
							visitCallback(current.Node, current.Node.Parent, current.Depth);
							current = (-1, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();
					RedBlackNode<T> lastVisited = null;
					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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

						(int Depth, RedBlackNode<T> Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left RedBlackNode<T>
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
							visitCallback(current.Node, current.Node.Parent, current.Depth);
							current = (-1, null);
						}
					}
				}
			}

			public void Iterate(HorizontalFlow flow, [NotNull] Func<RedBlackNode<T>, RedBlackNode<T>, int, bool> visitCallback)
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
					Queue<(int Depth, RedBlackNode<T> Node)> queue = new Queue<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = queue.Dequeue();
						if (!visitCallback(node, node.Parent, depth)) break;

						// Queue the next nodes
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
					}
				}

				void LevelOrderRL()
				{
					// Root-Right-Left (Queue)
					Queue<(int Depth, RedBlackNode<T> Node)> queue = new Queue<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					queue.Enqueue((0, _root));

					while (queue.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = queue.Dequeue();
						if (!visitCallback(node, node.Parent, depth)) break;

						// Queue the next nodes
						if (node.Right != null) queue.Enqueue((depth + 1, node.Right));
						if (node.Left != null) queue.Enqueue((depth + 1, node.Left));
					}
				}

				void PreOrderLR()
				{
					// Root-Left-Right (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = stack.Pop();
						if (!visitCallback(node, node.Parent, depth)) break;

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
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					stack.Push((0, _root));

					while (stack.Count > 0)
					{
						if (version != _tree._version) throw new VersionChangedException();

						// visit the next queued node
						(int depth, RedBlackNode<T> node) = stack.Pop();
						if (!visitCallback(node, node.Parent, depth)) break;

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
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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
							if (!visitCallback(current.Node, current.Node.Parent, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.Node.Right);
						}
					}
				}

				void InOrderRL()
				{
					// Right-Root-Left (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();

					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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
							if (!visitCallback(current.Node, current.Node.Parent, current.Depth)) break;

							// Navigate right
							current = (current.Depth + 1, current.Node.Left);
						}
					}
				}

				void PostOrderLR()
				{
					// Left-Right-Root (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();
					RedBlackNode<T> lastVisited = null;
					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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

						(int Depth, RedBlackNode<T> Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the left branch.
						 * Is there a right RedBlackNode<T>
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
							if (!visitCallback(current.Node, current.Node.Parent, current.Depth)) break;
							current = (-1, null);
						}
					}
				}

				void PostOrderRL()
				{
					// Right-Left-Root (Stack)
					Stack<(int Depth, RedBlackNode<T> Node)> stack = new Stack<(int Depth, RedBlackNode<T> Node)>();
					RedBlackNode<T> lastVisited = null;
					// Start at the root
					(int Depth, RedBlackNode<T> Node) current = (0, _root);

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

						(int Depth, RedBlackNode<T> Node) peek = stack.Peek();
						/*
						 * At this point we are either coming from
						 * either the root node or the right branch.
						 * Is there a left RedBlackNode<T>
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
							if (!visitCallback(current.Node, current.Node.Parent, current.Depth)) break;
							current = (-1, null);
						}
					}
				}
			}
		}

		/// <inheritdoc />
		public RedBlackTree()
			: this(Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public RedBlackTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public RedBlackTree(T value, IComparer<T> comparer)
			: base(comparer)
		{
			Add(value);
		}

		public RedBlackTree([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		public RedBlackTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(comparer)
		{
			foreach (T value in collection) 
				Add(value);
		}

		/// <inheritdoc />
		internal RedBlackTree(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = true;

		/// <inheritdoc />
		public override RedBlackNode<T> NewNode(T value) { return new RedBlackNode<T>(value); }

		/// <inheritdoc />
		public override void Iterate(RedBlackNode<T> root, TraverseMethod method, HorizontalFlow flow, Action<RedBlackNode<T>, RedBlackNode<T>, int> visitCallback)
		{
			if (root == null) return;
			new WithParentIterator(this, root, method).Iterate(flow, visitCallback);
		}

		/// <inheritdoc />
		public override void Iterate(RedBlackNode<T> root, TraverseMethod method, HorizontalFlow flow, Func<RedBlackNode<T>, RedBlackNode<T>, int, bool> visitCallback)
		{
			if (root == null) return;
			new WithParentIterator(this, root, method).Iterate(flow, visitCallback);
		}

		/// <inheritdoc />
		public override int GetHeight()
		{
			if (Root == null || Root.IsLeaf) return 0;
			int height = 0;
			Iterate(Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => height += 1 + Math.Max(e.Left == null ? -1 : 1, e.Right == null ? -1 : 1));
			return height;
		}

		public override RedBlackNode<T> FindNearestLeaf(T value)
		{
			RedBlackNode<T> parent = null, next = Root;

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

		public RedBlackNode<T> Predecessor(T value)
		{
			if (Root == null) return null;

			RedBlackNode<T> node = null, root = Root;

			// find the node with the specified value
			// if value is greater, find the value in the right sub-tree
			while (root != null && Comparer.IsGreaterThan(value, root.Value))
			{
				node = root;
				root = root.Right;
			}

			// the maximum value in left subtree is the predecessor node
			if (root != null && Comparer.IsEqual(value, root.Value) && root.Left != null)
			{
				node = root.Left.RightMost();
			}

			return node;
		}

		public RedBlackNode<T> Successor(T value)
		{
			if (Root == null) return null;

			RedBlackNode<T> node = null, root = Root;

			// find the node with the specified value
			// if value is lesser, find the value in the left sub-tree
			while (root != null && Comparer.IsLessThan(value, root.Value))
			{
				node = root;
				root = root.Left;
			}

			// the minimum value in right subtree is the successor node
			if (root != null && Comparer.IsEqual(value, root.Value) && root.Right != null)
			{
				node = root.Right.LeftMost();
			}

			return node;
		}

		/// <inheritdoc />
		public override void Add(T value)
		{
			// find a parent
			RedBlackNode<T> parent = FindNearestLeaf(value);

			if (parent == null)
			{
				// no parent means there is no root currently
				Root = NewNode(value);
				Root.Color = false;
				Count++;
				_version++;
				return;
			}

			// duplicate values means nodes will never be balanced!
			if (Comparer.IsEqual(value, parent.Value)) throw new DuplicateKeyException();

			RedBlackNode<T> node = NewNode(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			Count++;
			_version++;
			if (IsBalanced(node)) return;
			Balance(node);
		}

		public override bool Remove(T value)
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			RedBlackNode<T> node = Find(value);
			if (node == null) return false;

			RedBlackNode<T> newNode;

			// case 1: node has both left and right children
			while (node.IsNode)
			{
				newNode = node.Right.LeftMost();
				newNode.Swap(node);
				node = newNode;
			}

			newNode = node.Left ?? node.Right;
			bool dblBlack = !node.Color && (newNode == null || !newNode.Color);
			RedBlackNode<T> parent = node.Parent;

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
						RedBlackNode<T> sibling = node.Sibling();
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
		public override T Minimum()
		{
			return Root == null
						? default(T)
						: Root.LeftMost().Value;
		}

		/// <inheritdoc />
		public override T Maximum()
		{
			return Root == null
						? default(T)
						: Root.RightMost().Value;
		}

		/// <summary>
		/// Validates the node and its children.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public override bool Validate(RedBlackNode<T> node)
		{
			if (node == null) return true;

			bool isValid = true;
			Iterate(node, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e =>
			{
				if (e.IsLeft)
				{
					if (Comparer.IsGreaterThanOrEqual(e.Value, e.Parent.Value))
						isValid = false;
					else if (e.Parent.IsRight && Comparer.IsLessThan(e.Value, e.Parent.Parent.Value)) 
						isValid = false;
				}

				if (isValid && e.IsRight)
				{
					if (Comparer.IsLessThan(e.Value, e.Parent.Value)) 
						isValid = false;
					else if (isValid && e.Parent.IsLeft && Comparer.IsGreaterThanOrEqual(e.Value, e.Parent.Parent.Value)) 
						isValid = false;
				}

				return isValid;
			});
			return isValid;
		}

		public override bool IsBalanced(RedBlackNode<T> node)
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
		/// <inheritdoc />
		/// <para><see href="https://en.wikipedia.org/wiki/Red%E2%80%93black_tree">RedBlackTree</see> balance implementation.</para>
		/// </summary>
		public override void Balance()
		{
			if (Root == null || Root.IsLeaf) return;

			// find all unbalanced nodes
			Queue<RedBlackNode<T>> unbalancedNodes = new Queue<RedBlackNode<T>>();
			Iterate(Root, TraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e =>
			{
				if (IsBalanced(e)) return;
				unbalancedNodes.Enqueue(e);
			});

			while (unbalancedNodes.Count > 0)
			{
				RedBlackNode<T> node = unbalancedNodes.Dequeue();
				// check again if status changed
				if (IsBalanced(node)) continue;
				Balance(node);
			}
		}

		/// <inheritdoc />
		public override void FromLevelOrder(IEnumerable<T> collection)
		{
			base.FromLevelOrder(collection);
			if (Root == null) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromPreOrder(IEnumerable<T> collection)
		{
			base.FromPreOrder(collection);
			if (Root == null) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrder(IEnumerable<T> collection)
		{
			base.FromInOrder(collection);
			if (Root == null) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromPostOrder(IEnumerable<T> collection)
		{
			base.FromPostOrder(collection);
			if (Root == null) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrderAndLevelOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> levelOrderCollection)
		{
			base.FromInOrderAndLevelOrder(inOrderCollection, levelOrderCollection);
			if (Root == null) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrderAndPreOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> preOrderCollection)
		{
			base.FromInOrderAndPreOrder(inOrderCollection, preOrderCollection);
			if (Root == null) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrderAndPostOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> postOrderCollection)
		{
			base.FromInOrderAndPostOrder(inOrderCollection, postOrderCollection);
			if (Root == null) return;
			Balance();
		}

		private void Balance(RedBlackNode<T> node)
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
					RedBlackNode<T> parent = node.Parent;
					RedBlackNode<T> grandParent = parent.Parent;
					RedBlackNode<T> uncle = node.Uncle();

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
					RedBlackNode<T> sibling = node.Sibling(), parent = node.Parent;

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
		private RedBlackNode<T> RotateLeft([NotNull] RedBlackNode<T> node /* x */)
		{
			bool isLeft = node.IsLeft;
			RedBlackNode<T> oldParent /* y */ = node.Parent;
			RedBlackNode<T> newRoot /* y */ = node.Right;
			RedBlackNode<T> oldLeft /* T2 */ = newRoot.Left;

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
		private RedBlackNode<T> RotateRight([NotNull] RedBlackNode<T> node /* y */)
		{
			bool isLeft = node.IsLeft;
			RedBlackNode<T> oldParent /* y */ = node.Parent;
			RedBlackNode<T> newRoot /* x */ = node.Left;
			RedBlackNode<T> oldRight /* T2 */ = newRoot.Right;

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
		public static string ToString<T>([NotNull] this RedBlackTree<T> thisValue, Orientation orientation, bool diagnosticInfo = false)
		{
			if (thisValue.Root == null) return string.Empty;
			return orientation switch
			{
				Orientation.Horizontal => Horizontally(thisValue, diagnosticInfo),
				Orientation.Vertical => Vertically(thisValue, diagnosticInfo),
				_ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
			};

			static string Horizontally(RedBlackTree<T> tree, bool diagnostic)
			{
				const string STR_BLANK = "    ";
				const string STR_EXT = "│   ";
				const string STR_CONNECTOR = "─── ";
				const string STR_CONNECTOR_L = "└── ";
				const string STR_CONNECTOR_R = "┌── ";

				StringBuilder sb = new StringBuilder();
				Stack<string> connectors = new Stack<string>();
				tree.Iterate(tree.Root, TraverseMethod.InOrder, HorizontalFlow.RightToLeft, (e, depth) =>
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

			static string Vertically(RedBlackTree<T> tree, bool diagnostic)
			{
				const char C_BLANK = ' ';
				const char C_EXT = '─';
				const char C_CONNECTOR_L = '┌';
				const char C_CONNECTOR_R = '┐';

				int distance = 0;
				IDictionary<int, StringBuilder> lines = new Dictionary<int, StringBuilder>();
				tree.Iterate(tree.Root, TraverseMethod.InOrder, HorizontalFlow.LeftToRight, (e, depth) =>
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

				return string.Join(Environment.NewLine, lines.OrderBy(e => e.Key)
															.Select(e => e.Value));
			}
		}
	}
}
using System;
using System.Collections.Generic;
using asm.Exceptions.Collections;
using asm.Extensions;
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
		/// <inheritdoc />
		public RedBlackTree() 
		{
		}

		/// <inheritdoc />
		public RedBlackTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public RedBlackTree([NotNull] IEnumerable<T> collection)
			: base(collection)
		{
		}

		/// <inheritdoc />
		public RedBlackTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = true;

		/// <inheritdoc />
		public override RedBlackNode<T> NewNode(T value) { return new RedBlackNode<T>(value); }

		/// <inheritdoc />
		public override int GetHeight()
		{
			if (Root == null || Root.IsLeaf) return 0;
			int height = 0;
			IterateLevels(Root, HorizontalFlow.LeftToRight, (i, _) => height++);
			return height;
		}

		public override RedBlackNode<T> FindNearestParent(T value)
		{
			RedBlackNode<T> parent = null, next = Root;

			while (next != null && !Comparer.IsEqual(next.Value, value))
			{
				parent = next;
				next = Comparer.IsLessThan(value, next.Value)
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
			RedBlackNode<T> parent = FindNearestParent(value);

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
			Iterate(node, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e =>
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
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e =>
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
}
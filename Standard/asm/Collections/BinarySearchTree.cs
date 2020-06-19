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
	/// <para><see href="https://en.wikipedia.org/wiki/Binary_search_tree">BinarySearchTree</see> implementation.</para>
	/// </summary>
	[Serializable]
	public class BinarySearchTree<T> : LinkedBinaryTree<T>
	{
		protected const int BALANCE_FACTOR = 1;

		/// <inheritdoc />
		public BinarySearchTree() 
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> collection)
			: base(collection)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = false;

		/// <inheritdoc />
		public override LinkedBinaryNode<T> FindNearestParent(T value)
		{
			LinkedBinaryNode<T> parent = null, next = Root;

			while (next != null && !Comparer.IsEqual(next.Value, value))
			{
				parent = next;
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
			}

			return parent;
		}

		/// <inheritdoc />
		public override void Add(T value)
		{
			if (Root == null)
			{
				// no parent means there is no root currently
				Root = NewNode(value);
				Count++;
				SetHeight(Root);
				_version++;
				return;
			}

			// find a parent
			LinkedBinaryNode<T> parent = Root, next = Root;
			Stack<LinkedBinaryNode<T>> stack = new Stack<LinkedBinaryNode<T>>();

			while (next != null)
			{
				parent = next;
				stack.Push(parent);
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
			}

			LinkedBinaryNode<T> node = NewNode(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			// update nodes
			while (stack.Count > 0)
			{
				parent = stack.Pop();
				SetHeight(parent);
			}

			Count++;
			_version++;
		}

		/// <inheritdoc />
		public override bool Remove(T value)
		{
			if (Root == null) return false;

			// find the node
			int cmp;
			LinkedBinaryNode<T> parent = null, node = Root;
			Stack<LinkedBinaryNode<T>> stack = new Stack<LinkedBinaryNode<T>>();

			while (node != null && (cmp = Comparer.Compare(value, node.Value)) != 0)
			{
				parent = node;
				stack.Push(node);
				node = cmp < 0
							? node.Left
							: node.Right;
			}

			if (node == null || !Comparer.IsEqual(value, node.Value)) return false;

			LinkedBinaryNode<T> child;

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
				stack.Push(node.Right);
				child = node.Right;
			}
			// case 3: node has a right child that has a left child
			else
			{
				// find the right child's left most child
				LinkedBinaryNode<T> leftMostParent = parent;
				LinkedBinaryNode<T> leftmost = node.Right;

				while (leftmost.Left != null)
				{
					leftMostParent = leftmost;
					stack.Push(leftMostParent);
					leftmost = leftMostParent.Left;
				}

				stack.Push(leftmost);
				// move the left-most right to the parent's left
				if (leftMostParent != null) leftMostParent.Left = leftmost.Right;
				// adjust the left-most child nodes
				leftmost.Left = node.Left;
				leftmost.Right = node.Right;
				if (leftmost.Left != null) stack.Push(leftmost.Left);
				if (leftmost.Right != null) stack.Push(leftmost.Right);
				child = leftmost;
			}

			if (parent == null)
			{
				Root = child;
				if (child != null) stack.Push(child);
			}
			else if (Comparer.IsLessThan(node.Value, parent.Value))
			{
				// if node < parent, move the left to the parent's left
				parent.Left = child;
				stack.Push(parent);
			}
			else
			{
				// else, move the left to the parent's right
				parent.Right = child;
				stack.Push(parent);
			}

			// update nodes
			while (stack.Count > 0)
			{
				SetHeight(stack.Pop());
			}

			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public override T Minimum()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			return Root == null
						? default(T)
						: Root.LeftMost().Value;
		}

		/// <inheritdoc />
		public override T Maximum()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			return Root == null
						? default(T)
						: Root.RightMost().Value;
		}

		/// <inheritdoc />
		public override bool Validate(LinkedBinaryNode<T> node)
		{
			// https://leetcode.com/problems/validate-binary-search-tree/solution/
			if (node == null) return true;

			T previous = default(T);
			bool isValid = true, started = false;
			Iterate(node, BinaryTreeTraverseMethod.InOrder, HorizontalFlow.LeftToRight, e =>
			{
				if (!started)
				{
					started = true;
					previous = e.Node.Value;
					return true;
				}

				isValid = Comparer.IsLessThan(previous, e.Node.Value);
				return isValid;
			});
			return isValid;
		}

		/// <inheritdoc />
		public override bool IsBalanced(LinkedBinaryNode<T> node)
		{
			return node == null
					|| node.IsLeaf
					|| Math.Abs(node.BalanceFactor) <= BALANCE_FACTOR
					&& (node.Left == null 
						|| node.Left.IsLeaf 
						|| Math.Abs(node.Left.BalanceFactor) <= BALANCE_FACTOR)
					&& (node.Right == null 
						|| node.Right.IsLeaf 
						|| Math.Abs(node.Right.BalanceFactor) <= BALANCE_FACTOR);
		}

		/// <summary>
		/// <inheritdoc />
		/// <para><see href="https://en.wikipedia.org/wiki/AVL_tree">AVLTree</see> balance implementation.</para>
		/// </summary>
		public override void Balance()
		{
			if (Root == null || Root.IsLeaf) return;

			Queue<LinkedBinaryNodeInfo> unbalancedNodes = new Queue<LinkedBinaryNodeInfo>();

			// find all unbalanced nodes, will use a post order traversal
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e =>
			{
				if (!IsBalanced(e.Node)) unbalancedNodes.Enqueue(e);
			});

			while (unbalancedNodes.Count > 0)
			{
				LinkedBinaryNodeInfo current = unbalancedNodes.Dequeue();
				// check again if status changed
				if (IsBalanced(current.Node)) continue;
				Balance(current);
			}
		}

		/// <inheritdoc />
		public override void FromLevelOrder(IEnumerable<T> collection)
		{
			base.FromLevelOrder(collection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromPreOrder(IEnumerable<T> collection)
		{
			base.FromPreOrder(collection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrder(IEnumerable<T> collection)
		{
			base.FromInOrder(collection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromPostOrder(IEnumerable<T> collection)
		{
			base.FromPostOrder(collection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrderAndLevelOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> levelOrderCollection)
		{
			base.FromInOrderAndLevelOrder(inOrderCollection, levelOrderCollection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrderAndPreOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> preOrderCollection)
		{
			base.FromInOrderAndPreOrder(inOrderCollection, preOrderCollection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		/// <inheritdoc />
		public override void FromInOrderAndPostOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> postOrderCollection)
		{
			base.FromInOrderAndPostOrder(inOrderCollection, postOrderCollection);
			if (Root == null) return;
			Iterate(Root, BinaryTreeTraverseMethod.PostOrder, HorizontalFlow.LeftToRight, e => SetHeight(e.Node));
			if (!AutoBalance) return;
			Balance();
		}

		// BUG: not stable at large numbers of nodes 5000+
		protected void Balance(LinkedBinaryNodeInfo nodeInfo)
		{
			/*
			 * There are 4 cases for unbalanced nodes
			 * 1. Left-Left Case: y is left child of z and x is left child of y
			 *
			 *	       z                                      y 
			 *	      / \                                   /   \
			 *	     y   T4      Right Rotate(z) ->        x      z
			 *	    / \                                  /  \    /  \ 
			 *	   x   T3                               T1  T2  T3  T4
			 *	  / \
			 *	T1   T2
			 *
			 * 2. Left-Right Case: y is left child of z and x is right child of y
			 *
			 *	     z                               z                           x
			 *	    / \                            /   \                        /  \ 
			 *	   y   T4  Left Rotate(y) ->      x    T4  Right Rotate(z) -> y     z
			 *	  / \                            /  \                        / \   / \
			 *	T1   x                          y   T3                     T1  T2 T3  T4
			 *	    / \                        / \
			 *	  T2   T3                    T1   T2
			 *
			 * 3. Right-Right Case: y is right child of z and x is right child of y
			 *
			 *	  z                                y
			 *	 /  \                            /   \ 
			 *	T1   y     Left Rotate(z)       z      x
			 *	    /  \   - - - - - - - ->    / \    / \
			 *	   T2   x                     T1  T2 T3  T4
			 *	       / \
			 *	     T3  T4
			 *
			 * 4. Right-Left Case: y is right child of z and x is left child of y
			 *
			 *	   z                            z                            x
			 *	  / \                          / \                          /  \ 
			 *	T1   y   Right Rotate (y)    T1   x      Left Rotate(z)   z      y
			 *	    / \  - - - - - - - - ->      / \   - - - - - - - ->  / \    / \
			 *	   x   T4                       T2  y                   T1  T2 T3  T4
			 *	  / \                              / \
			 *	T2   T3                           T3  T4
			 */
			if (IsBalanced(nodeInfo.Node)) return;

			Queue<LinkedBinaryNodeInfo> queue = new Queue<LinkedBinaryNodeInfo>();
			queue.Enqueue(nodeInfo);

			while (queue.Count > 0)
			{
				nodeInfo = queue.Dequeue();
				if (Math.Abs(nodeInfo.Node.BalanceFactor) <= BALANCE_FACTOR) continue;

				bool changed = false;

				if (nodeInfo.Node.BalanceFactor > 1) // left heavy
				{
					if (nodeInfo.Node.Left.BalanceFactor < 0)
					{
						// duplicate values means nodes will never be balanced!
						if (Comparer.IsEqual(nodeInfo.Node.Value, nodeInfo.Node.Left.Value)) throw new DuplicateKeyException();
						RotateLeft(NodeInfo(nodeInfo.Node.Left, nodeInfo));
					}

					nodeInfo = RotateRight(nodeInfo);
					changed = true;
				}
				else if (nodeInfo.Node.BalanceFactor < 1) // right heavy
				{
					if (nodeInfo.Node.Right.BalanceFactor > 0) RotateRight(NodeInfo(nodeInfo.Node.Right, nodeInfo));
					// duplicate values means nodes will never be balanced!
					if (Comparer.IsEqual(nodeInfo.Node.Value, nodeInfo.Node.Right.Value)) throw new DuplicateKeyException();
					nodeInfo = RotateLeft(nodeInfo);
					changed = true;
				}

				if (!changed) continue;
				if (Math.Abs(nodeInfo.Node.BalanceFactor) > BALANCE_FACTOR) queue.Enqueue(nodeInfo);
				if (nodeInfo.Node.Left != null && Math.Abs(nodeInfo.Node.Left.BalanceFactor) > BALANCE_FACTOR) queue.Enqueue(NodeInfo(nodeInfo.Node.Left, nodeInfo));
				if (nodeInfo.Node.Right != null && Math.Abs(nodeInfo.Node.Right.BalanceFactor) > BALANCE_FACTOR) queue.Enqueue(NodeInfo(nodeInfo.Node.Right, nodeInfo));
			}
		}
	}
}
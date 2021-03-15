using System;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
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
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = false;

		/// <inheritdoc />
		public override LinkedBinaryNode<T> FindNearestParent(T value)
		{
			int cmp;
			LinkedBinaryNode<T> parent = null, next = Root;

			while (next != null && (cmp = Comparer.Compare(value, next.Value)) != 0)
			{
				parent = next;
				next = cmp < 0
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
				Root = MakeNode(value);
				Count++;
				_version++;
				return;
			}

			LinkedBinaryNode<T> parent = null, next = Root;
			// the stack has the same effect as the recursive call but only it's iterative now
			Stack<LinkedBinaryNode<T>> stack = new Stack<LinkedBinaryNode<T>>();
			int order = 0;

			// find a parent. as a general rule: whenever a node's left or right changes, it will be pushed to get updated
			while (next != null)
			{
				order = Comparer.Compare(value, next.Value);
				parent = next;
				stack.Push(parent);
				next = order < 0
							? next.Left
							: next.Right;
			}

			Debug.Assert(parent != null, "No parent node found for the new node.");

			LinkedBinaryNode<T> node = MakeNode(value);

			if (order < 0) parent.Left = node;
			else parent.Right = node;

			// update parents
			while (stack.Count > 0)
				SetHeight(stack.Pop());

			Count++;
			_version++;
		}

		/// <inheritdoc />
		public override bool Remove(T value)
		{
			/*
			 * Udemy courses sometimes contains garbage! the previous code was wrong or contains
			 * at least 2 bugs! Avoid the following implementations for BST or AVL trees:
			 * Buggy: Master the Coding Interview Data Structures + Algorithms - Andrei Neagoie.
			 * Less than optimal: Mastering Data Structures & Algorithms using C and C++ - Abdul Bari
			 *
			 * This one is much better but with a small modification because it contains a bug as well
			 * for not handling leaf nodes.
			 * https://www.geeksforgeeks.org/binary-search-tree-set-3-iterative-delete/?ref=rp
			 */
			if (Root == null) return false;

			LinkedBinaryNode<T> parent = null, node = null, next = Root;
			// will need to use a deque or a linked list instead of the stack because of swap in case 3
			LinkedDeque<LinkedBinaryNode<T>> deque = new LinkedDeque<LinkedBinaryNode<T>>();

			// find the node. as a general rule: whenever a node's left or right changes, it will be pushed to get updated
			while (next != null)
			{
				int order = Comparer.Compare(value, next.Value);

				if (order == 0)
				{
					node = next;
					break;
				}

				parent = next;
				deque.Push(parent);
				next = order < 0
							? next.Left
							: next.Right;
			}

			if (node == null) return false;

			// case 1: node has no children
			if (node.IsLeaf)
			{
				if (parent == null)
				{
					Root = null;
				}
				else
				{
					if (ReferenceEquals(parent.Left, node))
						parent.Left = null;
					else
						parent.Right = null;
				}
			}
			// case 2: node has one child
			else if (node.HasOneChild)
			{
				LinkedBinaryNode<T> newNode = node.Left ?? node.Right;

				if (parent == null)
				{
					Root = newNode;
				}
				else
				{
					if (ReferenceEquals(parent.Left, node))
						parent.Left = newNode;
					else
						parent.Right = newNode;
				}
			}
			// case 3: node has 2 children
			else
			{
				// find the right child's left most child (successor)
				LinkedBinaryNode<T> leftMostParent = null;
				LinkedBinaryNode<T> successor = node.Right;

				while (successor.Left != null)
				{
					leftMostParent = successor;
					deque.Push(leftMostParent);
					successor = successor.Left;
				}

				/*
				 * if there is a left-most for the node's right node, then
				 * move its right to its parent's left.
				 */ 
				if (leftMostParent != null)
				{
					leftMostParent.Left = successor.Right;

					// insert the node because it will be swapped later with the successor
					if (parent != null)
						deque.PushBefore(parent, node);
					else
						deque.PushLast(node);
				}
				// Otherwise, move the successor's right to the node's right
				else
				{
					node.Right = successor.Right;
					deque.Push(node);
				}

				// swap the value with the successor
				node.Swap(successor);
			}

			// update parents
			while (deque.Count > 0)
				SetHeight(deque.Pop());

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
			Iterate(node, e =>
			{
				if (!started)
				{
					started = true;
					previous = e.Value;
					return true;
				}

				isValid = Comparer.IsLessThan(previous, e.Value);
				return isValid;
			});
			return isValid;
		}

		/// <inheritdoc />
		public override bool IsBalanced()
		{
			if (Root == null || Root.IsLeaf) return true;
			
			bool balanced = true;
			Iterate(TreeTraverseMethod.LevelOrder, e =>
			{
				balanced &= IsBalancedLocal(e);
				return balanced;
			});
			return balanced;

			static bool IsBalancedLocal(LinkedBinaryNode<T> node)
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
		}

		/// <inheritdoc />
		public override void FromLevelOrder(IEnumerable<T> collection)
		{
			base.FromLevelOrder(collection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		/// <inheritdoc />
		public override void FromPreOrder(IEnumerable<T> collection)
		{
			base.FromPreOrder(collection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		/// <inheritdoc />
		public override void FromInOrder(IEnumerable<T> collection)
		{
			base.FromInOrder(collection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		/// <inheritdoc />
		public override void FromPostOrder(IEnumerable<T> collection)
		{
			base.FromPostOrder(collection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		/// <inheritdoc />
		public override void FromInOrderAndLevelOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> levelOrderCollection)
		{
			base.FromInOrderAndLevelOrder(inOrderCollection, levelOrderCollection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		/// <inheritdoc />
		public override void FromInOrderAndPreOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> preOrderCollection)
		{
			base.FromInOrderAndPreOrder(inOrderCollection, preOrderCollection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		/// <inheritdoc />
		public override void FromInOrderAndPostOrder(IEnumerable<T> inOrderCollection, IEnumerable<T> postOrderCollection)
		{
			base.FromInOrderAndPostOrder(inOrderCollection, postOrderCollection);
			if (Root == null) return;
			Iterate(TreeTraverseMethod.PostOrder, SetHeight);
		}

		[NotNull]
		private LinkedBinaryNode<T> Balance([NotNull] LinkedBinaryNode<T> node)
		{
			// shouldn't be called unless Math.Abs(node.BalanceFactor) > BALANCE_FACTOR
			if (node.BalanceFactor > 1) // left heavy
			{
				if (node.Left.BalanceFactor < 0)
				{
					// LR case
					node.Left = RotateLeft(node.Left);
					SetHeight(node);
				}

				// LL case
				return RotateRight(node);
			}

			if (node.BalanceFactor < 1) // right heavy
			{
				if (node.Right.BalanceFactor > 0)
				{
					// RL case
					node.Right = RotateRight(node.Right);
					SetHeight(node);
				}

				// RR case
				return RotateLeft(node);
			}

			return node;
		}
	}
}
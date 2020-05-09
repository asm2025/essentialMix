using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using asm.Extensions;
using asm.Patterns.Collections;
using asm.Patterns.Direction;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><strong>Binary search tree implementation.</strong></para>
	/// </summary>
	/// <typeparam name="T">The element type of the tree</typeparam>
	[Serializable]
	public sealed class BinarySearchTree<T> : LinkedBinaryTree<T>
	{
		private const int BALANCE_FACTOR = 1;

		/// <inheritdoc />
		public BinarySearchTree()
			: this(Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(T value)
			: this(value, Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree(T value, IComparer<T> comparer)
			: base(comparer)
		{
			Add(value);
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public BinarySearchTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(comparer)
		{
			Add(collection);
		}

		/// <inheritdoc />
		internal BinarySearchTree(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = false;

		/// <inheritdoc />
		public override Node Successor(T value)
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
		public override void Add(T value)
		{
			Node parent = Successor(value);

			if (parent == null)
			{
				// no parent means there is no root currently
				Root = new Node(value);
				Count++;
				_version++;
				return;
			}
			
			Node node = new Node(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			// update nodes
			UpdateDown(node);
			UpdateUp(parent);
			Count++;
			_version++;
		}

		/// <inheritdoc />
		public override bool Remove(Node node)
		{
			Node parent = node.Parent;
			Node child;

			// case 1: node has no right child
			if (node.Right == null)
			{
				child = node.Left;

				if (parent == null)
				{
					Root = child;
				}
				else if (Comparer.IsLessThan(node.Value, parent.Value))
				{
					// if node < parent, move the left to the parent's left
					parent.Left = child;
				}
				else
				{
					// else, move the left to the parent's right
					parent.Right = child;
				}
			}
			// case 2: node has a right child which doesn't have a left child
			else if (node.Right.Left == null)
			{
				// move the left to the right child's left
				node.Right.Left = node.Left;
				child = node.Right;

				if (parent == null)
				{
					Root = child;
				}
				else if (Comparer.IsLessThan(node.Value, parent.Value))
				{
					// if node < parent, move the right to the parent's left
					parent.Left = child;
				}
				else
				{
					// else, move the right to the parent's right
					parent.Right = child;
				}
			}
			// case 3: node has a right child that has a left child
			else
			{
				// find the right child's left most child
				Node leftmost = node.Right.Left;

				while (leftmost.Left != null)
				{
					leftmost = leftmost.Left;
				}

				// move the left-most right to the parent's left
				Node leftMostParent = leftmost.Parent;
				leftMostParent.Left = leftmost.Right;
				// update nodes
				UpdateDown(leftMostParent.Left);
				UpdateUp(leftMostParent);
				// adjust the left-most child nodes
				leftmost.Left = node.Left;
				leftmost.Right = node.Right;
				child = leftmost;

				if (parent == null)
				{
					Root = child;
				}
				else if (Comparer.IsLessThan(node.Value, parent.Value))
				{
					// if node < parent, move the left-most to the parent's left
					parent.Left = child;
				}
				else
				{
					// else, move the left-most to the parent's right
					parent.Right = child;
				}
			}

			if (child != null)
			{
				if (parent == null) child.Parent = null;
				// update nodes
				UpdateDown(child);
				UpdateUp(child);
			}
			else if (parent != null)
			{
				// update nodes
				UpdateUp(parent);
			}

			Count--;
			_version++;
			return true;
		}

		/// <inheritdoc />
		public override bool Validate(Node node)
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

		/// <inheritdoc />
		public override bool IsBalanced(Node node)
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
		/// <para><strong>This works the same as the <see cref="AVLTree{T}"/></strong></para>
		/// </summary>
		public override void Balance()
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
			/*
			 * balance the tree
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
			 *	    / \  - - - - - - - - ->     /  \   - - - - - - - ->  / \    / \
			 *	   x   T4                      T2   y                  T1  T2  T3  T4
			 *	  / \                              /  \
			 *	T2   T3                           T3   T4
			 */
			if (node == null || IsBalanced(node)) return;

			int factor = Math.Abs(node.BalanceFactor);

			do
			{
				// update root reference
				Node parent = node.Parent;

				if (node.BalanceFactor > 1)
				{
					// Left-Left Case
					if (node.Left?.BalanceFactor < 0) RotateLeft(node.Left);
					// Left-Right Case
					node = RotateRight(node);
				}
				else if (node.BalanceFactor < -1)
				{
					// Right-Right Case
					if (node.Right?.BalanceFactor > 0) RotateRight(node.Right);
					// Right-Left Case
					node = RotateLeft(node);
				}

				if (parent == null)
				{
					Root = node;
					node = null;
				}
				else
				{
					node = !IsBalanced(parent)
								? parent
								: parent.Parent;
				}

				if (node == null) continue;
				factor = Math.Abs(node.BalanceFactor);

				// find the first unbalanced parent, keep going up while Abs(BalanceFactor) < 2
				while (factor <= BALANCE_FACTOR && node.Parent != null)
				{
					node = node.Parent;
					factor = Math.Abs(node.BalanceFactor);
				}
			}
			while (node != null && factor > BALANCE_FACTOR);
		}
	}
}
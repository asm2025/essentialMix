using System;
using System.Collections.Generic;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/AVL_tree">AVLTree</see> implementation.</para>
	/// </summary>
	[Serializable]
	public sealed class AVLTree<T> : BinarySearchTree<T>
	{
		/// <inheritdoc />
		public AVLTree() 
		{
		}

		/// <inheritdoc />
		public AVLTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public AVLTree([NotNull] IEnumerable<T> collection)
			: base(collection)
		{
		}

		/// <inheritdoc />
		public AVLTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override bool AutoBalance { get; } = true;

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
				stack.Push(next);
				parent = next;
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
			}

			// duplicate values can make life miserable for us here because it will never be balanced!
			if (Comparer.IsEqual(value, parent.Value)) throw new DuplicateKeyException();

			LinkedBinaryNode<T> node = NewNode(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			Queue<LinkedBinaryNode<T>> unbalancedNodes = new Queue<LinkedBinaryNode<T>>();

			// update parents and find unbalanced parents in the changed nodes along the way
			// this has the same effect as the recursive call but only it's iterative now
			while (stack.Count > 0)
			{
				node = stack.Pop();
				SetHeight(node);
				if (IsBalanced(node)) continue;
				unbalancedNodes.Enqueue(node);
			}

			Count++;
			_version++;

			while (unbalancedNodes.Count > 0)
			{
				node = unbalancedNodes.Dequeue();
				// check again if status changed
				if (IsBalanced(node)) continue;
				Balance(node);
			}
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
				stack.Push(node);
				parent = node;
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
					stack.Push(leftmost.Left);
					leftmost = leftMostParent.Left;
				}

				// move the left-most right to the parent's left
				if (leftMostParent != null) leftMostParent.Left = leftmost.Right;
				// adjust the left-most child nodes
				leftmost.Left = node.Left;
				leftmost.Right = node.Right;
				stack.Push(leftmost.Left);
				stack.Push(leftmost.Right);
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

			Queue<LinkedBinaryNode<T>> unbalancedNodes = new Queue<LinkedBinaryNode<T>>();

			// update nodes
			while (stack.Count > 0)
			{
				node = stack.Pop();
				SetHeight(node);
				if (IsBalanced(node)) continue;
				unbalancedNodes.Enqueue(node);
			}

			Count--;
			_version++;

			while (unbalancedNodes.Count > 0)
			{
				node = unbalancedNodes.Dequeue();
				// check again if status changed
				if (IsBalanced(node)) continue;
				Balance(node);
			}

			return true;
		}
	}
}
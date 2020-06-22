using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public AVLTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public AVLTree([NotNull] IEnumerable<T> collection)
			: this(collection, null)
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
				_version++;
				return;
			}

			LinkedBinaryNode<T> parent = Root, next = Root;
			// the deque has the same effect as the recursive call but only it's iterative now
			LinkedDeque<LinkedBinaryNode<T>> deque = new LinkedDeque<LinkedBinaryNode<T>>();

			// find a parent
			// as a general role: whenever a node's left or right changes, it will be pushed to the deque to get updated
			while (next != null)
			{
				parent = next;
				deque.Push(parent);
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
			}

			// duplicate values can make life miserable for us here because it will never be balanced!
			if (Comparer.IsEqual(value, parent.Value)) throw new DuplicateKeyException();

			LinkedBinaryNode<T> node = NewNode(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			UpdateAndBalance(deque);
			Count++;
			_version++;
		}

		/// <inheritdoc />
		public override bool Remove(T value)
		{
			// Udemy - Master the Coding Interview Data Structures + Algorithms - Andrei Neagoie
			if (Root == null) return false;

			LinkedBinaryNode<T> parent = null, node = null, next = Root;
			// the deque has the same effect as the recursive call but only it's iterative now
			LinkedDeque<LinkedBinaryNode<T>> deque = new LinkedDeque<LinkedBinaryNode<T>>();

			// find the node
			// as a general role: whenever a node's left or right changes, it will be pushed to the deque to get updated
			while (next != null)
			{
				int cmp = Comparer.Compare(value, next.Value);

				if (cmp == 0)
				{
					node = next;
					break;
				}

				parent = next;
				deque.Push(parent);
				next = cmp < 0
							? next.Left
							: next.Right;
			}

			if (node == null) return false;

			// case 1: node has no right child
			if (node.Right == null)
			{
				if (parent == null)
				{
					Root = node.Left;
				}
				else
				{
					if (Comparer.IsLessThan(node.Value, parent.Value))
						parent.Left = node.Left;
					else
						parent.Right = node.Left;
				}
			}
			// case 2: node has a right child which doesn't have a left child
			else if (node.Right.Left == null)
			{
				// move the left to the right child's left
				node.Right.Left = node.Left;
				deque.Push(node.Right);

				if (parent == null)
				{
					Root = node.Right;
				}
				else
				{
					if (Comparer.IsLessThan(node.Value, parent.Value))
						parent.Left = node.Right;
					else
						parent.Right = node.Right;
				}
			}
			// case 3: node has a right child that has a left child
			else
			{
				// find the right child's left most child
				LinkedBinaryNode<T> leftMostParent = node.Right;
				LinkedBinaryNode<T> leftmost = leftMostParent.Left;
				deque.Push(leftMostParent);

				while (leftmost.Left != null)
				{
					leftMostParent = leftmost;
					deque.Push(leftMostParent);
					leftmost = leftmost.Left;
				}

				// move the left-most right to the parent's left
				leftMostParent.Left = leftmost.Right;
				// adjust the left-most child nodes
				leftmost.Left = node.Left;
				// don't add the node to itself
				leftmost.Right = node.Right;

				if (parent == null)
				{
					Root = leftmost;
					// add this to be the last to be updated
					deque.PushLast(leftmost);
				}
				else
				{
					if (Comparer.IsLessThan(node.Value, parent.Value))
						parent.Left = leftmost;
					else
						parent.Right = leftmost;

					deque.PushBefore(parent, leftmost);
				}
			}

			UpdateAndBalance(deque);
			Count--;
			_version++;
			return true;
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private void UpdateAndBalance([NotNull] LinkedDeque<LinkedBinaryNode<T>> deque)
		{
			// update parents
			while (deque.Count > 0)
			{
				LinkedBinaryNode<T> node = deque.Pop();
				SetHeight(node);
				// check the balance
				if (Math.Abs(node.BalanceFactor) <= BALANCE_FACTOR) continue;
				
				LinkedBinaryNode<T> parent = deque.Count > 0
												? deque.PeekStack()
												: null;
				bool isLeft = parent != null && ReferenceEquals(parent.Left, node);
				node = Balance(node);
				
				if (parent == null)
				{
					Root = node;
				}
				else
				{
					if (isLeft) parent.Left = node;
					else parent.Right = node;

					SetHeight(parent);
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para>AVLTree implementation.</para>
	/// </summary>
	[Serializable]
	public sealed class AVLTree<T> : BinarySearchTree<T>
	{
		/// <inheritdoc />
		public AVLTree()
			: this(Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public AVLTree(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public AVLTree(T value, IComparer<T> comparer)
			: base(comparer)
		{
			Add(value);
		}

		public AVLTree([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		public AVLTree([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(comparer)
		{
			foreach (T value in collection) 
				Add(value);
		}

		/// <inheritdoc />
		internal AVLTree(SerializationInfo info, StreamingContext context)
			: base(info, context)
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
				Root = new Node(value);
				Count++;
				_version++;
				return;
			}

			// find a parent
			Node parent = Root, next = Root;
			Stack<Node> stack = new Stack<Node>();

			while (next != null)
			{
				parent = next;
				stack.Push(parent);
				next = Comparer.IsLessThan(value, next.Value)
							? next.Left
							: next.Right;
			}

			// duplicate values can make life miserable for us here because it will never be balanced!
			if (Comparer.IsEqual(value, parent.Value)) throw new DuplicateKeyException();

			Node node = new Node(value);

			if (Comparer.IsLessThan(value, parent.Value)) parent.Left = node;
			else parent.Right = node;

			Queue<Node> unbalancedNodes = new Queue<Node>();

			// update parents and find unbalanced parents in the changed nodes along the way
			// this has the same effect as the recursive call but only it's iterative now
			while (stack.Count > 0)
			{
				parent = stack.Pop();
				SetHeight(parent);
				if (IsBalanced(parent)) continue;
				unbalancedNodes.Enqueue(parent);
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
		public override bool Remove(Node node)
		{
			Node parent = node.Parent;
			Node child, leftMostParent = null;

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
				child = node.Right;
			}
			// case 3: node has a right child that has a left child
			else
			{
				// find the right child's left most child
				Node leftmost = node.Right.Minimum();
				// move the left-most right to the parent's left
				leftMostParent = leftmost.Parent;
				leftMostParent.Left = leftmost.Right;
				// adjust the left-most child nodes
				leftmost.Left = node.Left;
				leftmost.Right = node.Right;
				child = leftmost;
			}

			if (parent == null)
			{
				if (child != null) child.Parent = null;
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

			Queue<Node> unbalancedNodes = new Queue<Node>();
			Node update = child != null
							? leftMostParent ?? child
							: parent;

			// update nodes
			while (update != null)
			{
				SetHeight(update);
				if (!IsBalanced(update)) unbalancedNodes.Enqueue(update);
				update = update.Parent;
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
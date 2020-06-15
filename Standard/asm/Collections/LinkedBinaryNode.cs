using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class LinkedBinaryNode<TNode, T>
		where TNode : LinkedBinaryNode<TNode, T>
	{
		private const int LEFT = 0;
		private const int RIGHT = 1;
		private const int PARENT = 2;

		private readonly TNode[] _nodes = new TNode[3];

		protected LinkedBinaryNode(T value)
		{
			Value = value;
		}
		
		public TNode Parent
		{
			get => _nodes[PARENT];
			internal set
			{
				if (_nodes[PARENT] == value) return;

				// reset old parent
				if (_nodes[PARENT] != null)
				{
					/*
					* The comparison with this and parent.left/.right is essential because the node
					* could have moved to another parent. Don't use IsLeft or IsRight here.
					*/
					if (_nodes[PARENT].Left == this) _nodes[PARENT].Left = null;
					else if (_nodes[PARENT].Right == this) _nodes[PARENT].Right = null;
				}

				_nodes[PARENT] = value;
			}
		}

		public TNode Left
		{
			get => _nodes[LEFT];
			internal set
			{
				if (_nodes[LEFT] == value) return;
				// reset old left
				if (_nodes[LEFT]?._nodes[PARENT] == this) _nodes[LEFT]._nodes[PARENT] = null;
				_nodes[LEFT] = value;
				if (_nodes[LEFT] == null) return;
				_nodes[LEFT]._nodes[PARENT] = (TNode)this;
			}
		}

		public virtual TNode Right
		{
			get => _nodes[RIGHT];
			internal set
			{
				if (_nodes[RIGHT] == value) return;
				// reset old right
				if (_nodes[RIGHT]?._nodes[PARENT] == this) _nodes[RIGHT]._nodes[PARENT] = null;
				_nodes[RIGHT] = value;
				if (_nodes[RIGHT] == null) return;
				_nodes[RIGHT]._nodes[PARENT] = (TNode)this;
			}
		}

		public T Value { get; set; }

		public bool IsRoot => _nodes[PARENT] == null;

		public bool IsLeft => _nodes[PARENT]?.Left == this;

		public bool IsRight => _nodes[PARENT]?.Right == this;

		public bool IsLeaf => _nodes[LEFT] == null && _nodes[RIGHT] == null;

		public bool IsNode => _nodes[LEFT] != null && _nodes[RIGHT] != null;

		public bool HasOneChild => (_nodes[LEFT] != null) ^ (_nodes[RIGHT] != null);

		public bool IsFull => !HasOneChild;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		protected internal virtual string ToString(int depth)
		{
			return $"{Value} :D{depth}";
		}

		[ItemNotNull]
		public IEnumerable<TNode> Ancestors()
		{
			TNode node = _nodes[PARENT];

			while (node != null)
			{
				yield return node;
				node = node._nodes[PARENT];
			}
		}

		public TNode Uncle()
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			return _nodes[PARENT]?._nodes[PARENT] == null
						? null // no parent or grand parent
						: _nodes[PARENT].IsLeft
							? _nodes[PARENT]._nodes[PARENT]._nodes[RIGHT] // uncle on the right
							: _nodes[PARENT]._nodes[PARENT]._nodes[LEFT];
		}

		public TNode Sibling()
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			return _nodes[PARENT] == null
						? null // no parent
						: IsLeft
							? _nodes[PARENT]._nodes[RIGHT] // sibling on the right
							: _nodes[PARENT]._nodes[LEFT];
		}

		[NotNull]
		public TNode LeftMost()
		{
			TNode leftMost = (TNode)this;

			while (leftMost.Left != null) 
				leftMost = leftMost.Left;

			return leftMost;
		}

		[NotNull]
		public TNode RightMost()
		{
			TNode rightMost = (TNode)this;

			while (rightMost.Right != null) 
				rightMost = rightMost.Right;

			return rightMost;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void Swap([NotNull] TNode other)
		{
			T tmp = other.Value;
			other.Value = Value;
			Value = tmp;
		}

		public static implicit operator T([NotNull] LinkedBinaryNode<TNode, T> node) { return node.Value; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class LinkedBinaryNode<T> : LinkedBinaryNode<LinkedBinaryNode<T>, T>
	{
		internal LinkedBinaryNode(T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		protected internal override string ToString(int depth)
		{
			return $"{Value} :D{depth}H{Height}B{BalanceFactor}";
		}

		public int Height { get; internal set; }

		public int BalanceFactor => (Left?.Height ?? -1) - (Right?.Height ?? -1);
	}
}
using System;
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

		private readonly TNode[] _nodes = new TNode[2];

		protected LinkedBinaryNode(T value)
		{
			Value = value;
		}

		public TNode Left
		{
			get => _nodes[LEFT];
			internal set
			{
				if (ReferenceEquals(value, this)) throw new InvalidOperationException("Circular reference detected.");
				_nodes[LEFT] = value;
			}
		}

		public TNode Right
		{
			get => _nodes[RIGHT];
			internal set
			{
				if (ReferenceEquals(value, this)) throw new InvalidOperationException("Circular reference detected.");
				_nodes[RIGHT] = value;
			}
		}

		public T Value { get; set; }

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

		public TNode Predecessor()
		{
			TNode node = _nodes[LEFT];
			if (node == null) return null;

			while (node._nodes[RIGHT] != null)
				node = node._nodes[RIGHT];

			return node;
		}

		public TNode Successor()
		{
			TNode node = _nodes[RIGHT];
			if (node == null) return null;

			while (node._nodes[LEFT] != null)
				node = node._nodes[LEFT];

			return node;
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
	}

	[DebuggerDisplay("{Value} :H{Height}B{BalanceFactor}")]
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
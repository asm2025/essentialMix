using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public virtual TNode Left
		{
			get => _nodes[LEFT];
			internal set => _nodes[LEFT] = value;
		}

		public virtual TNode Right
		{
			get => _nodes[RIGHT];
			internal set => _nodes[RIGHT] = value;
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
		protected internal virtual string ToString(int depth, bool diagnostic)
		{
			return diagnostic
						? $"{Value} :D{depth}"
						: Convert.ToString(Value);
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
		protected internal override string ToString(int depth, bool diagnostic)
		{
			return diagnostic
						? $"{Value} :D{depth}H{Height}B{BalanceFactor}"
						: Convert.ToString(Value);
		}

		public int Height { get; internal set; }

		public int BalanceFactor => (Left?.Height ?? -1) - (Right?.Height ?? -1);
	}
}
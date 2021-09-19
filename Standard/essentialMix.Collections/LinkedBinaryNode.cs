using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class LinkedBinaryNode<TNode, T> : INode<T>
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

		public bool IsFull => _nodes[LEFT] != null && _nodes[RIGHT] != null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		protected internal virtual string ToString(int level)
		{
			return $"{Value} :L{level}";
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
			(other.Value, Value) = (Value, other.Value);
		}

		public static implicit operator T([NotNull] LinkedBinaryNode<TNode, T> node) { return node.Value; }
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
		protected internal override string ToString(int level)
		{
			return $"{Value} :L{level}H{Height}B{BalanceFactor}";
		}

		public int Height { get; internal set; }

		public int BalanceFactor => (Left?.Height ?? -1) - (Right?.Height ?? -1);
	}
}
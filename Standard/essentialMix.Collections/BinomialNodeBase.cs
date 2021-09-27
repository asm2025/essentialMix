using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Value} :D{Degree}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class BinomialNodeBase<TNode, T> : ISiblingNode<TNode, T>
		where TNode : BinomialNodeBase<TNode, T>
	{
		private const int PARENT = 0;
		private const int CHILD = 1;
		private const int SIBLING = 2;

		private readonly TNode[] _nodes = new TNode[3];

		protected BinomialNodeBase(T value)
		{
			Value = value;
		}

		public TNode Parent
		{
			get => _nodes[PARENT];
			internal set
			{
				AssertNotCircularRef(value);
				_nodes[PARENT] = value;
			}
		}

		public TNode Child
		{
			get => _nodes[CHILD];
			internal set
			{
				AssertNotCircularRef(value);
				_nodes[CHILD] = value;
			}
		}

		public TNode Sibling
		{
			get => _nodes[SIBLING];
			internal set
			{
				AssertNotCircularRef(value);
				_nodes[SIBLING] = value;
			}
		}

		public T Value { get; set; }

		public int Degree { get; internal set; }

		public bool IsLeaf => _nodes[SIBLING] == null && _nodes[CHILD] == null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public virtual string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}

		[ItemNotNull]
		public IEnumerable<TNode> Ancestors()
		{
			TNode parent = _nodes[PARENT];

			while (parent != null)
			{
				yield return parent;
				parent = parent._nodes[PARENT];
			}
		}

		[ItemNotNull]
		public IEnumerable<TNode> Children()
		{
			TNode child = _nodes[CHILD];

			while (child != null)
			{
				yield return child;
				child = child._nodes[CHILD];
			}
		}

		[ItemNotNull]
		public IEnumerable<TNode> Siblings()
		{
			TNode sibling = _nodes[SIBLING];

			while (sibling != null)
			{
				yield return sibling;
				sibling = sibling._nodes[SIBLING];
			}
		}

		public virtual void Swap([NotNull] TNode other)
		{
			(other.Value, Value) = (Value, other.Value);
		}

		internal void Invalidate()
		{
			Degree = 0;
			Array.Clear(_nodes, 0, _nodes.Length);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private void AssertNotCircularRef(BinomialNodeBase<TNode, T> node)
		{
			if (!ReferenceEquals(this, node)) return;
			throw new InvalidOperationException("Circular reference detected.");
		}

		public static implicit operator T([NotNull] BinomialNodeBase<TNode, T> node) { return node.Value; }
	}
}
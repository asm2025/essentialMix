using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <inheritdoc />
	[Serializable]
	[DebuggerDisplay("{Value} :D{Degree}")]
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

		public TNode LeftMostChild()
		{
			TNode node = null, next = _nodes[CHILD];

			while (next != null)
			{
				node = next;
				next = next._nodes[CHILD];
			}

			return node;
		}

		public TNode RightMostSibling()
		{
			TNode node = null, next = _nodes[SIBLING];

			while (next != null)
			{
				node = next;
				next = next._nodes[SIBLING];
			}

			return node;
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

		public abstract void Swap(TNode other);

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

	/// <inheritdoc cref="BinomialNodeBase{TNode,T}" />
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<T> : BinomialNodeBase<BinomialNode<T>, T>, ISiblingNode<BinomialNode<T>, T>
	{
		/// <inheritdoc />
		public BinomialNode(T value)
			: base(value)
		{
		}

		public override string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}

		/// <inheritdoc />
		public override void Swap(BinomialNode<T> other)
		{
			(other.Value, Value) = (Value, other.Value);
		}
	}

	/// <inheritdoc cref="BinomialNodeBase{TNode,T}" />
	[Serializable]
	[DebuggerDisplay("{Key} = {Value} :D{Degree}")]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<TKey, TValue> : BinomialNodeBase<BinomialNode<TKey, TValue>, TValue>, ISiblingNode<BinomialNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public BinomialNode([NotNull] TKey key, TValue value)
			: base(value)
		{
			_key = key;
		}

		/// <inheritdoc />
		public TKey Key
		{
			get => _key;
			set => _key = value;
		}

		/// <inheritdoc />
		public override string ToString(int level)
		{
			return $"{Key} = {Value} :D{Degree}L{level}";
		}

		/// <inheritdoc />
		public override void Swap(BinomialNode<TKey, TValue> other)
		{
			(other.Key, Key) = (Key, other.Key);
			(other.Value, Value) = (Value, other.Value);
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Key} = {Value} :D{Degree}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class BinomialNode<TNode, TKey, TValue> : IKeyedNode<TKey, TValue>
		where TNode : BinomialNode<TNode, TKey, TValue>
	{
		private const int PARENT = 0;
		private const int CHILD = 1;
		private const int SIBLING = 2;

		private readonly TNode[] _nodes = new TNode[3];

		protected BinomialNode([NotNull] TValue value)
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

		[NotNull]
		public abstract TKey Key { get; set; }

		[NotNull]
		public TValue Value { get; set; }

		public int Degree { get; internal set; }

		public bool IsLeaf => _nodes[SIBLING] == null && _nodes[CHILD] == null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		internal virtual string ToString(int level)
		{
			return $"{Key} = {Value} :D{Degree}L{level}";
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

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void Swap([NotNull] TNode other)
		{
			TKey otherKey = other.Key;
			TValue otherValue = other.Value;
			other.Key = Key;
			other.Value = Value;
			Key = otherKey;
			Value = otherValue;
		}

		internal void Invalidate()
		{
			Degree = 0;
			Array.Clear(_nodes, 0, _nodes.Length);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private void AssertNotCircularRef(BinomialNode<TNode, TKey, TValue> node)
		{
			if (!ReferenceEquals(this, node)) return;
			throw new InvalidOperationException("Circular reference detected.");
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<TKey, TValue> : BinomialNode<BinomialNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public BinomialNode([NotNull] TKey key, [NotNull] TValue value)
			: base(value)
		{
			_key = key;
		}

		/// <inheritdoc />
		public override TKey Key
		{
			get => _key;
			set => _key = value;
		}
	}

	[DebuggerDisplay("{Value} :D{Degree}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BinomialNode<T> : BinomialNode<BinomialNode<T>, T, T>
	{
		/// <inheritdoc />
		public BinomialNode([NotNull] T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public override T Key
		{
			get => Value;
			set => Value = value;
		}

		internal override string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}
	}
}
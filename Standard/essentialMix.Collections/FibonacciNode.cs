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
	public abstract class FibonacciNodeBase<TNode, T> : ISiblingNode<TNode, T>
		where TNode : FibonacciNodeBase<TNode, T>
	{
		private const int PARENT = 0;
		private const int CHILD = 1;
		private const int NEXT = 2;
		private const int PREVIOUS = 3;

		// The previous/next nodes are not exactly a doubly linked list but rather a circular reference
		private readonly TNode[] _nodes = new TNode[4];

		protected FibonacciNodeBase(T value)
		{
			Value = value;
			_nodes[PREVIOUS] = _nodes[NEXT] = (TNode)this;
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

		/// <summary>
		/// The previous/next nodes are not exactly a doubly linked list but rather
		/// a circular reference so will have to watch out for this.
		/// </summary>
		public TNode Previous
		{
			get => _nodes[PREVIOUS];
			internal set => _nodes[PREVIOUS] = value;
		}

		/// <summary>
		/// The previous/next nodes are not exactly a doubly linked list but rather
		/// a circular reference so will have to watch out for this.
		/// </summary>
		public TNode Next
		{
			get => _nodes[NEXT];
			internal set => _nodes[NEXT] = value;
		}

		/// <inheritdoc />
		public TNode Sibling => _nodes[NEXT];

		public T Value { get; set; }

		public int Degree { get; internal set; }

		public bool Marked { get; internal set; }

		public bool IsLeaf => _nodes[NEXT] == null && _nodes[CHILD] == null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public virtual string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}

		[ItemNotNull]
		public IEnumerable<TNode> Backwards(TNode stopAt = null)
		{
			TNode node = _nodes[PREVIOUS];

			while (node != null && node != this && node != stopAt)
			{
				yield return node;
				node = node._nodes[PREVIOUS];
			}
		}

		[ItemNotNull]
		public IEnumerable<TNode> Forwards(TNode stopAt = null)
		{
			TNode node = _nodes[NEXT];

			while (node != null && node != this && node != stopAt)
			{
				yield return node;
				node = node._nodes[NEXT];
			}
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
			TNode node = null, next = _nodes[NEXT];

			while (next != null)
			{
				node = next;
				next = next._nodes[NEXT];
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

		/// <inheritdoc />
		public IEnumerable<TNode> Siblings() { return Forwards(); }

		public abstract void Swap(TNode other);

		internal void Invalidate()
		{
			Degree = 0;
			Marked = false;
			_nodes[PARENT] = _nodes[CHILD] = null;
			_nodes[PREVIOUS] = _nodes[NEXT] = (TNode)this;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private void AssertNotCircularRef(FibonacciNodeBase<TNode, T> node)
		{
			if (!ReferenceEquals(this, node)) return;
			throw new InvalidOperationException("Circular reference detected.");
		}

		public static implicit operator T([NotNull] FibonacciNodeBase<TNode, T> node) { return node.Value; }
	}

	/// <inheritdoc cref="FibonacciNodeBase{TNode,T}" />
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class FibonacciNode<T> : FibonacciNodeBase<FibonacciNode<T>, T>, ISiblingNode<FibonacciNode<T>, T>
	{
		/// <inheritdoc />
		public FibonacciNode(T value)
			: base(value)
		{
		}

		public override string ToString(int level)
		{
			return $"{Value} :D{Degree}L{level}";
		}

		/// <inheritdoc />
		public override void Swap(FibonacciNode<T> other)
		{
			(other.Value, Value) = (Value, other.Value);
		}
	}

	/// <inheritdoc cref="FibonacciNodeBase{TNode,T}" />
	[Serializable]
	[DebuggerDisplay("{Key} = {Value} :D{Degree}")]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class FibonacciNode<TKey, TValue> : FibonacciNodeBase<FibonacciNode<TKey, TValue>, TValue>, ISiblingNode<FibonacciNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public FibonacciNode([NotNull] TKey key, TValue value)
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
		public override void Swap(FibonacciNode<TKey, TValue> other)
		{
			(other.Key, Key) = (Key, other.Key);
			(other.Value, Value) = (Value, other.Value);
		}
	}
}
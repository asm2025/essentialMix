using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Key} = {Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class PairingNode<TNode, TKey, TValue>
		where TNode : PairingNode<TNode, TKey, TValue>
	{
		private const int CHILD = 0;
		private const int SIBLING = 1;
		private const int PREVIOUS = 2;

		private readonly TNode[] _nodes = new TNode[3];

		protected PairingNode([NotNull] TValue value)
		{
			Value = value;
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

		public TNode Previous
		{
			get => _nodes[PREVIOUS];
			internal set
			{
				AssertNotCircularRef(value);
				_nodes[PREVIOUS] = value;
			}
		}

		[NotNull]
		public abstract TKey Key { get; set; }

		[NotNull]
		public TValue Value { get; set; }

		public bool IsLeaf => _nodes[SIBLING] == null && _nodes[CHILD] == null;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		internal virtual string ToString(int level)
		{
			return $"{Key} = {Value} :L{level}";
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

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private void AssertNotCircularRef(PairingNode<TNode, TKey, TValue> node)
		{
			if (!ReferenceEquals(this, node)) return;
			throw new InvalidOperationException("Circular reference detected.");
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class PairingNode<TKey, TValue> : PairingNode<PairingNode<TKey, TValue>, TKey, TValue>
	{
		private TKey _key;

		/// <inheritdoc />
		public PairingNode([NotNull] TKey key, [NotNull] TValue value)
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

	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class PairingNode<T> : PairingNode<PairingNode<T>, T, T>
	{
		/// <inheritdoc />
		public PairingNode([NotNull] T value)
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
			return $"{Value} :L{level}";
		}
	}
}
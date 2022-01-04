using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
[Serializable]
[DebuggerDisplay("{Value}")]
[StructLayout(LayoutKind.Sequential)]
public abstract class PairingNodeBase<TNode, TValue> : ISiblingNode<TNode, TValue>
	where TNode : PairingNodeBase<TNode, TValue>
{
	private const int CHILD = 0;
	private const int SIBLING = 1;
	private const int PREVIOUS = 2;

	private readonly TNode[] _nodes = new TNode[3];

	protected PairingNodeBase(TValue value)
	{
		Value = value;
	}

	public TValue Value { get; set; }

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

	public bool IsLeaf => _nodes[SIBLING] == null && _nodes[CHILD] == null;

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return Convert.ToString(Value); }

	[NotNull]
	public virtual string ToString(int level)
	{
		return $"{Value} :L{level}";
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
		Array.Clear(_nodes, 0, _nodes.Length);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private void AssertNotCircularRef(PairingNodeBase<TNode, TValue> node)
	{
		if (!ReferenceEquals(this, node)) return;
		throw new InvalidOperationException("Circular reference detected.");
	}

	public static implicit operator TValue([NotNull] PairingNodeBase<TNode, TValue> node) { return node.Value; }
}

/// <inheritdoc cref="PairingNodeBase{TNode,TValue}" />
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public sealed class PairingNode<T> : PairingNodeBase<PairingNode<T>, T>, ISiblingNode<PairingNode<T>, T>
{
	/// <inheritdoc />
	public PairingNode(T value)
		: base(value)
	{
	}

	public override string ToString(int level)
	{
		return $"{Value} :L{level}";
	}

	/// <inheritdoc />
	public override void Swap(PairingNode<T> other)
	{
		(other.Value, Value) = (Value, other.Value);
	}
}

/// <inheritdoc cref="PairingNodeBase{TNode,TValue}" />
[Serializable]
[DebuggerDisplay("{Key} = {Value}")]
[StructLayout(LayoutKind.Sequential)]
public sealed class PairingNode<TKey, TValue> : PairingNodeBase<PairingNode<TKey, TValue>, TValue>, ISiblingNode<PairingNode<TKey, TValue>, TKey, TValue>
{
	private TKey _key;

	/// <inheritdoc />
	public PairingNode([NotNull] TKey key, TValue value)
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
		return $"{Key} = {Value} :L{level}";
	}

	/// <inheritdoc />
	public override void Swap(PairingNode<TKey, TValue> other)
	{
		(other.Key, Key) = (Key, other.Key);
		(other.Value, Value) = (Value, other.Value);
	}
}
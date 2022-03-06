using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
[Serializable]
[DebuggerDisplay("{Value}")]
public abstract class BTreeNodeBase<TNode, T> : ITreeNode<TNode, T>
	where TNode : BTreeNodeBase<TNode, T>
{
	protected BTreeNodeBase(T value)
	{
		Value = value;
	}

	/// <inheritdoc />
	public T Value { get; set; }

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return Convert.ToString(Value); }

	[NotNull]
	public virtual string ToString(int level)
	{
		return $"{Value} :L{level}";
	}

	public abstract void Swap(TNode other);

	public static implicit operator T([NotNull] BTreeNodeBase<TNode, T> node) { return node.Value; }
}

/// <inheritdoc cref="BTreeNodeBase{TNode,T}" />
[Serializable]
public sealed class BTreeNode<T> : BTreeNodeBase<BTreeNode<T>, T>, ITreeNode<BTreeNode<T>, T>
{
	/// <inheritdoc />
	public BTreeNode(T value)
		: base(value)
	{
	}

	/// <inheritdoc />
	public override string ToString(int level)
	{
		return $"{Value} :L{level}";
	}

	/// <inheritdoc />
	public override void Swap(BTreeNode<T> other)
	{
		(other.Value, Value) = (Value, other.Value);
	}
}

/// <inheritdoc cref="BTreeNodeBase{TNode,T}" />
[Serializable]
[DebuggerDisplay("{Key} = {Value}")]
public sealed class BTreeNode<TKey, TValue> : BTreeNodeBase<BTreeNode<TKey, TValue>, TValue>, ITreeNode<BTreeNode<TKey, TValue>, TKey, TValue>
{
	private TKey _key;

	/// <inheritdoc />
	public BTreeNode([NotNull] TKey key, TValue value)
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
	public override void Swap(BTreeNode<TKey, TValue> other)
	{
		(other.Key, Key) = (Key, other.Key);
		(other.Value, Value) = (Value, other.Value);
	}
}

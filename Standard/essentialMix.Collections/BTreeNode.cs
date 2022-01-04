using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
[DebuggerDisplay("{Value}")]
[StructLayout(LayoutKind.Sequential)]
public abstract class BTreeNodeBase<TNode, T> : ILinkedBinaryNode<TKey, TValue>
	where TNode : BTreeNodeBase<TNode, T>
{
	protected BTreeNode(TValue value)
	{
		Value = value;
	}

	/// <inheritdoc />
	[NotNull]
	public abstract TKey Key { get; set; }

	/// <inheritdoc />
	public TValue Value { get; set; }

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return Convert.ToString(Value); }

	[NotNull]
	public virtual string ToString(int level)
	{
		return $"{Key} = {Value} :L{level}";
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public void Swap([NotNull] TNode other)
	{
		(other.Key, Key) = (Key, other.Key);
		(other.Value, Value) = (Value, other.Value);
	}

	public static implicit operator TValue([NotNull] BTreeNode<TNode, TKey, TValue> node) { return node.Value; }
}

[Serializable]
[DebuggerDisplay("{Key} = {Value}")]
[StructLayout(LayoutKind.Sequential)]
public sealed class BTreeNode<TKey, TValue> : BTreeNode<BTreeNode<TKey, TValue>, TKey, TValue>
{
	private TKey _key;

	public BTreeNode([NotNull] TKey key, TValue value)
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

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public sealed class BTreeNode<T> : BTreeNode<BTreeNode<T>, T, T>
{
	public BTreeNode(T value)
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

	public override string ToString(int level)
	{
		return $"{Value} :L{level}";
	}
}
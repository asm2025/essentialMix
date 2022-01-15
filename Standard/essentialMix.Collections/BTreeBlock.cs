using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace essentialMix.Collections;

/// <inheritdoc cref="ITreeBlock{TBlock,TNode,T}" />
[Serializable]
[DebuggerDisplay("{Degree}, Count = {Count}")]
[StructLayout(LayoutKind.Sequential)]
public abstract class BTreeBlockBase<TBlock, TNode, T> : List<TNode>, ITreeBlockBase<TBlock, TNode, T>
	where TBlock : class, ITreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	private List<TBlock> _children;

	protected BTreeBlockBase(int degree)
		: base(degree)
	{
		if (degree < 2) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
		Degree = degree;
	}

	/// <inheritdoc />
	public List<TBlock> Children
	{
		get => _children ??= new List<TBlock>();
		set => _children = value;
	}

	/// <inheritdoc />
	public int Degree { get; }

	/// <inheritdoc />
	public bool IsLeaf => _children == null || _children.Count == 0;

	/// <inheritdoc />
	public bool IsFull => Count >= 2 * Degree - 1;

	/// <inheritdoc />
	public bool IsEmpty => Count == 0;

	/// <inheritdoc />
	public bool HasMinimumEntries => Count >= Degree - 1;
}

/// <inheritdoc cref="BTreeBlockBase{TBlock, TNode, T}" />
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public sealed class BTreeBlock<T> : BTreeBlockBase<BTreeBlock<T>, BTreeNode<T>, T>, ITreeBlock<BTreeBlock<T>, BTreeNode<T>, T>
{
	/// <inheritdoc />
	public BTreeBlock(int degree)
		: base(degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<T> MakeNode(T value) { return new BTreeNode<T>(value); }
}

/// <inheritdoc cref="BTreeBlockBase{TBlock, TNode,T}" />
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public sealed class BTreeBlock<TKey, TValue> : BTreeBlockBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue>, ITreeBlock<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TKey, TValue>
{
	/// <inheritdoc />
	public BTreeBlock(int degree)
		: base(degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<TKey, TValue> MakeNode(TKey key, TValue value) { return new BTreeNode<TKey, TValue>(key, value); }
}

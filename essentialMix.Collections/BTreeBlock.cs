using System;

namespace essentialMix.Collections;

[Serializable]
public sealed class BTreeBlock<T> : BTreeBase<BTreeBlock<T>, BTreeNode<T>, T>.BTreeBlockBase, ITreeBlock<BTreeBlock<T>, BTreeNode<T>, T>
{
	/// <inheritdoc />
	internal BTreeBlock(BTreeBase<BTreeBlock<T>, BTreeNode<T>, T> tree, int degree)
		: base(tree, degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<T> MakeNode(T value) { return new BTreeNode<T>(value); }
}

[Serializable]
public sealed class BTreeBlock<TKey, TValue> : BTreeBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue>.BTreeBlockBase, ITreeBlock<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TKey, TValue>
{
	/// <inheritdoc />
	internal BTreeBlock(BTreeBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue> tree, int degree)
		: base(tree, degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<TKey, TValue> MakeNode(TKey key, TValue value) { return new BTreeNode<TKey, TValue>(key, value); }
}

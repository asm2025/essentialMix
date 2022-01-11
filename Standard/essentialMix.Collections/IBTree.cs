using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
public interface IBTreeBase<TBlock, TNode, T> : ICollection<TBlock>
	where TBlock : ITreeBlockBase<TBlock, TNode, T>
	where TNode : ITreeNode<TNode, T>
{
	[NotNull]
	TBlock Root { get; }

	[NotNull]
	TBlock MakeBlock();
}

/// <inheritdoc />
public interface IBTree<TBlock, TNode, T> : IBTreeBase<TBlock, TNode, T>
	where TBlock : ITreeBlock<TBlock, TNode, T>
	where TNode : ITreeNode<TNode, T>
{
}

/// <inheritdoc />
public interface IBTree<TBlock, TNode, TKey, TValue> : IBTreeBase<TBlock, TNode, TValue>
	where TBlock : ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : ITreeNode<TNode, TKey, TValue>
{
	[NotNull]
	IComparer<TKey> KeyComparer { get; }
}

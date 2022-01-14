using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
public interface ITreeBlockBase<TBlock, TNode, T> : IBoundList<TNode>
	where TBlock : ITreeBlockBase<TBlock, TNode, T>
	where TNode : ITreeNode<TNode, T>
{
	IBoundList<TBlock> Children { get; set; }
	int Degree { get; }
	bool IsLeaf { get; }
	bool IsFull { get; }
	bool IsEmpty { get; }
	bool HasMinimumEntries { get; }
}

/// <inheritdoc />
public interface ITreeBlock<TBlock, TNode, T> : ITreeBlockBase<TBlock, TNode, T>
	where TBlock : ITreeBlock<TBlock, TNode, T>
	where TNode : ITreeNode<TNode, T>
{
	[NotNull]
	TNode MakeNode([NotNull] T value);
}

public interface ITreeBlock<TBlock, TNode, TKey, TValue> : ITreeBlockBase<TBlock, TNode, TValue>
	where TBlock : ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : ITreeNode<TNode, TKey, TValue>
{
	[NotNull]
	TNode MakeNode([NotNull] TKey key, [CanBeNull] TValue value);
}
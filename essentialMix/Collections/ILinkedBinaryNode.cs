using JetBrains.Annotations;

namespace essentialMix.Collections;

public interface ILinkedBinaryNode<TNode, T> : ITreeNode<T>
	where TNode : ILinkedBinaryNode<TNode, T>
{
	TNode Left { get; }
	TNode Right { get; }
	bool IsLeaf { get; }
	bool IsNode { get; }
	bool HasOneChild { get; }
	bool IsFull { get; }
	TNode Predecessor();
	TNode Successor();
	TNode LeftMost();
	TNode RightMost();
	void Swap([NotNull] TNode other);
}

public interface ILinkedBinaryNode<TNode, TKey, TValue> : ILinkedBinaryNode<TNode, TValue>
	where TNode : ILinkedBinaryNode<TNode, TKey, TValue>
{
	TKey Key { get; set; }
}
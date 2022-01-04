using JetBrains.Annotations;

namespace essentialMix.Collections;

public interface ITreeNode<T> : INode<T>
{
	string ToString(int level);
}

public interface ITreeNode<TNode, T> : ITreeNode<T>
	where TNode : ITreeNode<TNode, T>
{
	void Swap([NotNull] TNode other);
}

public interface ITreeNode<TNode, TKey, TValue> : ITreeNode<TNode, TValue>
	where TNode : ITreeNode<TNode, TKey, TValue>
{
	TKey Key { get; set; }
}
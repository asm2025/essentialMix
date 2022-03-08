using System.Collections.Generic;
using essentialMix.Comparers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public interface IBTreeBase<TBlock, TNode, T> : ICollection<TNode>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	TBlock Root { get; }
	int Degree { get; }
	int Height { get; }

	void CopyTo([NotNull] TNode[] array);
	void CopyTo([NotNull] TNode[] array, int arrayIndex, int count);
	void EnsureRoot();
	[NotNull]
	TBlock MakeBlock();
	int Compare(TNode x, TNode y);
	bool Equal(TNode x, TNode y);
}

public interface IBTree<TBlock, TNode, T> : IBTreeBase<TBlock, TNode, T>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[NotNull]
	IGenericComparer<T> Comparer { get; }
	TNode Find(T item);
	[NotNull]
	TNode MakeNode([NotNull] T value);
}

public interface IBTree<TBlock, TNode, TKey, TValue> : IBTreeBase<TBlock, TNode, TValue>
	where TBlock : BTreeBlockBase<TBlock, TNode, TValue>, IBTreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
	[NotNull]
	IGenericComparer<TKey> Comparer { get; }
	TNode Find([NotNull] TKey key);
	[NotNull]
	TNode MakeNode([NotNull] TKey key, [CanBeNull] TValue value);
}

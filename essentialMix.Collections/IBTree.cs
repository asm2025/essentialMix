using System.Collections.Generic;
using essentialMix.Comparers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
public interface IBTreeBase<TBlock, TNode, T> : ICollection<TNode>
	where TBlock : class, ITreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	TBlock Root { get; }
	int Degree { get; }
	int Height { get; }

	void EnsureRoot();
	[NotNull]
	TBlock MakeBlock();
	int Compare(TNode x, TNode y);
	bool Equal(TNode x, TNode y);
}

/// <inheritdoc />
public interface IBTree<TBlock, TNode, T> : IBTreeBase<TBlock, TNode, T>
	where TBlock : class, ITreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[NotNull]
	IGenericComparer<T> Comparer { get; }
	void Add(T item);
	bool Remove(T item);
	TNode Find(T item);
	bool Contains(T item);
}

/// <inheritdoc />
public interface IBTree<TBlock, TNode, TKey, TValue> : IBTreeBase<TBlock, TNode, TValue>
	where TBlock : class, ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
	[NotNull]
	IGenericComparer<TKey> Comparer { get; }
	void Add([NotNull] TKey key, TValue value);
	bool Remove([NotNull] TKey key);
	TNode Find([NotNull] TKey key);
	bool Contains([NotNull] TKey key);
}

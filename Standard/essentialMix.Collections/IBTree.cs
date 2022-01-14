using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
public interface IBTreeBase<TBlock, TNode, T> : IBoundList<TNode>
	where TBlock : class, ITreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[NotNull]
	TBlock Root { get; }
	int Degree { get; }
	int Height { get; }

	[NotNull]
	TBlock MakeBlock();
	int Compare(TNode node1, TNode node2);
}

/// <inheritdoc />
public interface IBTree<TBlock, TNode, T> : IBTreeBase<TBlock, TNode, T>
	where TBlock : class, ITreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[NotNull]
	IComparer<T> Comparer { get; }
	void Insert(int index, T item);
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
	IComparer<TKey> KeyComparer { get; }
	[NotNull]
	IComparer<TValue> Comparer { get; }
	void Insert(int index, TKey key, TValue value);
	void Add(TKey key, TValue value);
	bool Remove(TKey key);
	TNode Find(TKey key);
	bool Contains(TKey key);
}

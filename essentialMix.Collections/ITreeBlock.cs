using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
public interface ITreeBlockBase<TBlock, TNode, T> : IList<TNode>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>, ITreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	BTreeCollection<TBlock, TNode, T, TBlock> Children { get; set; }
	int Degree { get; }
	bool IsLeaf { get; }
	bool IsFull { get; }
	bool IsEmpty { get; }
	bool HasMinimumEntries { get; }

	void EnsureChildren();
	int IndexOf(TNode item, int index);
	int IndexOf(TNode item, int index, int count);
	int LastIndexOf(TNode item);
	int LastIndexOf(TNode item, int index);
	int LastIndexOf(TNode item, int index, int count);
	TNode Find([NotNull] Predicate<TNode> match);
	TNode FindLast([NotNull] Predicate<TNode> match);
	int FindIndex([NotNull] Predicate<TNode> match);
	int FindIndex(int startIndex, [NotNull] Predicate<TNode> match);
	int FindLastIndex([NotNull] Predicate<TNode> match);
	int FindLastIndex(int startIndex, [NotNull] Predicate<TNode> match);
	int FindLastIndex(int startIndex, int count, [NotNull] Predicate<TNode> match);
}

/// <inheritdoc />
public interface ITreeBlock<TBlock, TNode, T> : ITreeBlockBase<TBlock, TNode, T>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>, ITreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[NotNull]
	TNode MakeNode([NotNull] T value);
}

public interface ITreeBlock<TBlock, TNode, TKey, TValue> : ITreeBlockBase<TBlock, TNode, TValue>
	where TBlock : BTreeBlockBase<TBlock, TNode, TValue>, ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
	[NotNull]
	TNode MakeNode([NotNull] TKey key, [CanBeNull] TValue value);
}
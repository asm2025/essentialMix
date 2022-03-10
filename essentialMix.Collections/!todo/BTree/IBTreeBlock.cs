using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
public interface IBTreeBlockBase<TBlock, TNode, T> : IList<TNode>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>, IBTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	BTreeBlockCollection<TBlock, TNode, T> Children { get; set; }
	int Degree { get; }
	bool IsLeaf { get; }
	bool IsFull { get; }
	bool IsEmpty { get; }
	bool HasMinimumEntries { get; }

	IEnumerableEnumerator<T> Enumerate();
	IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method);
	IEnumerableEnumerator<T> Enumerate(bool rightToLeft);
	IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method, bool rightToLeft);

	IEnumerableEnumerator<TNode> EnumerateNodes();
	IEnumerableEnumerator<TNode> EnumerateNodes(TreeTraverseMethod method);
	IEnumerableEnumerator<TNode> EnumerateNodes(bool rightToLeft);
	IEnumerableEnumerator<TNode> EnumerateNodes(TreeTraverseMethod method, bool rightToLeft);

	IEnumerableEnumerator<TBlock> EnumerateBlocks();
	IEnumerableEnumerator<TBlock> EnumerateBlocks(TreeTraverseMethod method);
	IEnumerableEnumerator<TBlock> EnumerateBlocks(bool rightToLeft);
	IEnumerableEnumerator<TBlock> EnumerateBlocks(TreeTraverseMethod method, bool rightToLeft);

	void Iterate([NotNull] Action<T> visitCallback);
	void Iterate(TreeTraverseMethod method, [NotNull] Action<T> visitCallback);
	void Iterate(bool rightToLeft, [NotNull] Action<T> visitCallback);
	void Iterate(TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<T> visitCallback);

	void IterateNodes([NotNull] Action<TNode> visitCallback);
	void IterateNodes(TreeTraverseMethod method, [NotNull] Action<TNode> visitCallback);
	void IterateNodes(bool rightToLeft, [NotNull] Action<TNode> visitCallback);
	void IterateNodes(TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<TNode> visitCallback);

	void IterateBlocks([NotNull] Action<TBlock> visitCallback);
	void IterateBlocks(TreeTraverseMethod method, [NotNull] Action<TBlock> visitCallback);
	void IterateBlocks(bool rightToLeft, [NotNull] Action<TBlock> visitCallback);
	void IterateBlocks(TreeTraverseMethod method, bool rightToLeft, [NotNull] Action<TBlock> visitCallback);

	void Iterate([NotNull] Func<T, bool> visitCallback);
	void Iterate(TreeTraverseMethod method, [NotNull] Func<T, bool> visitCallback);
	void Iterate(bool rightToLeft, [NotNull] Func<T, bool> visitCallback);
	void Iterate(TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<T, bool> visitCallback);

	void IterateNodes([NotNull] Func<TNode, bool> visitCallback);
	void IterateNodes(TreeTraverseMethod method, [NotNull] Func<TNode, bool> visitCallback);
	void IterateNodes(bool rightToLeft, [NotNull] Func<TNode, bool> visitCallback);
	void IterateNodes(TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<TNode, bool> visitCallback);

	void IterateBlocks([NotNull] Func<TBlock, bool> visitCallback);
	void IterateBlocks(TreeTraverseMethod method, [NotNull] Func<TBlock, bool> visitCallback);
	void IterateBlocks(bool rightToLeft, [NotNull] Func<TBlock, bool> visitCallback);
	void IterateBlocks(TreeTraverseMethod method, bool rightToLeft, [NotNull] Func<TBlock, bool> visitCallback);

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
public interface IBTreeBlock<TBlock, TNode, T> : IBTreeBlockBase<TBlock, TNode, T>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>, IBTreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
}

public interface IBTreeBlock<TBlock, TNode, TKey, TValue> : IBTreeBlockBase<TBlock, TNode, TValue>
	where TBlock : BTreeBlockBase<TBlock, TNode, TValue>, IBTreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
}
using System;
using System.Collections.Generic;
using essentialMix.Comparers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public interface IBTreeBase<TBlock, TNode, T> : ICollection<T>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	TBlock Root { get; }
	int Degree { get; }
	int Height { get; }

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

	void Add(TNode node);
	bool Remove(TNode node);
	bool Contains(TNode node);
	void CopyTo([NotNull] T[] array);
	void CopyTo([NotNull] T[] array, int arrayIndex, int count);
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

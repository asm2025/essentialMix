using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Collections.DebugView;

namespace essentialMix.Collections;

/// <summary>
/// <see href="https://en.wikipedia.org/wiki/B-tree">B-tree</see> using the linked representation.
/// See a breif overview at <see href="https://www.youtube.com/watch?v=aZjYr87r1b8">10.2  B Trees and B+ Trees. How they are useful in Databases</see>.
/// <para>Based on BTree chapter in "Introduction to Algorithms", by Thomas Cormen, Charles Leiserson, Ronald Rivest.</para>
/// <para>This uses the same abstract pattern as <see cref="LinkedBinaryTree{TNode,T}"/></para>
/// </summary>
/// <typeparam name="TBlock">The block type. Must implement <see cref="ITreeBlock{TBlock, TNode, T}"/></typeparam>
/// <typeparam name="TNode">The node type. Must implement <see cref="ITreeNode{TNode, T}"/></typeparam>
/// <typeparam name="T">The element type of the tree</typeparam>
/*
* A B-tree of order m is a tree which satisfies the following properties:
* 1. The nodes in a B-tree of order m can have a maximum of m children.
* 2. Each internal node (non-leaf and non-root) can have at least (m/2) children.
* 3. The root has at least two children if it is not a leaf node.
* 4. A non-leaf node with k children contains k − 1 keys.
* 5. All leaves appear in the same level and carry no information.
*
* For a B-Tree of order 3
*	     ___ D ____
*	    /          \
*	   B            F H
*	 /   \        /  |  \
*	A     C      E   G   I J
*
*	    BFS                  DFS                  DFS                   DFS
*	  LevelOrder           PreOrder              InOrder              PostOrder
*	    1 ___                 1 ___                 4 ___                   ___
*	   /     \               /     \               /     \               /     \
*	  2       3 4           2       5             2       6 8                      
*	 /  \    / |  \        /  \    / |  \        /  \    / |  \        /  \    / |  \
*	5    6  7  8   9 10   3    4  6             1    3  5  7   9 10                      
*
* BFS (LevelOrder): DBFHACEGIJ => Root-Left-Right (Queue)
* DFS [PreOrder]:    => Root-Left-Right (Stack)
* DFS [InOrder]:    ABCDEFGHIJ => Left-Root-Right (Stack)
* DFS [PostOrder]:   => Left-Right-Root (Stack)
*/
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_BTreeDebugView<,,>))]
public abstract class BTreeBase<TBlock, TNode, T> : IBTreeBase<TBlock, TNode, T>
	where TBlock : class, ITreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	private TBlock _root;

	protected BTreeBase(int degree)
	{
		if (degree < 2) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
		Degree = degree;
	}

	/// <inheritdoc />
	public TNode this[int index]
	{
		get => Root[index];
		set => Root[index] = value;
	}

	/// <inheritdoc />
	public int Capacity
	{
		get => Root.Capacity;
		set => Root.Capacity = value;
	}

	/// <inheritdoc />
	public int Limit => Root.Limit;

	/// <inheritdoc />
	public TBlock Root => _root ??= MakeBlock();

	/// <inheritdoc />
	public int Degree { get; }

	/// <inheritdoc />
	public int Height { get; private set; }

	/// <inheritdoc />
	public int Count => Root.Count;

	/// <inheritdoc />
	public bool IsReadOnly => Root.IsReadOnly;

	/// <inheritdoc />
	public abstract TBlock MakeBlock();

	/// <inheritdoc />
	public abstract int Compare(TNode node1, TNode node2);

	/// <inheritdoc />
	public IEnumerator<TNode> GetEnumerator() { return Root.GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public void Insert(int index, TNode item) { Root.Insert(index, item); }

	/// <inheritdoc />
	public void Add(TNode item) { Root.Add(item); }

	/// <inheritdoc />
	public void RemoveAt(int index) { Root.RemoveAt(index); }

	/// <inheritdoc />
	public bool Remove(TNode item) { return Root.Remove(item); }

	/// <inheritdoc />
	public void Clear() { Root.Clear(); }

	/// <inheritdoc />
	public int IndexOf(TNode item) { return Root.IndexOf(item); }

	/// <inheritdoc />
	public bool Contains(TNode item) { return Root.Contains(item); }

	/// <inheritdoc />
	public void CopyTo(TNode[] array, int arrayIndex) { Root.CopyTo(array, arrayIndex); }
}

/// <inheritdoc cref="BTreeBase{TBlock, TNode, T}" />
/// <typeparam name="TBlock">The block type. Must implement <see cref="ITreeBlock{TBlock, TNode, T}"/></typeparam>
/// <typeparam name="TNode">The node type. Must implement <see cref="ITreeNode{TNode, T}"/></typeparam>
/// <typeparam name="T">The element type of the tree</typeparam>
[Serializable]
public abstract class BTree<TBlock, TNode, T> : BTreeBase<TBlock, TNode, T>, IBTree<TBlock, TNode, T>
	where TBlock : class, ITreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	/// <inheritdoc />
	protected BTree(int degree, IComparer<T> comparer)
		: base(degree)
	{
		Comparer = comparer ?? Comparer<T>.Default;
	}

	/// <inheritdoc />
	public IComparer<T> Comparer { get; }

	/// <inheritdoc />
	public void Insert(int index, T item) { TODO_IMPLEMENT_ME(); }

	/// <inheritdoc />
	public void Add(T item) { TODO_IMPLEMENT_ME(); }

	/// <inheritdoc />
	public bool Remove(T item) { return TODO_IMPLEMENT_ME; }

	/// <inheritdoc />
	public TNode Find(T item) { return TODO_IMPLEMENT_ME; }

	/// <inheritdoc />
	public bool Contains(T item) { return TODO_IMPLEMENT_ME; }
}

/// <inheritdoc cref="BTreeBase{TBlock, TNode, T}" />
/// <typeparam name="TBlock">The block type. Must implement <see cref="ITreeBlock{TBlock, TNode, TKey, T}"/></typeparam>
/// <typeparam name="TNode">The node type. Must implement <see cref="ITreeNode{TNode, TKey, TValue}"/></typeparam>
/// <typeparam name="TKey">The key type of the tree</typeparam>
/// <typeparam name="TValue">The element type of the tree</typeparam>
[Serializable]
[DebuggerTypeProxy(typeof(Dbg_BTreeDebugView<,,,>))]
public abstract class BTree<TBlock, TNode, TKey, TValue> : BTreeBase<TBlock, TNode, TValue>, IBTree<TBlock, TNode, TKey, TValue>
	where TBlock : class, ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
	/// <inheritdoc />
	protected BTree(int degree, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
		: base(degree)
	{
		KeyComparer = keyComparer ?? Comparer<TKey>.Default;
		Comparer = comparer ?? Comparer<TValue>.Default;
	}

	/// <inheritdoc />
	public IComparer<TKey> KeyComparer { get; }

	/// <inheritdoc />
	public IComparer<TValue> Comparer { get; }

	/// <inheritdoc />
	public void Insert(int index, TKey key, TValue value) { TODO_IMPLEMENT_ME(); }

	/// <inheritdoc />
	public void Add(TKey key, TValue value) { TODO_IMPLEMENT_ME(); }

	/// <inheritdoc />
	public bool Remove(TKey key) { return TODO_IMPLEMENT_ME; }

	/// <inheritdoc />
	public TNode Find(TKey key) { return TODO_IMPLEMENT_ME; }

	/// <inheritdoc />
	public bool Contains(TKey key) { return TODO_IMPLEMENT_ME; }
}

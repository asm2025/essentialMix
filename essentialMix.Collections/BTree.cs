using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Comparers;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/*
* A B-tree of order m is a tree which satisfies the following properties:
* 1. The nodes in a B-tree of order m can have a maximum of m children.
* 2. Each internal node (non-leaf and non-root) can have at least (m/2) children.
* 3. The root has at least two children if it is not a leaf node.
* 4. A non-leaf node with k children contains k − 1 keys.
* 5. All leaves appear in the same level and carry no information.
*
* For a B-Tree of order 3
*	     ___ D ______
*	    /          \ \
*	   B            F H __
*	 /   \        /  |  \ \
*	A     C      E   G   I J
*
*	    BFS                  DFS                  DFS                   DFS
*	  LevelOrder           PreOrder              InOrder              PostOrder
*	    1 _____               1 _____                4 _____               10 ____
*	   /     \ \             /     \ \              /     \ \             /     \ \
*	  2       3 4 __        2       5 8 __         2       6 8 __        3       6 9 __
*	 /  \    / |  \ \      /  \    / |  \ \       /  \    / |  \ \      /  \    / |  \ \
*	5    6  7  8   9 10   3    4  6  7  9  10    1    3  5  7   9 10   1    2  4  5   7 8
*
* BFS (LevelOrder): DBFHACEGIJ => Root-Left-Right (Queue)
* DFS [PreOrder]:   DBACFEGHIJ => Root-Left-Right (Stack)
* DFS [InOrder]:    ABCDEFGHIJ => Left-Root-Right (Stack)
* DFS [PostOrder]:  ACBEGFIJHD => Left-Right-Root (Stack)
*/
/// <summary>
/// <see href="https://en.wikipedia.org/wiki/B-tree">B-tree</see> using the linked representation.
/// See a brief overview at <see href="https://www.youtube.com/watch?v=aZjYr87r1b8">10.2  B Trees and B+ Trees. How they are useful in Databases</see>.
/// <para>Based on BTree chapter in "Introduction to Algorithms", by Thomas Cormen, Charles Leiserson, Ronald Rivest.</para>
/// <para>This uses the same abstract pattern as <see cref="LinkedBinaryTree{TNode,T}"/></para>
/// </summary>
/// <typeparam name="TBlock">The block type. Must implement <see cref="ITreeBlock{TBlock, TNode, T}"/></typeparam>
/// <typeparam name="TNode">The node type. Must implement <see cref="ITreeNode{TNode, T}"/></typeparam>
/// <typeparam name="T">The element type of the tree</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_BTreeDebugView<,,>))]
public abstract class BTreeBase<TBlock, TNode, T> : IBTreeBase<TBlock, TNode, T>, ICollection
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	internal int _version;

	private TBlock _root;

	[NonSerialized]
	private object _syncRoot;

	protected BTreeBase(int degree)
	{
		if (degree < BTree.MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
		Degree = degree;
		Height = 1;
	}

	/// <inheritdoc />
	public TBlock Root
	{
		get => _root;
		protected set => _root = value;
	}

	/// <inheritdoc />
	public int Degree { get; }

	/// <inheritdoc />
	public int Height { get; protected set; }

	public int Count { get; internal set; }

	/// <inheritdoc />
	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public void EnsureRoot() { _root ??= MakeBlock(); }

	/// <inheritdoc />
	public abstract TBlock MakeBlock();

	/// <inheritdoc />
	public abstract int Compare(TNode x, TNode y);

	/// <inheritdoc />
	public abstract bool Equal(TNode x, TNode y);

	/// <inheritdoc />
	public IEnumerator<TNode> GetEnumerator() { return _root?.GetEnumerator() ?? Enumerable.Empty<TNode>().GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public abstract void Add(TNode node);

	/// <inheritdoc />
	public abstract bool Remove(TNode node);

	/// <inheritdoc />
	public void Clear()
	{
		_root?.Clear();
	}

	/// <inheritdoc />
	public abstract bool Contains(TNode node);

	public void CopyTo([NotNull] TNode[] array) { CopyTo(array, 0); }
	public void CopyTo(TNode[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
	public void CopyTo([NotNull] TNode[] array, int arrayIndex, int count)
	{
		array.Length.ValidateRange(arrayIndex, ref count);
		if (count == 0) return;
		Count.ValidateRange(arrayIndex, ref count);
		if (_root == null) return;

		while (count > 0)
		{
			// todo
			count--;
		}
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array.Rank != 1) throw new RankException();
		if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
		if (Count == 0) return;

		if (array is TNode[] tArray)
		{
			CopyTo(tArray, arrayIndex);
			return;
		}

		/*
		* Catch the obvious case assignment will fail.
		* We can find all possible problems by doing the check though.
		* For example, if the element type of the Array is derived from T,
		* we can't figure out if we can successfully copy the element beforehand.
		*/
		array.Length.ValidateRange(arrayIndex, Count);

		Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
		Type sourceType = typeof(T);
		if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
		if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));

		try
		{
			foreach (TNode item in this)
			{
				objects[arrayIndex++] = item;
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Invalid array type", nameof(array));
		}
	}

	protected void Split([NotNull] TBlock parent, int index, [NotNull] TBlock block)
	{
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

		TBlock newBlock = MakeBlock();
		parent.Insert(index, block[Degree - 1]);
		parent.Children.Insert(index + 1, newBlock);
		newBlock.AddRange(block.GetRange(Degree, Degree - 1));
		block.RemoveRange(Degree - 1, Degree);
		if (block.IsLeaf) return;
		newBlock.Children.AddRange(block.Children.GetRange(Degree, Degree));
		block.Children.RemoveRange(Degree, Degree);
	}
}

/// <inheritdoc cref="BTreeBase{TBlock, TNode, T}" />
/// <typeparam name="TBlock">The block type. Must implement <see cref="ITreeBlock{TBlock, TNode, T}"/></typeparam>
/// <typeparam name="TNode">The node type. Must implement <see cref="ITreeNode{TNode, T}"/></typeparam>
/// <typeparam name="T">The element type of the tree</typeparam>
[Serializable]
public abstract class BTree<TBlock, TNode, T> : BTreeBase<TBlock, TNode, T>, IBTree<TBlock, TNode, T>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>, ITreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	/// <inheritdoc />
	protected BTree(int degree)
		: this(degree, null)
	{
	}

	/// <inheritdoc />
	protected BTree(int degree, IGenericComparer<T> comparer)
		: base(degree)
	{
		Comparer = comparer ?? GenericComparer<T>.Default;
	}

	/// <inheritdoc />
	public IGenericComparer<T> Comparer { get; }

	/// <inheritdoc />
	public sealed override void Add(TNode node)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));
		Add(node.Value);
	}

	/// <inheritdoc />
	public void Add(T item)
	{
		EnsureRoot();

		if (!Root.IsFull)
		{
			Add(Root, item);
			return;
		}

		TBlock oldRoot = Root;
		Root = MakeBlock();
		Root.EnsureChildren();
		Root.Children.Add(oldRoot);
		Split(Root, 0, oldRoot);
		Add(Root, item);
		Height++;
	}

	/// <inheritdoc />
	public sealed override bool Remove(TNode node) { return node != null && Remove(node.Value); }
	/// <inheritdoc />
	public bool Remove(T item)
	{
		if (Root == null || !Remove(Root, item)) return false;
		if (Root.Count > 0 || Root.IsLeaf) return true;
		Root = Root.Children[0];
		Height--;
		return true;
	}

	/// <inheritdoc />
	public TNode Find(T item)
	{
		return Root == null
					? null
					: FindLocal(Root, item, Comparer);

		static TNode FindLocal(TBlock block, T item, IGenericComparer<T> comparer)
		{
			int position = block.Count(e => comparer.Compare(item, e.Value) > 0);
			if (position < block.Count && comparer.Equals(item, block[position].Value)) return block[position];
			return block.IsLeaf
						? null
						: FindLocal(block.Children[position], item, comparer);
		}
	}

	/// <inheritdoc />
	public sealed override bool Contains(TNode node) { return node != null && Contains(node.Value); }
	/// <inheritdoc />
	public bool Contains(T item) { return Find(item) != null; }

	/// <inheritdoc />
	public override int Compare(TNode x, TNode y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(x, null)) return 1;
		if (ReferenceEquals(y, null)) return -1;
		return Comparer.Compare(x.Value, y.Value);
	}

	/// <inheritdoc />
	public override bool Equal(TNode x, TNode y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
		return Comparer.Equals(x.Value, y.Value);
	}

	protected void Add([NotNull] TBlock block, T item)
	{
		int position = block.Count == 0
							? 0
							: block.FindLastIndex(e => Comparer.Compare(item, e.Value) >= 0) + 1;

		while (!block.IsLeaf)
		{
			TBlock child = block.Children[position];

			if (child.IsFull)
			{
				Split(block, position, child);
				if (Comparer.Compare(item, block[position].Value) > 0) position++;
				block = block.Children[position];
			}

			position = block.Count == 0
							? 0
							: block.FindLastIndex(e => Comparer.Compare(item, e.Value) >= 0) + 1;
		}

		block.Insert(position, block.MakeNode(item));
	}

	protected bool Remove([NotNull] TBlock block, T item)
	{
		return RemoveItem(block, item, Comparer);

		static bool RemoveItem(TBlock block, T item, IGenericComparer<T> comparer)
		{
			if (block.Count == 0) return false;

			int position = block.Count(e => comparer.Compare(item, e.Value) > 0);
			return position < block.Count && comparer.Equals(item, block[position].Value)
						? RemoveFromBlock(block, item, position, comparer)
						: !block.IsLeaf && RemoveFromSubTree(block, item, position, comparer);
		}

		static bool RemoveFromSubTree(TBlock root, T item, int subtreeIndexInBlock, IGenericComparer<T> comparer)
		{
			TBlock block = root.Children[subtreeIndexInBlock];
			if (!block.HasMinimumEntries) return RemoveItem(block, item, comparer);

			/*
			 * Removing any item from the block will break the btree property, So this block must have
			 * at least 'degree' of nodes by moving an item from a sibling block or merging nodes.
			 */
			int leftIndex = subtreeIndexInBlock - 1;
			TBlock leftSibling = subtreeIndexInBlock > 0
									? root.Children[leftIndex]
									: null;
			int rightIndex = subtreeIndexInBlock + 1;
			TBlock rightSibling = subtreeIndexInBlock < root.Children.Count - 1
									? root.Children[rightIndex]
									: null;

			if (leftSibling != null && leftSibling.Count > root.Degree - 1)
			{
				/*
				 * left sibling has a node to spare, so move one node from it into the parent's block
				 * and one node from parent into this block.
				 */
				block.Insert(0, root[subtreeIndexInBlock]);
				root[subtreeIndexInBlock] = leftSibling[leftSibling.Count - 1];
				leftSibling.RemoveAt(leftSibling.Count - 1);
				if (leftSibling.IsLeaf) return RemoveItem(block, item, comparer);
				block.Children.Insert(0, leftSibling.Children[leftSibling.Children.Count - 1]);
				leftSibling.Children.RemoveAt(leftSibling.Children.Count - 1);
			}
			else if (rightSibling != null && rightSibling.Count > root.Degree - 1)
			{
				/*
				 * right sibling has a node to spare, so move one node from it into the parent's block
				 * and one node from parent into this block.
				 */
				block.Add(root[subtreeIndexInBlock]);
				root[subtreeIndexInBlock] = rightSibling[0];
				rightSibling.RemoveAt(0);
				if (rightSibling.IsLeaf) return RemoveItem(block, item, comparer);
				block.Children.Add(rightSibling.Children[0]);
				rightSibling.Children.RemoveAt(0);
			}
			else if (leftSibling != null)
			{
				// this block merges left sibling into this block
				block.Insert(0, root[subtreeIndexInBlock]);
				block.InsertRange(0, leftSibling);
				if (!leftSibling.IsLeaf) block.Children.InsertRange(0, leftSibling.Children);
				root.RemoveAt(leftIndex);
				root.RemoveAt(subtreeIndexInBlock);
			}
			else
			{
				Debug.Assert(rightSibling != null, "Block should have at least one sibling.");
				block.Add(root[subtreeIndexInBlock]);
				block.AddRange(rightSibling);
				if (!rightSibling.IsLeaf) block.Children.AddRange(rightSibling.Children);
				root.Children.RemoveAt(rightIndex);
				root.RemoveAt(subtreeIndexInBlock);
			}

			/*
			 * At this point, block has at least 'degree' of nodes. This guarantees that if any node needs to be
			 * removed from it in order to guarantee BTree's property.
			 */
			return RemoveItem(block, item, comparer);
		}

		static bool RemoveFromBlock(TBlock block, T item, int position, IGenericComparer<T> comparer)
		{
			if (block.IsLeaf)
			{
				// just remove it and move on. BTree property is maintained.
				block.RemoveAt(position);
				return true;
			}

			TBlock predecessor = block.Children[position];

			if (predecessor.Count > block.Degree)
			{
				block[position] = DeletePredecessor(predecessor);
				return true;
			}

			TBlock successor = block.Children[position + 1];

			if (successor.Count >= block.Degree)
			{
				block[position] = DeleteSuccessor(predecessor);
				return true;
			}

			predecessor.Add(block[position]);
			predecessor.AddRange(successor);
			predecessor.Children.AddRange(successor.Children);
			block.RemoveAt(position);
			block.Children.RemoveAt(position + 1);
			return RemoveItem(predecessor, item, comparer);
		}

		static TNode DeletePredecessor(TBlock block)
		{
			while (!block.IsLeaf)
			{
				block = block.Children[block.Children.Count - 1];
			}

			TNode node = block[block.Count - 1];
			block.RemoveAt(block.Count - 1);
			return node;
		}

		static TNode DeleteSuccessor(TBlock block)
		{
			while (!block.IsLeaf)
			{
				block = block.Children[0];
			}

			TNode node = block[0];
			block.RemoveAt(0);
			return node;
		}
	}
}

/// <inheritdoc cref="BTreeBase{TBlock, TNode, T}" />
/// <typeparam name="TBlock">The block type. Must implement <see cref="ITreeBlock{TBlock, TNode, TKey, T}"/></typeparam>
/// <typeparam name="TNode">The node type. Must implement <see cref="ITreeNode{TNode, TKey, TValue}"/></typeparam>
/// <typeparam name="TKey">The key type of the tree</typeparam>
/// <typeparam name="TValue">The element type of the tree</typeparam>
[Serializable]
[DebuggerTypeProxy(typeof(Dbg_BTreeDebugView<,,,>))]
public abstract class BTree<TBlock, TNode, TKey, TValue> : BTreeBase<TBlock, TNode, TValue>, IBTree<TBlock, TNode, TKey, TValue>
	where TBlock : BTreeBlockBase<TBlock, TNode, T>, ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
	/// <inheritdoc />
	protected BTree(int degree)
		: this(degree, null)
	{
	}

	/// <inheritdoc />
	protected BTree(int degree, IGenericComparer<TKey> comparer)
		: base(degree)
	{
		Comparer = comparer ?? GenericComparer<TKey>.Default;
	}

	/// <inheritdoc />
	public IGenericComparer<TKey> Comparer { get; }

	/// <inheritdoc />
	public sealed override void Add(TNode node)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));
		Add(node.Key, node.Value);
	}

	/// <inheritdoc />
	public void Add(TKey key, TValue value)
	{
		EnsureRoot();

		if (!Root.IsFull)
		{
			Add(Root, key, value);
			return;
		}

		TBlock oldRoot = Root;
		Root = MakeBlock();
		Root.EnsureChildren();
		Root.Children.Add(oldRoot);
		Split(Root, 0, oldRoot);
		Add(Root, key, value);
		Height++;
	}

	/// <inheritdoc />
	public sealed override bool Remove(TNode node) { return node != null && Remove(node.Key); }
	/// <inheritdoc />
	public bool Remove(TKey key)
	{
		if (Root == null || !Remove(Root, key)) return false;
		if (Root.Count > 0 || Root.IsLeaf) return true;
		Root = Root.Children[0];
		Height--;
		return true;
	}

	/// <inheritdoc />
	public TNode Find(TKey key)
	{
		return Root == null
					? null
					: FindLocal(Root, key, Comparer);

		static TNode FindLocal(TBlock block, TKey key, IGenericComparer<TKey> comparer)
		{
			int position = block.Count(e => comparer.Compare(key, e.Value) > 0);
			if (position < block.Count && comparer.Equals(key, block[position].Value)) return block[position];
			return block.IsLeaf
						? null
						: FindLocal(block.Children[position], key, comparer);
		}
	}

	/// <inheritdoc />
	public sealed override bool Contains(TNode node) { return node != null && Contains(node.Key); }
	/// <inheritdoc />
	public bool Contains(TKey key) { return Find(key) != null; }

	/// <inheritdoc />
	public override int Compare(TNode x, TNode y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(x, null)) return 1;
		if (ReferenceEquals(y, null)) return -1;
		return Comparer.Compare(x.Key, y.Key);
	}

	/// <inheritdoc />
	public override bool Equal(TNode x, TNode y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
		return Comparer.Equals(x.Key, y.Key);
	}

	protected void Add([NotNull] TBlock block, TKey key, TValue value)
	{
		int position = block.Count == 0
							? 0
							: block.FindLastIndex(e => Comparer.Compare(key, e.Key) >= 0) + 1;

		while (!block.IsLeaf)
		{
			TBlock child = block.Children[position];

			if (child.IsFull)
			{
				Split(block, position, child);
				if (Comparer.Compare(key, block[position].Key) > 0) position++;
				block = block.Children[position];
			}

			position = block.Count == 0
							? 0
							: block.FindLastIndex(e => Comparer.Compare(key, e.Key) >= 0) + 1;
		}

		block.Insert(position, block.MakeNode(key, value));
	}

	protected bool Remove([NotNull] TBlock block, TKey key)
	{
		return RemoveItem(block, key, Comparer);

		static bool RemoveItem(TBlock block, TKey key, IGenericComparer<TKey> comparer)
		{
			if (block.Count == 0) return false;

			int position = block.Count(e => comparer.Compare(key, e.Value) > 0);
			return position < block.Count && comparer.Equals(key, block[position].Value)
						? RemoveFromBlock(block, key, position, comparer)
						: !block.IsLeaf && RemoveFromSubTree(block, key, position, comparer);
		}

		static bool RemoveFromSubTree(TBlock root, TKey key, int subtreeIndexInBlock, IGenericComparer<TKey> comparer)
		{
			TBlock block = root.Children[subtreeIndexInBlock];
			if (!block.HasMinimumEntries) return RemoveItem(block, key, comparer);

			/*
			 * Removing any item from the block will break the btree property, So this block must have
			 * at least 'degree' of nodes by moving an item from a sibling block or merging nodes.
			 */
			int leftIndex = subtreeIndexInBlock - 1;
			TBlock leftSibling = subtreeIndexInBlock > 0
									? root.Children[leftIndex]
									: null;
			int rightIndex = subtreeIndexInBlock + 1;
			TBlock rightSibling = subtreeIndexInBlock < root.Children.Count - 1
									? root.Children[rightIndex]
									: null;

			if (leftSibling != null && leftSibling.Count > root.Degree - 1)
			{
				/*
				 * left sibling has a node to spare, so move one node from it into the parent's block
				 * and one node from parent into this block.
				 */
				block.Insert(0, root[subtreeIndexInBlock]);
				root[subtreeIndexInBlock] = leftSibling[leftSibling.Count - 1];
				leftSibling.RemoveAt(leftSibling.Count - 1);
				if (leftSibling.IsLeaf) return RemoveItem(block, key, comparer);
				block.Children.Insert(0, leftSibling.Children[leftSibling.Children.Count - 1]);
				leftSibling.Children.RemoveAt(leftSibling.Children.Count - 1);
			}
			else if (rightSibling != null && rightSibling.Count > root.Degree - 1)
			{
				/*
				 * right sibling has a node to spare, so move one node from it into the parent's block
				 * and one node from parent into this block.
				 */
				block.Add(root[subtreeIndexInBlock]);
				root[subtreeIndexInBlock] = rightSibling[0];
				rightSibling.RemoveAt(0);
				if (rightSibling.IsLeaf) return RemoveItem(block, key, comparer);
				block.Children.Add(rightSibling.Children[0]);
				rightSibling.Children.RemoveAt(0);
			}
			else if (leftSibling != null)
			{
				// this block merges left sibling into this block
				block.Insert(0, root[subtreeIndexInBlock]);
				block.InsertRange(0, leftSibling);
				if (!leftSibling.IsLeaf) block.Children.InsertRange(0, leftSibling.Children);
				root.RemoveAt(leftIndex);
				root.RemoveAt(subtreeIndexInBlock);
			}
			else
			{
				Debug.Assert(rightSibling != null, "Block should have at least one sibling.");
				block.Add(root[subtreeIndexInBlock]);
				block.AddRange(rightSibling);
				if (!rightSibling.IsLeaf) block.Children.AddRange(rightSibling.Children);
				root.Children.RemoveAt(rightIndex);
				root.RemoveAt(subtreeIndexInBlock);
			}

			/*
			 * At this point, block has at least 'degree' of nodes. This guarantees that if any node needs to be
			 * removed from it in order to guarantee BTree's property.
			 */
			return RemoveItem(block, key, comparer);
		}

		static bool RemoveFromBlock(TBlock block, TKey key, int position, IGenericComparer<TKey> comparer)
		{
			if (block.IsLeaf)
			{
				// just remove it and move on. BTree property is maintained.
				block.RemoveAt(position);
				return true;
			}

			TBlock predecessor = block.Children[position];

			if (predecessor.Count > block.Degree)
			{
				block[position] = DeletePredecessor(predecessor);
				return true;
			}

			TBlock successor = block.Children[position + 1];

			if (successor.Count >= block.Degree)
			{
				block[position] = DeleteSuccessor(predecessor);
				return true;
			}

			predecessor.Add(block[position]);
			predecessor.AddRange(successor);
			predecessor.Children.AddRange(successor.Children);
			block.RemoveAt(position);
			block.Children.RemoveAt(position + 1);
			return RemoveItem(predecessor, key, comparer);
		}

		static TNode DeletePredecessor(TBlock block)
		{
			while (!block.IsLeaf)
			{
				block = block.Children[block.Children.Count - 1];
			}

			TNode node = block[block.Count - 1];
			block.RemoveAt(block.Count - 1);
			return node;
		}

		static TNode DeleteSuccessor(TBlock block)
		{
			while (!block.IsLeaf)
			{
				block = block.Children[0];
			}

			TNode node = block[0];
			block.RemoveAt(0);
			return node;
		}
	}
}

[Serializable]
public class BTree<T> : BTree<BTreeBlock<T>, BTreeNode<T>, T>
{
	/// <inheritdoc />
	public BTree(int degree)
		: base(degree)
	{
	}

	/// <inheritdoc />
	public BTree(int degree, IGenericComparer<T> comparer)
		: base(degree, comparer)
	{
	}

	/// <inheritdoc />
	public override BTreeBlock<T> MakeBlock() { return new BTreeBlock<T>(this, Degree); }
}

[Serializable]
public class BTree<TKey, TValue> : BTree<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TKey, TValue>
{
	/// <inheritdoc />
	public BTree(int degree)
		: base(degree)
	{
	}

	/// <inheritdoc />
	public BTree(int degree, IGenericComparer<TKey> comparer)
		: base(degree, comparer)
	{
	}

	/// <inheritdoc />
	public override BTreeBlock<TKey, TValue> MakeBlock() { return new BTreeBlock<TKey, TValue>(this, Degree); }
}

public static class BTree
{
	public const int MINIMUM_DEGREE = 2;

	public static int MinimumEntries(int degree)
	{
		if (degree < MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree));
		return degree - 1;
	}

	public static int MaximumEntries(int degree)
	{
		if (degree < MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree));
		return 2 * degree - 1;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int FastMinimumEntries(int degree)
	{
		return degree - 1;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int FastMaximumEntries(int degree)
	{
		return 2 * degree - 1;
	}
}
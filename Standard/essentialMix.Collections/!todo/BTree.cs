using System.Collections;
using System.Collections.Generic;

namespace essentialMix.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/B-tree">B-tree</see> using the linked representation.
	/// See a breif overview at <see href="https://www.youtube.com/watch?v=aZjYr87r1b8">10.2  B Trees and B+ Trees. How they are useful in Databases</see>.
	/// <para>Based on BTree chapter in "Introduction to Algorithms", by Thomas Cormen, Charles Leiserson, Ronald Rivest.</para>
	/// <para>This uses the same abstract pattern as <see cref="LinkedBinaryTree{TNode,T}"/></para>
	/// </summary>
	/// <typeparam name="TNode">The node type. Must inherit from <see cref="LinkedBinaryNode{TNode, T}"/></typeparam>
	/// <typeparam name="T">The element type of the tree</typeparam>
	/*
	 * A B-tree of order m is a tree which satisfies the following properties:
	 * 1. The nodes in a B-tree of order m can have a maximum of m children.
	 * 2. Each internal node (non-leaf and non-root) can have at least (m/2) children.
	 * 3. The root has at least two children if it is not a leaf node.
	 * 4. A non-leaf node with k children contains k − 1 keys.
	 * 5. All leaves appear in the same level and carry no information.
	 */
	public abstract class BTree<TNode, TEntry, T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
		where TNode : BTreeNode<TNode, TEntry, T>
		where TEntry : BTreeEntry<TEntry, T>
	{
	}
}

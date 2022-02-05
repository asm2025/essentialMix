using System;
using System.Collections.Generic;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections;
/*
* https://brilliant.org/wiki/pairing-heap/
* https://users.cs.fiu.edu/~weiss/dsaa_c++/code/PairingHeap.cpp <= actually nice one :)
*/

/// <summary>
/// <inheritdoc />
/// <para>
/// <see href="https://en.wikipedia.org/wiki/Pairing_heap">Pairing heap</see> are heap-ordered multi-way tree structures, and can
/// be considered simplified Fibonacci heaps. They are considered a "robust choice" for implementing such algorithms as Prim's MST
/// algorithm because they have fast running time for their operations. They are modification of Pairing Heaps.
/// </para>
/// </summary>
/// <typeparam name="T">The element type of the heap</typeparam>
[Serializable]
public abstract class PairingHeap<T> : SiblingsHeap<PairingNode<T>, T>
{
	/// <inheritdoc />
	protected PairingHeap()
		: this((IComparer<T>)null)
	{
	}

	protected PairingHeap(IComparer<T> comparer)
		: base(comparer)
	{
	}

	protected PairingHeap([NotNull] IEnumerable<T> enumerable)
		: this(enumerable, null)
	{
	}

	protected PairingHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
		: base(enumerable, comparer)
	{
	}

	/// <inheritdoc />
	public sealed override PairingNode<T> MakeNode(T value) { return new PairingNode<T>(value); }

	/// <inheritdoc />
	public sealed override PairingNode<T> Add(PairingNode<T> node)
	{
		node.Invalidate();
		Head = Head == null
					? node
					: Meld(Head, node);
		Count++;
		_version++;
		return node;
	}

	/// <inheritdoc />
	public sealed override bool Remove(PairingNode<T> node)
	{
		// https://www.geeksforgeeks.org/pairing-heap/
		// https://brilliant.org/wiki/pairing-heap/
		PairingNode<T> mergedLeftMost = TwoPassMerge(node.LeftMostChild());
		Head = ReferenceEquals(node, Head)
					? mergedLeftMost
					: Meld(Head, mergedLeftMost);
		Count--;
		_version++;
		return true;
	}

	/// <inheritdoc />
	public sealed override void Clear()
	{
		Head = null;
		Count = 0;
		_version++;
	}

	/// <inheritdoc />
	public sealed override void DecreaseKey(PairingNode<T> node, T newValue)
	{
		if (Head == null) throw new CollectionIsEmptyException();
		if (Compare(node.Value, newValue) < 0) throw new InvalidOperationException("Invalid new key.");
		node.Value = newValue;
		if (ReferenceEquals(node, Head)) return;
		if (node.Sibling != null) node.Sibling.Previous = node.Previous;
			
		if (node.Previous != null)
		{
			if (node.Previous.Child == node) node.Previous.Child = node.Sibling;
			else node.Previous.Sibling = node.Sibling;
		}

		node.Sibling = null;
		Head = Meld(Head, node);
		_version++;
	}

	/// <inheritdoc />
	public sealed override T Value()
	{
		if (Head == null) throw new CollectionIsEmptyException();
		return Head.Value;
	}

	/// <inheritdoc />
	public sealed override PairingNode<T> ExtractNode()
	{
		if (Head == null) throw new CollectionIsEmptyException();
		PairingNode<T> node = Head;
		Head = Head.Child == null
					? null
					: TwoPassMerge(Head.Child);
		Count--;
		_version++;
		node.Invalidate();
		return node;
	}

	/// <summary>
	/// Maintains heap properties by comparing and linking a and b together to satisfy heap order.
	/// x.Sibling MUST be NULL on entry.
	/// </summary>
	/// <param name="x">The first node. Usually the Head node.</param>
	/// <param name="y">The second node.</param>
	/// <returns>The merged node. The value returned should be assigned back to whichever node was passed as the first node parameter.</returns>
	private PairingNode<T> Meld(PairingNode<T> x, PairingNode<T> y)
	{
		if (ReferenceEquals(x, y)) return x;
		if (x == null) return y;
		if (y == null) return x;

		if (Compare(y.Value, x.Value) < 0)
		{
			y.Previous = x.Previous;
			x.Previous = y;
			x.Sibling = y.Child;
			if (x.Sibling != null) x.Sibling.Previous = x;
			y.Child = x;
			return y;
		}

		y.Previous = x;
		x.Sibling = y.Sibling;
		if (x.Sibling != null) x.Sibling.Previous = x;
		y.Sibling = x.Child;
		if (y.Sibling != null) y.Sibling.Previous = y;
		x.Child = y;
		return x;
	}

	/// <summary>
	/// Implements two-pass merging. It is usually used to delete the root node and combine the siblings.
	/// </summary>
	/// <param name="node"></param>
	/// <returns>The new root node</returns>
	private PairingNode<T> TwoPassMerge(PairingNode<T> node)
	{
		if (node?.Sibling == null) return node;

		List<PairingNode<T>> nodes = new List<PairingNode<T>>();

		do
		{
			nodes.Add(node);
			// break links
			node.Previous.Sibling = null;
			node = node.Sibling;
		}
		while (node != null);

		int i = 0;
		/*
		* Combine subtrees two at a time, going left to right.
		* The 1st item will keep the merge result.
		*/
		while (i + 1 < nodes.Count)
		{
			nodes[i] = Meld(nodes[i], nodes[i + 1]);
			i += 2;
		}

		// i has the result of last merge.
		i -= 2;
		// If an odd number of nodes, get the last one.
		if (i == nodes.Count - 3) nodes[i] = Meld(nodes[i], nodes[i + 2]);

		/*
		* Now go right to left, merging last node with next to last.
		* The result becomes the new last.
		*/
		while (i >= 2)
		{
			nodes[i - 2] = Meld(nodes[i - 2], nodes[i]);
			i -= 2;
		}

		return nodes[0];
	}
}

/// <summary>
/// <inheritdoc />
/// <para>
/// <see href="https://en.wikipedia.org/wiki/Pairing_heap">Pairing heap</see> are heap-ordered multi-way tree structures, and can
/// be considered simplified Fibonacci heaps. They are considered a "robust choice" for implementing such algorithms as Prim's MST
/// algorithm because they have fast running time for their operations. They are modification of Pairing Heaps.
/// </para>
/// </summary>
/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
/// priority of the item.
/// </typeparam>
/// <typeparam name="TValue">The element type of the heap</typeparam>
[Serializable]
public abstract class PairingHeap<TKey, TValue> : SiblingsHeap<PairingNode<TKey, TValue>, TKey, TValue>
{
	/// <inheritdoc />
	protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem)
		: base(getKeyForItem)
	{
	}

	/// <inheritdoc />
	protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
		: base(getKeyForItem, keyComparer, comparer)
	{
	}

	/// <inheritdoc />
	protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
		: base(getKeyForItem, enumerable)
	{
	}

	/// <inheritdoc />
	protected PairingHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
		: base(getKeyForItem, enumerable, keyComparer, comparer)
	{
	}

	/// <inheritdoc />
	public sealed override PairingNode<TKey, TValue> MakeNode(TValue value) { return new PairingNode<TKey, TValue>(GetKeyForItem(value), value); }

	/// <inheritdoc />
	public sealed override PairingNode<TKey, TValue> Add(PairingNode<TKey, TValue> node)
	{
		node.Invalidate();
		Head = Head == null
					? node
					: Meld(Head, node);
		Count++;
		_version++;
		return node;
	}

	/// <inheritdoc />
	public sealed override bool Remove(PairingNode<TKey, TValue> node)
	{
		// https://www.geeksforgeeks.org/pairing-heap/
		// https://brilliant.org/wiki/pairing-heap/
		PairingNode<TKey, TValue> mergedLeftMost = TwoPassMerge(node.LeftMostChild());
		Head = ReferenceEquals(node, Head)
					? mergedLeftMost
					: Meld(Head, mergedLeftMost);
		Count--;
		_version++;
		return true;
	}

	/// <inheritdoc />
	public sealed override void Clear()
	{
		Head = null;
		Count = 0;
		_version++;
	}

	/// <inheritdoc />
	public sealed override void DecreaseKey(PairingNode<TKey, TValue> node, TKey newKey)
	{
		if (Head == null) throw new CollectionIsEmptyException();
		if (Compare(node.Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
		node.Key = newKey;
		if (ReferenceEquals(node, Head)) return;
		if (node.Sibling != null) node.Sibling.Previous = node.Previous;
			
		if (node.Previous != null)
		{
			if (node.Previous.Child == node) node.Previous.Child = node.Sibling;
			else node.Previous.Sibling = node.Sibling;
		}

		node.Sibling = null;
		Head = Meld(Head, node);
		_version++;
	}

	/// <inheritdoc />
	public sealed override TValue Value()
	{
		if (Head == null) throw new CollectionIsEmptyException();
		return Head.Value;
	}

	/// <inheritdoc />
	public sealed override PairingNode<TKey, TValue> ExtractNode()
	{
		if (Head == null) throw new CollectionIsEmptyException();
		PairingNode<TKey, TValue> node = Head;
		Head = Head.Child == null
					? null
					: TwoPassMerge(Head.Child);
		Count--;
		_version++;
		node.Invalidate();
		return node;
	}

	/// <summary>
	/// Maintains heap properties by comparing and linking a and b together to satisfy heap order.
	/// x.Sibling MUST be NULL on entry.
	/// </summary>
	/// <param name="x">The first node. Usually the Head node.</param>
	/// <param name="y">The second node.</param>
	/// <returns>The merged node. The value returned should be assigned back to whichever node was passed as the first node parameter.</returns>
	private PairingNode<TKey, TValue> Meld(PairingNode<TKey, TValue> x, PairingNode<TKey, TValue> y)
	{
		if (ReferenceEquals(x, y)) return x;
		if (x == null) return y;
		if (y == null) return x;

		if (Compare(y.Key, x.Key) < 0)
		{
			y.Previous = x.Previous;
			x.Previous = y;
			x.Sibling = y.Child;
			if (x.Sibling != null) x.Sibling.Previous = x;
			y.Child = x;
			return y;
		}

		y.Previous = x;
		x.Sibling = y.Sibling;
		if (x.Sibling != null) x.Sibling.Previous = x;
		y.Sibling = x.Child;
		if (y.Sibling != null) y.Sibling.Previous = y;
		x.Child = y;
		return x;
	}

	/// <summary>
	/// Implements two-pass merging. It is usually used to delete the root node and combine the siblings.
	/// </summary>
	/// <param name="node"></param>
	/// <returns>The new root node</returns>
	private PairingNode<TKey, TValue> TwoPassMerge(PairingNode<TKey, TValue> node)
	{
		if (node?.Sibling == null) return node;

		List<PairingNode<TKey, TValue>> nodes = new List<PairingNode<TKey, TValue>>();

		do
		{
			nodes.Add(node);
			// break links
			node.Previous.Sibling = null;
			node = node.Sibling;
		}
		while (node != null);

		int i = 0;
		/*
		* Combine subtrees two at a time, going left to right.
		* The 1st item will keep the merge result.
		*/
		while (i + 1 < nodes.Count)
		{
			nodes[i] = Meld(nodes[i], nodes[i + 1]);
			i += 2;
		}

		// i has the result of last merge.
		i -= 2;
		// If an odd number of nodes, get the last one.
		if (i == nodes.Count - 3) nodes[i] = Meld(nodes[i], nodes[i + 2]);

		/*
		* Now go right to left, merging last node with next to last.
		* The result becomes the new last.
		*/
		while (i >= 2)
		{
			nodes[i - 2] = Meld(nodes[i - 2], nodes[i]);
			i -= 2;
		}

		return nodes[0];
	}
}
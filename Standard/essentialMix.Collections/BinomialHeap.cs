using System;
using System.Collections.Generic;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/*
	 * https://brilliant.org/wiki/binomial-heap/
	 *
	 * https://algorithmtutor.com/Data-Structures/Tree/Binomial-Heaps/ << good (after fixing a couple of bugs - extractMin and merge).
	 * OK, in the above link, extractMin has a weird implementation and I'm not sure it's working essentially!
	 *
	 * https://gist.github.com/chinchila/81a4c9bfd852e775f2bdf68339d212a2 << good. actually this one is better and simpler.
	 * the rest, no matter what site it is, has some issues after test, not stable or utter garbage!
	 *
	 * And then I found https://keithschwarz.com/interesting/code/?dir=binomial-heap from Keith Schwarz a.k.a templatetypedef
	 * @stackOverflow. It contains some useful and dense explanation everywhere. He seems to be an interesting lecturer at Stanford as
	 * well with a bunch of interesting code.
	 * His implementation has a different style which does not use a degree or parent node pointer per each node, which is cool because it
	 * enhances the structure in terms of space required to store the nodes. Unfortunately, It does not have a Head/Root node but rather a
	 * trees list and does not implement a few essential functions such as remove a node or DecreaseKey!
	 * So, Maybe will try it some other time.
	 */
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Binomial_heap">Binomial heap</see> using the linked representation.
	/// It is a data structure that acts as a priority queue but also allows pairs of heaps to be merged together.
	/// It is implemented as a heap similar to a binary heap but using a special tree structure that is different
	/// from the complete binary trees used by binary heaps.
	/// </summary>
	/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
	/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
	/// priority of the item.</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	[Serializable]
	public abstract class BinomialHeapBase<TNode, T> : SiblingsHeap<TNode, T>
		where TNode : BinomialNodeBase<TNode, T>
	{

	}

	[Serializable]
	public abstract class BinomialHeap<TKey, TValue> : SiblingsHeap<BinomialNode<TKey, TValue>, TKey, TValue>
	{
		/// <inheritdoc />
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: base(getKeyForItem)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: base(getKeyForItem, enumerable)
		{
		}

		/// <inheritdoc />
		protected BinomialHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(getKeyForItem, enumerable, keyComparer, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override BinomialNode<TKey, TValue> MakeNode(TValue value) { return new BinomialNode<TKey, TValue>(GetKeyForItem(value), value); }

		/// <inheritdoc />
		public sealed override BinomialNode<TKey, TValue> Add(BinomialNode<TKey, TValue> node)
		{
			node.Invalidate();
			Head = Head == null
						? node
						: Union(Head, node);
			Count++;
			_version++;
			return node;
		}

		/// <inheritdoc />
		public sealed override bool Remove(BinomialNode<TKey, TValue> node)
		{
			BubbleUp(node, true);
			ExtractValue();
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
		public sealed override void DecreaseKey(BinomialNode<TKey, TValue> node, TKey newKey)
		{
			if (Head == null) throw new CollectionIsEmptyException();
			if (Compare(node.Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			node.Key = newKey;
			if (node == Head) return;
			BubbleUp(node);
		}

		/// <inheritdoc />
		public sealed override TValue Value()
		{
			if (Head == null) throw new CollectionIsEmptyException();

			BinomialNode<TKey, TValue> node = Head;
			if (node.Sibling == null) return node.Value;

			foreach (BinomialNode<TKey, TValue> sibling in node.Siblings())
			{
				if (Compare(sibling.Key, node.Key) >= 0) continue;
				node = sibling;
			}

			return node.Value;
		}

		/// <inheritdoc />
		public sealed override BinomialNode<TKey, TValue> ExtractNode()
		{
			if (Head == null) throw new CollectionIsEmptyException();

			BinomialNode<TKey, TValue> minPrev = null
				, min = Head
				, nextPrev = min
				, next = min.Sibling;

			// search root nodes for the value (min/max)
			while (next != null)
			{
				if (Compare(next.Key, min.Key) < 0)
				{
					minPrev = nextPrev;
					min = next;
				}

				nextPrev = next;
				next = next.Sibling;
			}

			if (Head == min) Head = min.Sibling;
			else if (minPrev != null) minPrev.Sibling = min.Sibling;
			
			BinomialNode<TKey, TValue> head = null;
			BinomialNode<TKey, TValue> child = min.Child;

			while (child != null)
			{
				next = child.Sibling;
				child.Sibling = head;
				child.Parent = null;
				head = child;
				child = next;
			}

			Head = Union(Head, head);
			Count--;
			_version++;
			min.Invalidate();
			return min;
		}

		[NotNull]
		private BinomialNode<TKey, TValue> BubbleUp([NotNull] BinomialNode<TKey, TValue> node, bool toRoot = false)
		{
			if (node == Head) return node;

			BinomialNode<TKey, TValue> parent = node.Parent;

			while (parent != null && (toRoot || Compare(node.Key, parent.Key) < 0))
			{
				node.Swap(parent);
				node = parent;
				parent = node.Parent;
			}

			return node;
		}

		private void Link([NotNull] BinomialNode<TKey, TValue> x, [NotNull] BinomialNode<TKey, TValue> y)
		{
			y.Parent = x;
			y.Sibling = x.Child;
			x.Child = y;
			x.Degree++;
		}

		private BinomialNode<TKey, TValue> Merge(BinomialNode<TKey, TValue> x, BinomialNode<TKey, TValue> y)
		{
			if (ReferenceEquals(x, y)) return x;
			if (x == null) return y;
			if (y == null) return x;

			BinomialNode<TKey, TValue> head;

			if (x.Degree <= y.Degree)
			{
				head = x;
				x = x.Sibling;
			}
			else
			{
				head = y;
				y = y.Sibling;
			}

			BinomialNode<TKey, TValue> tail = head;

			/*
			 * merge two heaps without taking care of trees with same degree the roots of the tree must be in
			 * ascending order of degree from left to right.
			 */
			while (x != null && y != null)
			{
				if (x.Degree <= y.Degree)
				{
					tail.Sibling = x;
					x = x.Sibling;
				}
				else
				{
					tail.Sibling = y;
					y = y.Sibling;
				}

				tail = tail.Sibling;
			}

			tail.Sibling = x ?? y;
			return head;
		}

		private BinomialNode<TKey, TValue> Union(BinomialNode<TKey, TValue> x, BinomialNode<TKey, TValue> y)
		{
			BinomialNode<TKey, TValue> head = Merge(x, y);
			if (head == null) return null;

			BinomialNode<TKey, TValue> prev = null
				, node = head
				, next = node.Sibling;

			// scan the merged list and merge binomial trees with same degree
			while (next != null)
			{
				/*
				 * if two adjacent tree roots have different degree or 3 consecutive roots
				 * have same degree move to the next tree.
				 */
				if (node.Degree != next.Degree || next.Sibling != null && node.Degree == next.Sibling.Degree)
				{
					prev = node;
					node = next;
				}
				else
				{
					// otherwise merge binomial trees with same degree
					if (Compare(node.Key, next.Key) < 0)
					{
						node.Sibling = next.Sibling;
						Link(node, next);
					}
					else
					{
						if (prev == null) head = next;
						else prev.Sibling = next;

						Link(next, node);
						node = next;
					}
				}

				next = node.Sibling;
			}

			return head;
		}
	}

	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Binomial_heap">Binomial heap</see> using the linked representation.
	/// It is a data structure that acts as a priority queue but also allows pairs of heaps to be merged together.
	/// It is implemented as a heap similar to a binary heap but using a special tree structure that is different
	/// from the complete binary trees used by binary heaps.
	/// </summary>
	/// <typeparam name="T">The element type of the heap</typeparam>
	[Serializable]
	public abstract class BinomialHeap<T> : SiblingsHeap<BinomialNode<T>, T>
	{
		/// <inheritdoc />
		protected BinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		protected BinomialHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected BinomialHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected BinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override BinomialNode<T> MakeNode(T value) { return new BinomialNode<T>(value); }

		/// <inheritdoc />
		public sealed override BinomialNode<T> Add(BinomialNode<T> node)
		{
			node.Invalidate();
			Head = Head == null
						? node
						: Union(Head, node);
			Count++;
			_version++;
			return node;
		}

		/// <inheritdoc />
		public sealed override bool Remove(BinomialNode<T> node)
		{
			BubbleUp(node, true);
			ExtractValue();
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
		public sealed override void DecreaseKey(BinomialNode<T> node, T newValue)
		{
			if (Head == null) throw new CollectionIsEmptyException();
			if (Compare(node.Value, newValue) < 0) throw new InvalidOperationException("Invalid new key.");
			node.Value = newValue;
			if (node == Head) return;
			BubbleUp(node);
		}

		/// <inheritdoc />
		public sealed override T Value()
		{
			if (Head == null) throw new CollectionIsEmptyException();

			BinomialNode<T> node = Head;
			if (node.Sibling == null) return node.Value;

			foreach (BinomialNode<T> sibling in node.Siblings())
			{
				if (Compare(sibling.Value, node.Value) >= 0) continue;
				node = sibling;
			}

			return node.Value;
		}

		/// <inheritdoc />
		public sealed override BinomialNode<T> ExtractNode()
		{
			if (Head == null) throw new CollectionIsEmptyException();

			BinomialNode<T> minPrev = null
				, min = Head
				, nextPrev = min
				, next = min.Sibling;

			// search root nodes for the value (min/max)
			while (next != null)
			{
				if (Compare(next.Value, min.Value) < 0)
				{
					minPrev = nextPrev;
					min = next;
				}

				nextPrev = next;
				next = next.Sibling;
			}

			if (Head == min) Head = min.Sibling;
			else if (minPrev != null) minPrev.Sibling = min.Sibling;
			
			BinomialNode<T> head = null;
			BinomialNode<T> child = min.Child;

			while (child != null)
			{
				next = child.Sibling;
				child.Sibling = head;
				child.Parent = null;
				head = child;
				child = next;
			}

			Head = Union(Head, head);
			Count--;
			_version++;
			min.Invalidate();
			return min;
		}

		[NotNull]
		private BinomialNode<T> BubbleUp([NotNull] BinomialNode<T> node, bool toRoot = false)
		{
			if (node == Head) return node;

			BinomialNode<T> parent = node.Parent;

			while (parent != null && (toRoot || Compare(node.Value, parent.Value) < 0))
			{
				node.Swap(parent);
				node = parent;
				parent = node.Parent;
			}

			return node;
		}

		private void Link([NotNull] BinomialNode<T> x, [NotNull] BinomialNode<T> y)
		{
			y.Parent = x;
			y.Sibling = x.Child;
			x.Child = y;
			x.Degree++;
		}

		private BinomialNode<T> Merge(BinomialNode<T> x, BinomialNode<T> y)
		{
			if (ReferenceEquals(x, y)) return x;
			if (x == null) return y;
			if (y == null) return x;

			BinomialNode<T> head;

			if (x.Degree <= y.Degree)
			{
				head = x;
				x = x.Sibling;
			}
			else
			{
				head = y;
				y = y.Sibling;
			}

			BinomialNode<T> tail = head;

			/*
			 * merge two heaps without taking care of trees with same degree the roots of the tree must be in
			 * ascending order of degree from left to right.
			 */
			while (x != null && y != null)
			{
				if (x.Degree <= y.Degree)
				{
					tail.Sibling = x;
					x = x.Sibling;
				}
				else
				{
					tail.Sibling = y;
					y = y.Sibling;
				}

				tail = tail.Sibling;
			}

			tail.Sibling = x ?? y;
			return head;
		}

		private BinomialNode<T> Union(BinomialNode<T> x, BinomialNode<T> y)
		{
			BinomialNode<T> head = Merge(x, y);
			if (head == null) return null;

			BinomialNode<T> prev = null
				, node = head
				, next = node.Sibling;

			// scan the merged list and merge binomial trees with same degree
			while (next != null)
			{
				/*
				 * if two adjacent tree roots have different degree or 3 consecutive roots
				 * have same degree move to the next tree.
				 */
				if (node.Degree != next.Degree || next.Sibling != null && node.Degree == next.Sibling.Degree)
				{
					prev = node;
					node = next;
				}
				else
				{
					// otherwise merge binomial trees with same degree
					if (Compare(node.Value, next.Value) < 0)
					{
						node.Sibling = next.Sibling;
						Link(node, next);
					}
					else
					{
						if (prev == null) head = next;
						else prev.Sibling = next;

						Link(next, node);
						node = next;
					}
				}

				next = node.Sibling;
			}

			return head;
		}
	}
}

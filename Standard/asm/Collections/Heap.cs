using System;
using System.Collections.Generic;
using System.Diagnostics;
using asm.Extensions;
using asm.Patterns.Layout;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Binary_tree">BinaryTree</see> using the array representation.
	/// </summary>
	/// <typeparam name="T">The element type of the heap</typeparam>
	/*
	 *	     ___ F ____
	 *	    /          \
	 *	   C            I
	 *	 /   \        /   \
	 *	A     D      G     J
	 *	  \    \      \     \
	 *	    B   E      H     K
	 *
	 *	    BFS            DFS            DFS            DFS
	 *	  LevelOrder      PreOrder       InOrder        PostOrder
	 *	     1              1              4              5
	 *	   /   \          /   \          /   \          /   \
	 *	  2     3        2     5        2     5        3     4
	 *	 /   \          /   \          /   \          /   \
	 *	4     5        3     4        1     3        1     2
	 *
	 * BFS (LevelOrder): FCIADGJBEHK => Root-Left-Right (Queue)
	 * DFS [PreOrder]:   FCABDEIGHJK => Root-Left-Right (Stack)
	 * DFS [InOrder]:    ABCDEFGHIJK => Left-Root-Right (Stack)
	 * DFS [PostOrder]:  BAEDCHGKJIF => Left-Right-Root (Stack)
	 */
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public abstract class Heap<T> : ArrayBinaryTree<T>
	{
		/// <inheritdoc />
		protected Heap() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected Heap(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected Heap(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		protected Heap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected Heap([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected Heap([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: this(0, comparer)
		{
			Add(collection);
		}

		public T Value
		{
			get
			{
				if (Count == 0) throw new InvalidOperationException("Heap is empty.");
				return Items[0];
			}
		}

		public void Add([NotNull] IEnumerable<T> values)
		{
			foreach (T value in values) 
				Add(value);
		}

		public void Add(T value)
		{
			if (Count == Items.Length) EnsureCapacity(Count + 1);
			Items[Count] = value;
			Count++;
			_version++;
			if (Count < 2) return;
			BubbleUp(Count - 1);
		}

		public bool Remove() { return Remove(out _); }
		public bool Remove(out T value)
		{
			if (Count == 0)
			{
				value = default(T);
				return false;
			}

			value = Items[0];
			Items[0] = Count == 1
							? default(T)
							: Items[Count - 1];
			Count--;
			_version++;
			if (Count > 1) BubbleDown(0);
			return true;
		}

		public void Clear()
		{
			if (Count == 0) return;
			Array.Clear(Items, 0, Count);
			Count = 0;
		}

		public T ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 0; i < k - 1 && Remove(); i++)
			{
			}

			return Value;
		}

		/// <inheritdoc />
		public override bool Validate()
		{
			/*
			 * if typeof(T) is ValueType, there is no way to tell if the default value is valid or not
			 * but for class type, a null is empty so we can validate this.
			 */
			if (typeof(T).IsValueType) return true;

			// nodes with one child are not supposed to appear more than once while scanning from left to right
			bool isValid = true, nonNode = false;
			ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(this);
			Iterate(0, BinaryTreeTraverseMethod.LevelOrder, HorizontalFlow.LeftToRight, e =>
			{
				node.Index = e;
				
				if (node.LeftIndex > -1 && !ReferenceEquals(null, Items[node.LeftIndex]))
				{
					if (nonNode) isValid = false;
				}
				else
				{
					nonNode = true;
				}

				if (node.RightIndex > -1 && !ReferenceEquals(null, Items[node.RightIndex]))
				{
					if (nonNode) isValid = false;
				}
				else
				{
					nonNode = true;
				}

				return isValid;
			});
			return isValid;
		}

		protected abstract void BubbleUp(int index);

		protected abstract void BubbleDown(int index);
	}

	public static class Heap
	{
		public static void Heapify<T>([NotNull] IList<T> values) { Heapify(values, 0, -1, false, null); }
		public static void Heapify<T>([NotNull] IList<T> values, bool descending) { Heapify(values, 0, -1, descending, null); }
		public static void Heapify<T>([NotNull] IList<T> values, IComparer<T> comparer) { Heapify(values, 0, -1, false, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, bool descending, IComparer<T> comparer) { Heapify(values, 0, -1, descending, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index) { Heapify(values, index, -1, false, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, bool descending) { Heapify(values, index, -1, descending, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, IComparer<T> comparer) { Heapify(values, index, -1, false, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, bool descending, IComparer<T> comparer) { Heapify(values, index, -1, descending, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count) { Heapify(values, index, count, false, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count, bool descending) { Heapify(values, index, count, descending, null); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count, IComparer<T> comparer) { Heapify(values, index, count, false, comparer); }
		public static void Heapify<T>([NotNull] IList<T> values, int index, int count, bool descending, IComparer<T> comparer)
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms part 2
			values.Count.ValidateRange(index, ref count);
			if (count < 2 || values.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// Build heap (rearrange array) starting from the last parent
			for (int i = count / 2 - 1; i >= index; i--)
			{
				// Heapify
				for (int parent = i, left = parent * 2 + 1, right = parent * 2 + 2, largest = parent;
					left < count;
					parent = largest, left = parent * 2 + 1, right = parent * 2 + 2, largest = parent)
				{
					// If left child is larger than root 
					if (left < count && comparer.IsGreaterThan(values[left], values[largest])) largest = left;
					// If right child is larger than largest so far 
					if (right < count && comparer.IsGreaterThan(values[right], values[largest])) largest = right;
					if (largest == parent) break;
					values.FastSwap(i, largest);
				}

				if (comparer.IsLessThanOrEqual(values[i], values[(i - 1) / 2])) continue;
				
				// if child is (bigger/smaller) than parent
				for (int child = i, parent = (child - 1) / 2; 
					parent >= index && comparer.IsGreaterThan(values[child], values[parent]); 
					child = parent, parent = (child - 1) / 2)
				{
					values.FastSwap(child, parent);
				}
			}
		}

		public static void Sort<T>([NotNull] IList<T> values, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/iterative-heap-sort/
			values.Count.ValidateRange(index, ref count);
			if (count < 2 || values.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// Build heap (rearrange array) starting from the last parent
			for (int i = index + 1; i < count; i++)
			{
				// swap child and parent until parent is smaller
				for (int child = i, parent = (child - 1) / 2;
					parent >= index && comparer.IsGreaterThan(values[child], values[parent]);
					child = parent, parent = (child - 1) / 2)
				{
					values.FastSwap(child, parent);
				}
			}

			for (int i = count - 1; i > index; i--)
			{
				// swap value of first indexed with last indexed
				values.FastSwap(index, i);
				
				// maintaining heap property after each swapping
				for (int parent = 0, child = parent * 2 + 1; child < i; parent = child, child = parent * 2 + 1)
				{
					// if left is smaller than right point index to right
					if (child < i - 1 && comparer.IsLessThan(values[child], values[child + 1])) child++;
					// if parent is smaller than child, swap them
					if (child < i && comparer.IsLessThan(values[parent], values[child])) values.FastSwap(parent, child);
				}
			}
		}
	}
}
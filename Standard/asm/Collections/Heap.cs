using System;
using System.Collections.Generic;
using System.Diagnostics;
using asm.Patterns.Collections;
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
			: this(0, Comparer<T>.Default)
		{
		}

		/// <inheritdoc />
		protected Heap(int capacity)
			: this(capacity, Comparer<T>.Default)
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
			: this(collection, Comparer<T>.Default)
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
			Iterate(0, TraverseMethod.LevelOrder, HorizontalFlow.LeftToRight, e =>
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
}
using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MaxPriorityQueue<TPriority, T> : PriorityQueue<TPriority, T>
		where TPriority : struct, IComparable
	{
		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] Func<T, TPriority> priority)
			: this(0, null, priority)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity, [NotNull] Func<T, TPriority> priority)
			: this(capacity, null, priority)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: base(capacity, comparer, priority)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] IEnumerable<T> collection, [NotNull] Func<T, TPriority> priority)
			: this(collection, null, priority)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
			Add(collection);
		}

		/// <inheritdoc />
		protected sealed override void BubbleUp(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			// the parent's value must be greater than its children so move the greater value up.
			while (node.ParentIndex > -1 && GetPriority(Items[node.ParentIndex]).CompareTo(GetPriority(Items[node.Index])) < 0)
			{
				Swap(node.Index, node.ParentIndex);
				node.Index = node.ParentIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		/// <inheritdoc />
		protected sealed override void BubbleDown(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			/*
			 * the parent's value must be greater than its children.
			 * move the smaller value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the greater value.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && GetPriority(Items[node.LeftIndex]).CompareTo(GetPriority(Items[childIndex])) > 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && GetPriority(Items[node.RightIndex]).CompareTo(GetPriority(Items[childIndex])) > 0) childIndex = node.RightIndex;
				if (childIndex == node.Index) break;
				Swap(node.Index, childIndex);
				node.Index = childIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}
	}

	[Serializable]
	public class MaxPriorityQueue<T> : MaxPriorityQueue<T, T>
		where T : struct, IComparable
	{
		/// <inheritdoc />
		public MaxPriorityQueue()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue(int capacity, IComparer<T> comparer)
			: base(capacity, comparer, e => e)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public MaxPriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: this(0, comparer)
		{
			Add(collection);
		}
	}
}
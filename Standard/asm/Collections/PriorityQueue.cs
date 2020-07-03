using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public abstract class PriorityQueue<T> : Heap<T>
	{
		/// <inheritdoc />
		protected PriorityQueue()
			: this(0, null, null)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity)
			: this(capacity, null, null)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(IComparer<T> priorityComparer)
			: this(0, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity, IComparer<T> priorityComparer)
			: this(capacity, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(0, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue(int capacity, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(capacity, comparer)
		{
			PriorityComparer = priorityComparer ?? Comparer;
		}

		/// <inheritdoc />
		protected PriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> priorityComparer)
			: this(enumerable, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected PriorityQueue([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(0, comparer, priorityComparer)
		{
			Add(enumerable);
		}

		[NotNull]
		public IComparer<T> PriorityComparer { get; }

		/// <inheritdoc />
		protected sealed override void BubbleUp(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			// the parent's value must be greater than its children so move the greater value up.
			while (node.ParentIndex > -1 && Compare(Items[node.ParentIndex], Items[node.Index]) > 0)
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
				if (node.LeftIndex > -1 && Compare(Items[node.LeftIndex], Items[childIndex]) < 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && Compare(Items[node.RightIndex], Items[childIndex]) < 0) childIndex = node.RightIndex;
				if (childIndex == node.Index) break;
				Swap(node.Index, childIndex);
				node.Index = childIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		protected abstract int Compare(T x, T y);
	}
}
using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MinPriorityQueue<T> : PriorityQueue<T>
	{
		/// <inheritdoc />
		public MinPriorityQueue()
			: this(0, null, null)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity)
			: this(capacity, null, null)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(IComparer<T> priorityComparer)
			: this(0, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity, IComparer<T> priorityComparer)
			: this(capacity, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(IComparer<T> comparer, IComparer<T> priorityComparer)
			: this(0, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(capacity, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> priorityComparer)
			: this(collection, null, priorityComparer)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer, IComparer<T> priorityComparer)
			: base(collection, comparer, priorityComparer)
		{
		}

		/// <inheritdoc />
		protected sealed override void BubbleUp(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			// the parent's value must be lesser than its children so move the smaller value up.
			while (node.ParentIndex > -1 && PriorityComparer.IsGreaterThan(Items[node.ParentIndex], Items[node.Index]))
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
			 * the parent's value must be lesser than its children.
			 * move the greater value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the smaller value.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && PriorityComparer.IsLessThan(Items[node.LeftIndex], Items[childIndex])) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && PriorityComparer.IsLessThan(Items[node.RightIndex], Items[childIndex])) childIndex = node.RightIndex;
				if (childIndex == node.Index) break;
				Swap(node.Index, childIndex);
				node.Index = childIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}
	}
}
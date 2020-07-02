using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public sealed class MaxHeap<T> : Heap<T>
	{
		/// <inheritdoc />
		public MaxHeap() 
		{
		}

		/// <inheritdoc />
		public MaxHeap(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public MaxHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MaxHeap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public MaxHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MaxHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override void BubbleUp(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			/*
			 * the parent value must be greater than its children.
			 * move the greater value up which means:
			 * if Items[node.ParentIndex] < Items[node.Index], swap the values.
			 */
			while (node.ParentIndex > -1 && Comparer.IsLessThan(Items[node.ParentIndex], Items[node.Index]))
			{
				Swap(node.Index, node.ParentIndex);
				node.Index = node.ParentIndex;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		/// <inheritdoc />
		protected override void BubbleDown(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			Navigator node = NewNavigator(index);

			/*
			 * the parent value must be greater than its children.
			 * move the smaller value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the greater value. this means:
			 * if Items[node.Index] < Items[node.LeftIndex] and
			 * Items[node.Index] < Items[node.RightIndex], swap the values
			 * with the greater child.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && Comparer.IsGreaterThan(Items[node.LeftIndex], Items[childIndex])) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && Comparer.IsGreaterThan(Items[node.RightIndex], Items[childIndex])) childIndex = node.RightIndex;
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

using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public sealed class MinHeap<T> : Heap<T>
	{
		/// <inheritdoc />
		public MinHeap() 
		{
		}

		/// <inheritdoc />
		public MinHeap(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public MinHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MinHeap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public MinHeap([NotNull] IEnumerable<T> collection)
			: base(collection)
		{
		}

		/// <inheritdoc />
		public MinHeap([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected override void BubbleUp(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(this, index);

			/*
			 * the parent value must be less than its children.
			 * move the smaller value up which means:
			 * if Items[node.ParentIndex] > Items[node.Index], swap the values.
			 */
			while (node.ParentIndex > -1 && Comparer.IsGreaterThan(Items[node.ParentIndex], Items[node.Index]))
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
			ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(this, index);

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
				if (node.LeftIndex > -1 && Comparer.IsLessThan(Items[node.LeftIndex], Items[childIndex])) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && Comparer.IsLessThan(Items[node.RightIndex], Items[childIndex])) childIndex = node.RightIndex;
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
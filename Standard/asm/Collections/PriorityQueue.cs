using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class PriorityQueue<TPriority, T> : Heap<T>
		where TPriority : struct, IComparable
	{
		[NotNull]
		private Func<T, TPriority> _getPriority;

		/// <inheritdoc />
		public PriorityQueue([NotNull] Func<T, TPriority> priority)
			: this(0, null, priority)
		{
		}

		/// <inheritdoc />
		public PriorityQueue(int capacity, [NotNull] Func<T, TPriority> priority)
			: this(capacity, null, priority)
		{
		}

		/// <inheritdoc />
		public PriorityQueue(IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
		}

		/// <inheritdoc />
		public PriorityQueue(int capacity, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: base(capacity, comparer)
		{
			_getPriority = priority;
		}

		/// <inheritdoc />
		public PriorityQueue([NotNull] IEnumerable<T> collection, [NotNull] Func<T, TPriority> priority)
			: this(collection, null, priority)
		{
		}

		/// <inheritdoc />
		public PriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
			Add(collection);
		}

		/// <inheritdoc />
		protected sealed override void BubbleUp(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			bool changed = false;
			ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(this, index);

			/*
			 * the parent value must be less than its children.
			 * move the smaller value up which means:
			 * if Items[node.ParentIndex] > Items[node.Index], swap the values.
			 */
			while (node.ParentIndex > -1 && _getPriority(Items[node.ParentIndex]).CompareTo(_getPriority(Items[node.Index])) > 0)
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
				if (node.LeftIndex > -1 && _getPriority(Items[node.LeftIndex]).CompareTo(_getPriority(Items[childIndex])) < 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && _getPriority(Items[node.RightIndex]).CompareTo(_getPriority(Items[childIndex])) < 0) childIndex = node.RightIndex;
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
	public class PriorityQueue<T> : PriorityQueue<T, T>
		where T : struct, IComparable
	{
		/// <inheritdoc />
		public PriorityQueue()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public PriorityQueue(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public PriorityQueue(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public PriorityQueue(int capacity, IComparer<T> comparer)
			: base(capacity, comparer, e => e)
		{
		}

		/// <inheritdoc />
		public PriorityQueue([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public PriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: this(0, comparer)
		{
			Add(collection);
		}
	}
}
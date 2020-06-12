using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MinPriorityQueue<TPriority, T> : PriorityQueue<TPriority, T>
		where TPriority : struct, IComparable
	{
		/// <inheritdoc />
		public MinPriorityQueue([NotNull] Func<T, TPriority> priority)
			: this(0, null, priority)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity, [NotNull] Func<T, TPriority> priority)
			: this(capacity, null, priority)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: this(0, comparer, priority)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
			: base(capacity, comparer, priority)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue([NotNull] IEnumerable<T> collection, [NotNull] Func<T, TPriority> priority)
			: this(collection, null, priority)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer, [NotNull] Func<T, TPriority> priority)
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

			// the parent's value must be lesser than its children so move the smaller value up.
			while (node.ParentIndex > -1 && GetPriority(Items[node.ParentIndex]).CompareTo(GetPriority(Items[node.Index])) > 0)
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
			 * the parent's value must be lesser than its children.
			 * move the greater value down to either left or right.
			 * to select which child to swap the value with, pick the
			 * child with the smaller value.
			 */
			while (node.LeftIndex > -1 || node.RightIndex > -1)
			{
				int childIndex = node.Index;
				if (node.LeftIndex > -1 && GetPriority(Items[node.LeftIndex]).CompareTo(GetPriority(Items[childIndex])) < 0) childIndex = node.LeftIndex;
				if (node.RightIndex > -1 && GetPriority(Items[node.RightIndex]).CompareTo(GetPriority(Items[childIndex])) < 0) childIndex = node.RightIndex;
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
	public class MinPriorityQueue<T> : MinPriorityQueue<T, T>
		where T : struct, IComparable
	{
		/// <inheritdoc />
		public MinPriorityQueue()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue(int capacity, IComparer<T> comparer)
			: base(capacity, comparer, e => e)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public MinPriorityQueue([NotNull] IEnumerable<T> collection, IComparer<T> comparer)
			: this(0, comparer)
		{
			Add(collection);
		}
	}
}
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MaxBinaryHeap<T> : BinaryHeap<T>
	{
		/// <inheritdoc />
		public MaxBinaryHeap() 
		{
		}

		/// <inheritdoc />
		public MaxBinaryHeap(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public MaxBinaryHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinaryHeap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinaryHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MaxBinaryHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override int Compare(T x, T y) { return Comparer.Compare(x, y) * -1; }
	}
}

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class MinBinaryHeap<T> : BinaryHeap<T>
	{
		/// <inheritdoc />
		public MinBinaryHeap() 
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap(int capacity)
			: base(capacity)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MinBinaryHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		protected override int Compare(T x, T y) { return Comparer.Compare(x, y); }
	}
}
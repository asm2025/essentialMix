using System.Collections.Generic;

namespace asm.Comparers
{
	public sealed class ComparerEquality<T> : IEqualityComparer<T>
	{
		private readonly IComparer<T> _comparer;

		public ComparerEquality(IComparer<T> comparer)
		{
			_comparer = comparer;
		}

		/// <inheritdoc />
		public bool Equals(T x, T y) { return _comparer.Compare(x, y) == 0; }

		/// <inheritdoc />
		public int GetHashCode(T obj) { return obj.GetHashCode(); }
	}
}
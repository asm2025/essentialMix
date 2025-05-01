using System.Collections.Generic;

namespace essentialMix.Comparers;

public sealed class ComparerForEquality<T>(IComparer<T> comparer) : IEqualityComparer<T>
{
	/// <inheritdoc />
	public bool Equals(T x, T y) { return comparer.Compare(x, y) == 0; }

	/// <inheritdoc />
	public int GetHashCode(T obj) { return obj.GetHashCode(); }
}
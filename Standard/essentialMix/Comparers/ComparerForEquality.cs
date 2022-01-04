using System.Collections.Generic;

namespace essentialMix.Comparers;

public sealed class ComparerForEquality<T> : IEqualityComparer<T>
{
	private readonly IComparer<T> _comparer;

	public ComparerForEquality(IComparer<T> comparer)
	{
		_comparer = comparer;
	}

	/// <inheritdoc />
	public bool Equals(T x, T y) { return _comparer.Compare(x, y) == 0; }

	/// <inheritdoc />
	public int GetHashCode(T obj) { return obj.GetHashCode(); }
}
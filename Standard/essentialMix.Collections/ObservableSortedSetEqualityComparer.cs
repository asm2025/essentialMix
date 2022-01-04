using System.Collections.Generic;

namespace essentialMix.Collections;

/// <summary>
///     A class that generates an IEqualityComparer for this ObservableSortedSet. Requires that the definition of
///     equality defined by the IComparer for this ObservableSortedSet be consistent with the default IEqualityComparer
///     for the type T. If not, such an IEqualityComparer should be provided through the constructor.
/// </summary>
public class ObservableSortedSetEqualityComparer<T> : IEqualityComparer<ObservableSortedSet<T>>
{
	public ObservableSortedSetEqualityComparer()
		: this(null, null)
	{
	}

	public ObservableSortedSetEqualityComparer(IComparer<T> comparer)
		: this(comparer, null)
	{
	}

	public ObservableSortedSetEqualityComparer(IEqualityComparer<T> equalityComparer)
		: this(null, equalityComparer)
	{
	}

	/// <summary>
	///     Create a new SetEqualityComparer, given a comparer for member order and another for member equality (these
	///     must be consistent in their definition of equality)
	/// </summary>
	public ObservableSortedSetEqualityComparer(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
	{
		Comparer = comparer ?? Comparer<T>.Default;
		EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
	}

	protected IComparer<T> Comparer { get; }
	protected IEqualityComparer<T> EqualityComparer { get; }

	// using comparer to keep equals properties in tact; don't want to choose one of the comparers
	public bool Equals(ObservableSortedSet<T> x, ObservableSortedSet<T> y) { return ObservableSortedSet<T>.SetEquals(x, y, Comparer); }

	//IMPORTANT: this part uses the fact that GetHashCode() is consistent with the notion of equality in the set
	public int GetHashCode(ObservableSortedSet<T> obj)
	{
		int hashCode = 0;

		foreach (T t in obj) 
			hashCode ^= EqualityComparer.GetHashCode(t) & 0x7FFFFFFF;

		return hashCode;
	}

	// Equals method for the comparer itself. 
	public override bool Equals(object obj)
	{
		if (obj is not ObservableSortedSetEqualityComparer<T> comparer) return false;
		return Comparer == comparer.Comparer;
	}

	public override int GetHashCode() { return Comparer.GetHashCode() ^ EqualityComparer.GetHashCode(); }
}
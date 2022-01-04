using System.Collections.Generic;
using System.Linq;
using essentialMix.Comparers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.JonSkeet.MiscUtil.Collections;

/// <summary>
/// Comparer to daisy-chain two existing comparers and 
/// apply in sequence (i.e. sort by x then y)
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChainComparer<T> : IGenericComparer<T>
{
	public ChainComparer()
		: this(null, null) { }

	public ChainComparer(IEnumerable<IComparer<T>> comparers)
		: this(comparers, null) { }

	public ChainComparer(IEnumerable<IComparer<T>> comparers, IEnumerable<IEqualityComparer<T>> equalityComparers)
	{
		if (comparers != null)
		{
			foreach (IComparer<T> comparer in comparers.Where(e => e != null)) 
				Comparers.Add(comparer);
		}

		if (equalityComparers != null)
		{
			foreach (IEqualityComparer<T> equalityComparer in equalityComparers.Where(e => e != null))
				EqualityComparers.Add(equalityComparer);
		}
	}

	[NotNull]
	protected SynchronizedCollection<IComparer<T>> Comparers { get; } = new SynchronizedCollection<IComparer<T>>();

	[NotNull]
	protected SynchronizedCollection<IEqualityComparer<T>> EqualityComparers { get; } = new SynchronizedCollection<IEqualityComparer<T>>();

	public virtual int Compare(T x, T y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		return Comparers.Select(e => e.Compare(x, y))
						.FirstOrDefault(e => e != 0);
	}

	public virtual bool Equals(T x, T y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null) return false;
		return y != null && EqualityComparers.Any(e => e.Equals(x, y));
	}

	public int GetHashCode(T obj) { return obj.GetHashCode(); }

	public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

	public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

	public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
}
using System.Collections.Generic;
using essentialMix.Comparers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.JonSkeet.MiscUtil.Collections;

/// <summary>
/// Comparer to daisy-chain two existing comparers and 
/// apply in sequence (i.e. sort by x then y)
/// </summary>
/// <typeparam name="T"></typeparam>
public class LinkedComparer<T> : IGenericComparer<T>
{
	public LinkedComparer([NotNull] IComparer<T> firstComparer, [NotNull] IComparer<T> secondComparer)
		: this(firstComparer, secondComparer, EqualityComparer<T>.Default, null)
	{
	}

	public LinkedComparer([NotNull] IComparer<T> firstComparer, [NotNull] IComparer<T> secondComparer, [NotNull] IEqualityComparer<T> firstEqualityComparer)
		: this(firstComparer, secondComparer, firstEqualityComparer, null)
	{
	}

	public LinkedComparer([NotNull] IComparer<T> firstComparer, [NotNull] IComparer<T> secondComparer, [NotNull] IEqualityComparer<T> firstEqualityComparer, IEqualityComparer<T> secondEqualityComparer)
	{
		FirstComparer = firstComparer;
		SecondComparer = secondComparer;
		FirstEqualityComparer = firstEqualityComparer;
		SecondEqualityComparer = secondEqualityComparer;
	}

	[NotNull]
	public IComparer<T> FirstComparer { get; }

	[NotNull]
	public IComparer<T> SecondComparer { get; }

	[NotNull]
	public IEqualityComparer<T> FirstEqualityComparer { get; }

	public IEqualityComparer<T> SecondEqualityComparer { get; }

	public virtual int Compare(T x, T y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;

		int n = FirstComparer.Compare(x, y);
		return n != 0 ? n : SecondComparer.Compare(x ,y);
	}

	public virtual bool Equals(T x, T y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return FirstEqualityComparer.Equals(x, y) || SecondEqualityComparer != null && SecondEqualityComparer.Equals(x, y);
	}

	public int GetHashCode(T obj) { return obj.GetHashCode(); }

	public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

	public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

	public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
}
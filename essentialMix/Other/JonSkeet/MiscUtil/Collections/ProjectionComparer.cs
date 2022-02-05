using System;
using System.Collections.Generic;
using essentialMix.Comparers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.JonSkeet.MiscUtil.Collections;

public class ProjectionComparer<TSource, TKey> : IGenericComparer<TSource>
{
	public ProjectionComparer([NotNull] Func<TSource, TKey> projection)
		: this(projection, null, null) { }

	public ProjectionComparer([NotNull] Func<TSource, TKey> projection, IComparer<TKey> comparer)
		: this(projection, comparer, null) { }

	public ProjectionComparer([NotNull] Func<TSource, TKey> projection, IComparer<TKey> comparer, IEqualityComparer<TKey> equalityComparer)
	{
		Projection = projection;
		Comparer = comparer ?? Comparer<TKey>.Default;
		EqualityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;
	}

	[NotNull]
	protected Func<TSource, TKey> Projection { get; }
	protected IComparer<TKey> Comparer { get; }
	protected IEqualityComparer<TKey> EqualityComparer { get; }

	public virtual int Compare(TSource x, TSource y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		return Comparer.Compare(Projection(x), Projection(y));
	}

	public virtual bool Equals(TSource x, TSource y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null) return false;
		return y != null && EqualityComparer.Equals(Projection(x), Projection(y));
	}

	public int GetHashCode(TSource obj) { return obj.GetHashCode(); }

	public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

	public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

	public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
}
using System.Collections.Generic;

namespace essentialMix.Comparers;

public class GenericComparer<T> : IGenericComparer<T>
{
	public static IGenericComparer<T> Default { get; } = new GenericComparer<T>();

	public GenericComparer()
		: this(null, null)
	{
	}

	public GenericComparer(IComparer<T> comparer)
		: this(comparer, null)
	{
	}

	public GenericComparer(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
	{
		Comparer = comparer ?? Comparer<T>.Default;
		EqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
	}

	protected IComparer<T> Comparer { get; }
	protected IEqualityComparer<T> EqualityComparer { get; }

	public virtual int Compare(T x, T y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(x, null)) return 1;
		if (ReferenceEquals(y, null)) return -1;
		return Comparer.Compare(x, y);
	}

	public virtual bool Equals(T x, T y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
		return EqualityComparer.Equals(x, y);
	}

	public virtual int GetHashCode(T obj) { return obj.GetHashCode(); }

	public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

	public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

	public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
}
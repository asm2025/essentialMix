using System.Collections.Generic;

namespace essentialMix.Comparers;

public class GenericComparer<T>(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
	: IGenericComparer<T>
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

	protected IComparer<T> Comparer { get; } = comparer ?? Comparer<T>.Default;
	protected IEqualityComparer<T> EqualityComparer { get; } = equalityComparer ?? EqualityComparer<T>.Default;

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
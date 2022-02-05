using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using essentialMix.Comparers;

namespace essentialMix.Collections;

[Serializable]
public class NumericRangeCollection<T> : Collection<NumericRange<T>>, IComparable<NumericRangeCollection<T>>, IComparable, IEquatable<NumericRangeCollection<T>>
	where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
{
	public NumericRangeCollection()
	{
	}

	public NumericRangeCollection([NotNull] List<NumericRange<T>> list) 
		: base(list)
	{
	}

	public NumericRangeCollection([NotNull] IList<NumericRange<T>> collection) 
		: base(collection)
	{
	}

	public virtual TResult Simplify<TResult>(NumericRangeComparer<T> comparer = null, TResult defaultValue = default(TResult))
		where TResult : List<NumericRange<T>>, new()
	{
		return NumericRangeCollectionComparer<T>.Default.Simplify(this, comparer, defaultValue);
	}

	public virtual int CompareTo(NumericRangeCollection<T> other) { return NumericRangeCollectionComparer<T>.Default.Compare(this, other); }

	public virtual int CompareTo(object obj) { return ReferenceComparer.Default.Compare(this, obj); }

	public virtual bool Equals(NumericRangeCollection<T> other) { return NumericRangeCollectionComparer<T>.Default.Equals(this, other); }
}
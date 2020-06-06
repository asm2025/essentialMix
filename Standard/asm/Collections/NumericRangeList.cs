using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using asm.Comparers;

namespace asm.Collections
{
	[Serializable]
	public class NumericRangeList<T> : List<NumericRange<T>>, IComparable<NumericRangeList<T>>, IComparable, IEquatable<NumericRangeList<T>>
		where T : struct, IComparable<T>, IComparable, IEquatable<T>, IConvertible
	{
		public NumericRangeList()
		{
		}

		public NumericRangeList([NotNull] List<NumericRange<T>> list) 
			: base(list)
		{
		}

		public NumericRangeList([NotNull] IEnumerable<NumericRange<T>> collection) 
			: base(collection)
		{
		}

		public virtual TResult Simplify<TResult>(NumericRangeComparer<T> comparer = null, TResult defaultValue = default(TResult))
			where TResult : List<NumericRange<T>>, new()
		{
			return NumericRangeCollectionComparer<T>.Default.Simplify(this, comparer, defaultValue);
		}

		public virtual int CompareTo(NumericRangeList<T> other) { return NumericRangeCollectionComparer<T>.Default.Compare(this, other); }

		public virtual int CompareTo(object obj) { return ReferenceComparer.Default.Compare(this, obj); }

		public virtual bool Equals(NumericRangeList<T> other) { return NumericRangeCollectionComparer<T>.Default.Equals(this, other); }
	}
}
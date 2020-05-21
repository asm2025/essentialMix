using System;
using System.Collections.Generic;
using asm.Comparers;

namespace asm.Collections
{
	public class NumericRangeComparer<T> : GenericComparer<NumericRange<T>>
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
	{
		public new static NumericRangeComparer<T> Default { get; } = new NumericRangeComparer<T>();

		/// <inheritdoc />
		public NumericRangeComparer()
		{
		}

		/// <inheritdoc />
		public NumericRangeComparer(IComparer<NumericRange<T>> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public NumericRangeComparer(IComparer<NumericRange<T>> comparer, IEqualityComparer<NumericRange<T>> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(NumericRange<T> x, NumericRange<T> y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			int n = x.Minimum.CompareTo(y.Minimum);
			return n != 0 ? n : x.Maximum.CompareTo(y.Maximum);
		}

		public override bool Equals(NumericRange<T> x, NumericRange<T> y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null) return false;
			if (y == null) return false;
			return x.Minimum.Equals(y.Minimum) && x.Maximum.Equals(y.Maximum);
		}

		public override int GetHashCode(NumericRange<T> obj)
		{
			unchecked
			{
				int hash = 397;
				hash = (hash * 397) ^ obj.Minimum.GetHashCode();
				if (obj.IsRange) hash = (hash * 397) ^ obj.Maximum.GetHashCode();
				return hash;
			}
		}
	}
}
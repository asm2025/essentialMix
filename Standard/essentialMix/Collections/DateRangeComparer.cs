using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections
{
	public class DateRangeComparer : GenericComparer<DateRange>
	{
		public new static DateRangeComparer Default { get; } = new DateRangeComparer();

		/// <inheritdoc />
		public DateRangeComparer() 
		{
		}

		/// <inheritdoc />
		public DateRangeComparer(IComparer<DateRange> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public DateRangeComparer(IComparer<DateRange> comparer, IEqualityComparer<DateRange> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(DateRange x, DateRange y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			int n = x.Unit.CompareTo(y.Unit);
			if (n != 0) return n;
			n = x.Minimum.CompareTo(y.Minimum);
			return n != 0 ? n : x.Maximum.CompareTo(y.Maximum);
		}

		public override bool Equals(DateRange x, DateRange y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return x.Unit == y.Unit && x.Minimum.Equals(y.Minimum) && x.Maximum.Equals(y.Maximum);
		}

		public override int GetHashCode(DateRange obj)
		{
			unchecked
			{
				int hash = 397;
				hash = (hash * 397) ^ obj.Unit.GetHashCode();
				hash = (hash * 397) ^ obj.Minimum.GetHashCode();
				hash = (hash * 397) ^ obj.Maximum.GetHashCode();
				return hash;
			}
		}
	}
}
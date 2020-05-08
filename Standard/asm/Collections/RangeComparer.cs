using System;
using asm.Comparers;
using asm.Extensions;
using asm.Helpers;

namespace asm.Collections
{
	public sealed class RangeComparer<T> : GenericComparer<IReadOnlyRange<T>>
		where T : struct, IComparable
	{
		public new static RangeComparer<T> Default { get; } = new RangeComparer<T>();

		/// <inheritdoc />
		public RangeComparer() 
		{
		}

		public override int Compare(IReadOnlyRange<T> x, IReadOnlyRange<T> y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			int n = Compare(x.Minimum, y.Minimum);
			return n != 0 ? n : Compare(x.Maximum, y.Maximum);
		}

		public int Compare(IReadOnlyRange<T> x, T y)
		{
			if (x == null) return -1;
			int n = Compare(x.Minimum, y);
			if (n == 0) return 0;
			if (n > 0) return -1;
			return Compare(x.Maximum, y);
		}

		public override bool Equals(IReadOnlyRange<T> x, IReadOnlyRange<T> y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return Compare(x.Minimum, y.Minimum) == 0 && Compare(x.Maximum, y.Maximum) == 0;
		}

		public bool Equals(IReadOnlyRange<T> x, T y)
		{
			return Compare(x, y) == 0;
		}

		public override int GetHashCode(IReadOnlyRange<T> obj)
		{
			return ValueTypeHelper.MakeLong(obj.Minimum.GetHashCode(), obj.Maximum.GetHashCode()).Lo();
		}

		private static int Compare(T x, T y)
		{
			return x.CompareTo(y);
		}
	}
}
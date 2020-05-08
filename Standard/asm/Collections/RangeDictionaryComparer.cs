using System;
using asm.Comparers;
using asm.Extensions;
using asm.Helpers;

namespace asm.Collections
{
	public sealed class RangeDictionaryComparer<T> : GenericComparer<(T Minimum, T Maximum)>
		where T : IComparable
	{
		public new static RangeDictionaryComparer<T> Default { get; } = new RangeDictionaryComparer<T>();

		/// <inheritdoc />
		public RangeDictionaryComparer()
		{
		}

		public override int Compare((T Minimum, T Maximum) x, (T Minimum, T Maximum) y)
		{
			int n = Compare(x.Minimum, y.Minimum);
			return n != 0 ? n : Compare(x.Maximum, y.Maximum);
		}

		public int Compare((T Minimum, T Maximum) x, T y)
		{
			int n = Compare(x.Minimum, y);
			if (n == 0) return 0;
			if (n > 0) return -1;
			n = Compare(x.Maximum, y);
			if (n < 0) return 1;
			return 0;
		}

		public int Compare(T x, T y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			return x.CompareTo(y);
		}

		public override bool Equals((T Minimum, T Maximum) x, (T Minimum, T Maximum) y)
		{
			return Compare(x.Minimum, y.Minimum) == 0 && Compare(x.Maximum, y.Maximum) == 0;
		}

		public bool Equals((T Minimum, T Maximum) x, T y)
		{
			return Compare(x, y) == 0;
		}

		public bool Equals(T x, T y)
		{
			return Compare(x, y) == 0;
		}

		public override int GetHashCode((T Minimum, T Maximum) obj)
		{
			return ValueTypeHelper.MakeLong(obj.Minimum?.GetHashCode() ?? 0, obj.Maximum?.GetHashCode() ?? 0).Lo();
		}
	}
}
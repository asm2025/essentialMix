using System;
using System.Collections.Generic;
using asm.Comparers;

namespace asm.Collections
{
	public class EnumRangeComparer<T> : GenericComparer<EnumRange<T>>
		where T : struct, Enum, IComparable
	{
		public new static EnumRangeComparer<T> Default { get; } = new EnumRangeComparer<T>();

		private static readonly IComparer<T> __comparer = Comparer<T>.Default;
		private static readonly IEqualityComparer<T> __equalityComparer = EqualityComparer<T>.Default;

		/// <inheritdoc />
		public EnumRangeComparer() 
		{
		}

		/// <inheritdoc />
		public EnumRangeComparer(IComparer<EnumRange<T>> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public EnumRangeComparer(IComparer<EnumRange<T>> comparer, IEqualityComparer<EnumRange<T>> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(EnumRange<T> x, EnumRange<T> y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			int n = __comparer.Compare(x.Minimum, y.Minimum);
			return n != 0 ? n : __comparer.Compare(x.Maximum, y.Maximum);
		}

		public override bool Equals(EnumRange<T> x, EnumRange<T> y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return __equalityComparer.Equals(x.Minimum, y.Minimum) && __equalityComparer.Equals(x.Maximum, y.Maximum);
		}

		public override int GetHashCode(EnumRange<T> obj)
		{
			unchecked
			{
				return __equalityComparer.GetHashCode(obj.Minimum) + __equalityComparer.GetHashCode(obj.Maximum);
			}
		}
	}
}
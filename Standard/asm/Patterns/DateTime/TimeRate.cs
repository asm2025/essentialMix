using System;
using JetBrains.Annotations;

namespace asm.Patterns.DateTime
{
	public struct TimeRate : IComparable<TimeRate>, IComparable, IEquatable<TimeRate>
	{
		/// <inheritdoc />
		public TimeRate(TimeSpan start, TimeSpan end, decimal rate)
			: this(new TimeRange(start, end), rate)
		{
		}

		public TimeRate(TimeRange range, decimal rate)
		{
			Range = range;
			Rate = rate;
		}

		public TimeRange Range { get; set; }
		public decimal Rate { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return $"{Range} => {Rate:N}"; }

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			return !(obj is TimeRate r)
						? -1
						: CompareTo(r);
		}

		/// <inheritdoc />
		public int CompareTo(TimeRate other)
		{
			int n = Range.CompareTo(other.Range);
			return n != 0 ? n : Rate.CompareTo(other.Rate);
		}

		/// <inheritdoc />
		public bool Equals(TimeRate other) { return Range.Equals(other.Range) && Rate.Equals(other.Rate); }
	}
}
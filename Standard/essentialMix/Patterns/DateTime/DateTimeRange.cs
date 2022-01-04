using System;
using JetBrains.Annotations;

namespace essentialMix.Patterns.DateTime;

public readonly struct DateTimeRange : IComparable<DateTimeRange>, IComparable, IEquatable<DateTimeRange>
{
	public DateTimeRange(System.DateTime start, System.DateTime end)
	{
		Start = start;
		End = end;
	}

	public System.DateTime Start { get; }
	public System.DateTime End { get; }

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return $"{Start:d} - {End:d}"; }

	/// <inheritdoc />
	int IComparable.CompareTo(object obj)
	{
		return obj is not DateTimeRange r
					? -1
					: CompareTo(r);
	}

	/// <inheritdoc />
	public int CompareTo(DateTimeRange other)
	{
		int n = Start.CompareTo(other.Start);
		return n != 0 ? n : End.CompareTo(other.End);
	}

	/// <inheritdoc />
	public bool Equals(DateTimeRange other) { return Start == other.Start && End == other.End; }

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		return obj is DateTimeRange other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (Start.GetHashCode() * 397) ^ End.GetHashCode();
		}
	}

	public bool Overlaps(DateTimeRange other) { return Overlaps(other.Start, other.End); }

	public bool Overlaps(System.DateTime start, System.DateTime end) { return Contains(start) || Contains(end); }

	public bool Contains(System.DateTime value)
	{
		return Start <= End
					? value >= Start && value <= End
					: value >= Start || value <= End;
	}

	public TimeSpan Intersect(DateTimeRange other) { return Intersect(other.Start, other.End); }

	public TimeSpan Intersect(System.DateTime start, System.DateTime end)
	{
		System.DateTime start1, end1;
		System.DateTime start2, end2;

		if (Start > End)
		{
			start1 = End;
			end1 = Start;
		}
		else
		{
			start1 = Start;
			end1 = End;
		}

		if (start > end)
		{
			start2 = end;
			end2 = start;
		}
		else
		{
			start2 = start;
			end2 = end;
		}

		System.DateTime startF = start1 > start2 ? start1 : start2, endF = end1 < end2 ? end1 : end2;
		return startF >= endF ? TimeSpan.Zero : endF - startF;
	}

	public DateTimeRange Union(DateTimeRange other) { return Union(other.Start, other.End); }

	public DateTimeRange Union(System.DateTime start, System.DateTime end)
	{
		System.DateTime start1, end1;
		System.DateTime start2, end2;

		if (Start > End)
		{
			start1 = End;
			end1 = Start;
		}
		else
		{
			start1 = Start;
			end1 = End;
		}

		if (start > end)
		{
			start2 = end;
			end2 = start;
		}
		else
		{
			start2 = start;
			end2 = end;
		}

		System.DateTime startF = start1 < start2 ? start1 : start2, endF = end1 > end2 ? end1 : end2;
		return new DateTimeRange(startF, endF);
	}

	public DateTimeRange Shift(TimeSpan value) { return new DateTimeRange(Start + value, End + value); }

	public DateTimeRange Inflate(TimeSpan value) { return new DateTimeRange(Start - value, End + value); }

	public static bool operator ==(DateTimeRange x, DateTimeRange y) { return x.Equals(y); }

	public static bool operator !=(DateTimeRange x, DateTimeRange y) { return !x.Equals(y); }

	public static bool operator >(DateTimeRange x, DateTimeRange y) { return x.CompareTo(y) > 0; }

	public static bool operator <(DateTimeRange x, DateTimeRange y) { return x.CompareTo(y) < 0; }

	public static bool operator >=(DateTimeRange x, DateTimeRange y) { return x.CompareTo(y) >= 0; }

	public static bool operator <=(DateTimeRange x, DateTimeRange y) { return x.CompareTo(y) <= 0; }

	/// <summary>
	/// Shifts the range by value
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static DateTimeRange operator +(DateTimeRange x, TimeSpan value) { return new DateTimeRange(x.Start, x.End).Shift(value); }

	/// <summary>
	/// Shifts the range by -(value)
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static DateTimeRange operator -(DateTimeRange x, TimeSpan value) { return new DateTimeRange(x.Start, x.End).Shift(-value); }

	/// <summary>
	/// Inflates the range by value
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static DateTimeRange operator *(DateTimeRange x, TimeSpan value) { return new DateTimeRange(x.Start, x.End).Inflate(value); }

	/// <summary>
	/// Inflates the range by -(value)
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static DateTimeRange operator /(DateTimeRange x, TimeSpan value) { return new DateTimeRange(x.Start, x.End).Inflate(-value); }

	/// <summary>
	/// Gets the remaining time between the date and the range end
	/// If Start is greater than End, the range will flip start and end.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static TimeSpan operator %(DateTimeRange x, System.DateTime value)
	{
		System.DateTime end = x.Start > x.End ? x.Start : x.End;
		return value >= end ? TimeSpan.Zero : end - value;
	}

	public static DateTimeRange operator >>(DateTimeRange x, int minutes)
	{
		if (minutes > 0) x.Shift(TimeSpan.FromMinutes(minutes));
		return x;
	}

	public static DateTimeRange operator <<(DateTimeRange x, int minutes)
	{
		if (minutes < 0) x.Shift(TimeSpan.FromMinutes(minutes));
		return x;
	}
}
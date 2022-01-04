using System;
using JetBrains.Annotations;

namespace essentialMix.Patterns.DateTime;

public readonly struct TimeRange : IComparable<TimeRange>, IComparable, IEquatable<TimeRange>
{
	public TimeRange(TimeSpan start, TimeSpan end)
	{
		Start = start;
		End = end;
	}

	public TimeSpan Start { get; }
	public TimeSpan End { get; }

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return $"{Start:g} - {End:g}"; }

	/// <inheritdoc />
	public int CompareTo(object obj)
	{
		return obj is not TimeRange r
					? -1
					: CompareTo(r);
	}

	/// <inheritdoc />
	public int CompareTo(TimeRange other)
	{
		int n = Start.CompareTo(other.Start);
		return n != 0 ? n : End.CompareTo(other.End);
	}

	/// <inheritdoc />
	public bool Equals(TimeRange other) { return Start == other.Start && End == other.End; }

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		return obj is TimeRange other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (Start.GetHashCode() * 397) ^ End.GetHashCode();
		}
	}

	public bool Overlaps(TimeRange other) { return Overlaps(other.Start, other.End); }

	public bool Overlaps(TimeSpan start, TimeSpan end) { return Contains(start) || Contains(end); }

	public bool Contains(System.DateTime value) { return Contains(value.TimeOfDay); }

	public bool Contains(TimeSpan value)
	{
		return Start <= End
					? value >= Start && value <= End
					: value >= Start || value <= End;
	}

	public TimeSpan Intersect(TimeRange other) { return Intersect(other.Start, other.End); }

	public TimeSpan Intersect(TimeSpan start, TimeSpan end)
	{
		// Loops back input from 23:59 - 00:00
		if (Start > End)
		{
			return new TimeRange(Start, TimeSpan.FromHours(24)).Intersect(start, end)
					+ new TimeRange(TimeSpan.Zero, End).Intersect(start, end);
		}

		// Loops back Shift from 23:59 - 00:00
		if (start > end)
		{
			return Intersect(TimeSpan.Zero, end)
					+ Intersect(start, TimeSpan.FromHours(24));
		}

		if (Start > end || End < start) return TimeSpan.Zero;

		TimeSpan actualStart = Start < start ? start : Start;
		TimeSpan actualEnd = End > end ? end : End;
		return actualEnd - actualStart;
	}

	public TimeRange Union(TimeRange other) { return Union(other.Start, other.End); }

	public TimeRange Union(TimeSpan start, TimeSpan end)
	{
		System.DateTime baseDate = System.DateTime.Today.Date;
		System.DateTime start1 = baseDate + Start, start2 = baseDate + start;
		System.DateTime end1 = Start > End ? baseDate.AddDays(1) + End : baseDate + End;
		System.DateTime end2 = start > end ? baseDate.AddDays(1) + end : baseDate + end;
		TimeSpan startF = (start1 < start2 ? start1 : start2).TimeOfDay, endF = (end1 > end2 ? end1 : end2).TimeOfDay;
		return new TimeRange(startF, endF);
	}

	public TimeRange Shift(TimeSpan value) { return new TimeRange(Start + value, End + value); }

	public TimeRange Inflate(TimeSpan value) { return new TimeRange(Start + value, End + value); }

	public DateTimeRange ToDateTimeRange(System.DateTime baseDate)
	{
		return new DateTimeRange(baseDate + Start, Start > End
														? baseDate.AddDays(1) + End
														: baseDate + End);
	}

	public static bool operator ==(TimeRange x, TimeRange y) { return x.Equals(y); }

	public static bool operator !=(TimeRange x, TimeRange y) { return !x.Equals(y); }

	public static bool operator >(TimeRange x, TimeRange y) { return x.CompareTo(y) > 0; }

	public static bool operator <(TimeRange x, TimeRange y) { return x.CompareTo(y) < 0; }

	public static bool operator >=(TimeRange x, TimeRange y) { return x.CompareTo(y) >= 0; }

	public static bool operator <=(TimeRange x, TimeRange y) { return x.CompareTo(y) <= 0; }

	/// <summary>
	/// Shifts the range by value
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static TimeRange operator +(TimeRange x, TimeSpan value) { return new TimeRange(x.Start, x.End).Shift(value); }

	/// <summary>
	/// Shifts the range by -(value)
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static TimeRange operator -(TimeRange x, TimeSpan value) { return new TimeRange(x.Start, x.End).Shift(-value); }

	/// <summary>
	/// Inflates the range by value
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static TimeRange operator *(TimeRange x, TimeSpan value) { return new TimeRange(x.Start, x.End).Inflate(value); }

	/// <summary>
	/// Inflates the range by -(value)
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static TimeRange operator /(TimeRange x, TimeSpan value) { return new TimeRange(x.Start, x.End).Inflate(-value); }

	/// <summary>
	/// Gets the remaining time between the time span and the range end
	/// If Start is greater than End, the range spans to the next day.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="value"></param>
	public static TimeSpan operator %(TimeRange x, TimeSpan value)
	{
		System.DateTime baseDate = System.DateTime.Today.Date;
		System.DateTime rangeEnd = x.Start < x.End
										? baseDate + x.End
										: baseDate.AddDays(1) + x.End;
		System.DateTime dateEnd = baseDate + value;
		return rangeEnd <= dateEnd ? TimeSpan.Zero : rangeEnd - dateEnd;
	}

	public static TimeRange operator >>(TimeRange x, int minutes)
	{
		if (minutes > 0) x.Shift(TimeSpan.FromMinutes(minutes));
		return x;
	}

	public static TimeRange operator <<(TimeRange x, int minutes)
	{
		if (minutes < 0) x.Shift(TimeSpan.FromMinutes(minutes));
		return x;
	}
}
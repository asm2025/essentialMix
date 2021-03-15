using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.DateTime;

namespace essentialMix.Collections
{
	[Serializable]
	[TypeConverter(typeof(DisplayNameExpandableObjectConverter))]
	public class DateRange : Range<DateTime>
	{
		public DateRange(DateTimeUnit unit)
			: this(DateTime.MinValue, DateTime.MaxValue, unit)
		{
		}

		public DateRange(DateTime entry, DateTimeUnit unit)
			: this(entry, entry, unit)
		{
		}

		public DateRange([NotNull] DateRange range)
			: this(range.Minimum, range.Maximum, range.Unit)
		{
		}

		public DateRange([NotNull] IReadOnlyRange<DateTime> range, DateTimeUnit unit)
			: this(range.Minimum, range.Maximum, unit)
		{
		}

		public DateRange([NotNull] LambdaRange<DateTime> range, DateTimeUnit unit)
			: this(range.Minimum, range.Maximum, unit)
		{
		}

		public DateRange(DateTime? start, DateTimeUnit unit)
			: this(start, null, unit)
		{
		}

		public DateRange(DateTime? start, DateTime? end, DateTimeUnit unit)
			: base(start ?? DateTime.MinValue, end ?? DateTime.MaxValue)
		{
			if (!unit.InRange(DateTimeUnit.Millisecond, DateTimeUnit.Year)) throw new ArgumentOutOfRangeException(nameof(unit));
			Unit = unit;
		}

		public DateTimeUnit Unit { get; }

		public override string ToString() { return string.Join(Range.SPLIT.ToString(), Minimum.ToDateString(), Maximum.ToDateString(), Unit); }

		public override int GetHashCode() { return DateRangeComparer.Default.GetHashCode(this); }

		public override bool Equals(object obj) { return obj != null && (ReferenceEquals(this, obj) || Equals(obj as IReadOnlyRange<DateTime>)); }

		public override bool Merge(IReadOnlyRange<DateTime> other)
		{
			if (ReferenceEquals(this, other)) return false;
			if (other.IsEmpty || Contains(other)) return true;

			if (Overlaps(other))
			{
				DateTime min = Minimum.NotAbove(other.Minimum);
				DateTime max = Maximum.NotBelow(other.Maximum.Maximum(min));
				if (min == Minimum && max == Maximum) return true;
				Minimum = min;
				Maximum = max;
				return true;
			}

			if (IsPreviousTo(other))
			{
				Maximum = other.Maximum;
				return true;
			}

			if (!IsNextTo(other)) return false;
			Minimum = other.Minimum;
			return true;
		}

		public override void ShiftBy(DateTime date)
		{
			switch (Unit)
			{
				case DateTimeUnit.Millisecond:
					ShiftByMilliseconds(date.Date.Milliseconds(DateTime.Today));
					break;
				case DateTimeUnit.Second:
					ShiftBySeconds(date.Date.Seconds(DateTime.Today));
					break;
				case DateTimeUnit.Minute:
					ShiftByMinutes((int)date.Date.Minutes(DateTime.Today));
					break;
				case DateTimeUnit.Hour:
					ShiftByHours((int)date.Date.Hours(DateTime.Today));
					break;
				case DateTimeUnit.Day:
					ShiftByDays((int)date.Date.Days(DateTime.Today));
					break;
				case DateTimeUnit.Month:
					ShiftByMonths((int)date.Date.Months(DateTime.Today));
					break;
				case DateTimeUnit.Year:
					ShiftByYears((int)date.Date.Years(DateTime.Today));
					break;
			}
		}

		public void ShiftBy(TimeSpan time)
		{
			switch (Unit)
			{
				case DateTimeUnit.Millisecond:
					ShiftByMilliseconds(time.TotalMilliseconds);
					break;
				case DateTimeUnit.Second:
					ShiftBySeconds(time.TotalSeconds);
					break;
				case DateTimeUnit.Minute:
					ShiftByMinutes((int)time.Minutes());
					break;
				case DateTimeUnit.Hour:
					ShiftByHours((int)time.Hours());
					break;
				case DateTimeUnit.Day:
					ShiftByDays((int)time.Days());
					break;
				case DateTimeUnit.Month:
					ShiftByMonths((int)time.Months());
					break;
				case DateTimeUnit.Year:
					ShiftByYears((int)time.Years());
					break;
			}
		}

		public void ShiftBy(double value)
		{
			switch (Unit)
			{
				case DateTimeUnit.Millisecond:
					ShiftByMilliseconds(value);
					break;
				case DateTimeUnit.Second:
					ShiftBySeconds(value);
					break;
				case DateTimeUnit.Minute:
					ShiftByMinutes((int)value);
					break;
				case DateTimeUnit.Hour:
					ShiftByHours((int)value);
					break;
				case DateTimeUnit.Day:
					ShiftByDays((int)value);
					break;
				case DateTimeUnit.Month:
					ShiftByMonths((int)value);
					break;
				case DateTimeUnit.Year:
					ShiftByYears((int)value);
					break;
			}
		}

		public void ShiftByMilliseconds(double value)
		{
			if (value > 0.0d)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddMilliseconds((int)value.NotAbove((long)(Bounds.Maximum - Minimum).Milliseconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMilliseconds((int)value.NotAbove((long)(Bounds.Maximum - Maximum).Milliseconds()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0.0d)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddMilliseconds((int)value.NotBelow((long)(Bounds.Minimum - Minimum).Milliseconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMilliseconds((int)value.NotBelow((long)(Bounds.Minimum - Maximum).Milliseconds()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void ShiftBySeconds(double value)
		{
			if (value > 0.0d)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddSeconds(value.NotAbove((Bounds.Maximum - Minimum).Seconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddSeconds(value.NotAbove((Bounds.Maximum - Maximum).Seconds()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0.0d)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddSeconds(value.NotBelow((Bounds.Minimum - Minimum).Seconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddSeconds(value.NotBelow((Bounds.Minimum - Maximum).Seconds()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void ShiftByMinutes(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddMinutes(value.NotAbove((int)(Bounds.Maximum - Minimum).Minutes()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMinutes(value.NotAbove((int)(Bounds.Maximum - Maximum).Minutes()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddMinutes(value.NotBelow((int)(Bounds.Minimum - Minimum).Minutes()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMinutes(value.NotBelow((int)(Bounds.Minimum - Maximum).Minutes()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void ShiftByHours(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddHours(value.NotAbove((int)(Bounds.Maximum - Minimum).Hours()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddHours(value.NotAbove((int)(Bounds.Maximum - Maximum).Hours()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddHours(value.NotBelow((int)(Bounds.Minimum - Minimum).Hours()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddHours(value.NotBelow((int)(Bounds.Minimum - Maximum).Hours()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void ShiftByDays(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddDays(value.NotAbove((int)(Bounds.Maximum - Minimum).Days()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddDays(value.NotAbove((int)(Bounds.Maximum - Maximum).Days()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddDays(value.NotBelow((int)(Bounds.Minimum - Minimum).Days()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddDays(value.NotBelow((int)(Bounds.Minimum - Maximum).Days()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void ShiftByMonths(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddMonths(value.NotAbove((int)(Bounds.Maximum - Minimum).Months()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMonths(value.NotAbove((int)(Bounds.Maximum - Maximum).Months()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddMonths(value.NotBelow((int)(Bounds.Minimum - Minimum).Months()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMonths(value.NotBelow((int)(Bounds.Minimum - Maximum).Months()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void ShiftByYears(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* increase min, max by the amount of steps only
				* if they will be less than or equal bounds max
				*/
				if (Minimum.TryAddYears(value.NotAbove((int)(Bounds.Maximum - Minimum).Years()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddYears(value.NotAbove((int)(Bounds.Minimum - Maximum).Years()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* decrease min, max by the amount of steps only
				* if they will be greater than or equal bounds min
				*/
				// This is not a mistake, I'll actually add a negative to a negative
				if (Minimum.TryAddYears(value.NotBelow((int)(Bounds.Minimum - Minimum).Years()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddYears(value.NotBelow((int)(Bounds.Minimum - Maximum).Years()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public override void InflateBy(DateTime date)
		{
			switch (Unit)
			{
				case DateTimeUnit.Millisecond:
					InflateByMilliseconds(date.Date.Milliseconds(DateTime.Today));
					break;
				case DateTimeUnit.Second:
					InflateBySeconds(date.Date.Seconds(DateTime.Today));
					break;
				case DateTimeUnit.Minute:
					InflateByMinutes((int)date.Date.Minutes(DateTime.Today));
					break;
				case DateTimeUnit.Hour:
					InflateByHours((int)date.Date.Hours(DateTime.Today));
					break;
				case DateTimeUnit.Day:
					InflateByDays((int)date.Date.Days(DateTime.Today));
					break;
				case DateTimeUnit.Month:
					InflateByMonths((int)date.Date.Months(DateTime.Today));
					break;
				case DateTimeUnit.Year:
					InflateByYears((int)date.Date.Years(DateTime.Today));
					break;
			}
		}

		public void InflateBy(TimeSpan time)
		{
			switch (Unit)
			{
				case DateTimeUnit.Millisecond:
					InflateByMilliseconds(time.TotalMilliseconds);
					break;
				case DateTimeUnit.Second:
					InflateBySeconds(time.TotalSeconds);
					break;
				case DateTimeUnit.Minute:
					InflateByMinutes((int)time.Minutes());
					break;
				case DateTimeUnit.Hour:
					InflateByHours((int)time.Hours());
					break;
				case DateTimeUnit.Day:
					InflateByDays((int)time.Days());
					break;
				case DateTimeUnit.Month:
					InflateByMonths((int)time.Months());
					break;
				case DateTimeUnit.Year:
					InflateByYears((int)time.Years());
					break;
			}
		}

		public void InflateBy(double value)
		{
			switch (Unit)
			{
				case DateTimeUnit.Millisecond:
					InflateByMilliseconds(value);
					break;
				case DateTimeUnit.Second:
					InflateBySeconds(value);
					break;
				case DateTimeUnit.Minute:
					InflateByMinutes((int)value);
					break;
				case DateTimeUnit.Hour:
					InflateByHours((int)value);
					break;
				case DateTimeUnit.Day:
					InflateByDays((int)value);
					break;
				case DateTimeUnit.Month:
					InflateByMonths((int)value);
					break;
				case DateTimeUnit.Year:
					InflateByYears((int)value);
					break;
			}
		}

		public void InflateByMilliseconds(double value)
		{
			if (value > 0.0d)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddMilliseconds(-value.NotBelow((Bounds.Minimum - Minimum).Milliseconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMilliseconds(value.NotAbove((Bounds.Maximum - Maximum).Milliseconds()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0.0d)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddMilliseconds(-value.NotAbove((Bounds.Maximum - Minimum).Milliseconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMilliseconds(value.NotBelow((Bounds.Minimum - Maximum).Milliseconds()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void InflateBySeconds(double value)
		{
			if (value > 0.0d)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddSeconds(-value.NotBelow((Bounds.Minimum - Minimum).Seconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddSeconds(value.NotAbove((Bounds.Maximum - Maximum).Seconds()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0.0d)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddSeconds(-value.NotAbove((Bounds.Maximum - Minimum).Seconds()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddSeconds(value.NotBelow((Bounds.Minimum - Maximum).Seconds()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void InflateByMinutes(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddMinutes(-value.NotBelow((int)(Bounds.Minimum - Minimum).Minutes()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMinutes(value.NotAbove((int)(Bounds.Maximum - Maximum).Minutes()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddMinutes(-value.NotAbove((int)(Bounds.Maximum - Minimum).Minutes()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMinutes(value.NotBelow((int)(Bounds.Minimum - Maximum).Minutes()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void InflateByHours(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddHours(-value.NotBelow((int)(Bounds.Minimum - Minimum).Hours()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddHours(value.NotAbove((int)(Bounds.Maximum - Maximum).Hours()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddHours(-value.NotAbove((int)(Bounds.Maximum - Minimum).Hours()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddHours(value.NotBelow((int)(Bounds.Minimum - Maximum).Hours()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void InflateByDays(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddDays(-value.NotBelow((int)(Bounds.Minimum - Minimum).Days()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddDays(value.NotAbove((int)(Bounds.Maximum - Maximum).Days()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddDays(-value.NotAbove((int)(Bounds.Maximum - Minimum).Days()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddDays(value.NotBelow((int)(Bounds.Minimum - Maximum).Days()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void InflateByMonths(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddMonths(-value.NotBelow((int)(Bounds.Minimum - Minimum).Months()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMonths(value.NotAbove((int)(Bounds.Maximum - Maximum).Months()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddMonths(-value.NotAbove((int)(Bounds.Maximum - Minimum).Months()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddMonths(value.NotBelow((int)(Bounds.Minimum - Maximum).Months()), out DateTime dtx)) Maximum = dtx;
			}
		}

		public void InflateByYears(int value)
		{
			if (value > 0)
			{
				/*
				* steps is positive
				* decrease min, increase max by the amount of steps
				*/
				if (Minimum.TryAddYears(-value.NotBelow((int)(Bounds.Minimum - Minimum).Years()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddYears(value.NotAbove((int)(Bounds.Maximum - Maximum).Years()), out DateTime dtx)) Maximum = dtx;
			}
			else if (value < 0)
			{
				/*
				* steps is negative
				* increase min, decrease max by the amount of steps
				*/
				if (Minimum.TryAddYears(-value.NotAbove((int)(Bounds.Maximum - Minimum).Years()), out DateTime dtn)) Minimum = dtn;
				if (Maximum.TryAddYears(value.NotBelow((int)(Bounds.Minimum - Maximum).Years()), out DateTime dtx)) Maximum = dtx;
			}
		}

		protected override RangeLister<DateTime> Lister => new DateRangeLister(this);

		public static DateRange Parse(string value)
		{
			IsWellFormattedRangeString(value, out DateRange parsed);
			return parsed;
		}

		[NotNull]
		public static ICollection<DateRange> ParseGroup(string value)
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return Array.Empty<DateRange>();

			if (!value.Contains(Range.GROUP))
			{
				if (!IsWellFormattedRangeString(value, out DateRange range)) return Array.Empty<DateRange>();
				return new Collection<DateRange> {range};
			}

			ICollection<DateRange> collection = new Collection<DateRange>();
			IsWellFormattedRangeGroupString(value, collection);
			return collection;
		}

		public static bool IsWellFormattedRangeString(string value) { return IsWellFormattedRangeString(value, out _); }

		private static bool IsWellFormattedRangeString(string value, out DateRange parsed)
		{
			parsed = null;
			if (string.IsNullOrEmpty(value)) return false;

			if (!value.Contains(Range.SPLIT))
			{
				if (!DateTimeHelper.TryParseDateString(value, out DateTime p)) return false;
				parsed = new DateRange(p, DateTimeUnit.None);
				return true;
			}

			string[] parts = value.Split(3, Range.SPLIT);
			if (!parts.Length.InRange(2, 3) || parts.Any(string.IsNullOrEmpty)) return false;
			if (!DateTimeHelper.TryParseDateString(value, out DateTime p1) || !DateTimeHelper.TryParseDateString(value, out DateTime p2)) return false;
			
			DateTimeUnit unit = DateTimeUnit.None;
			
			if (parts.Length == 3)
			{
				if (!Enum.TryParse(parts[2], true, out unit)) return false;
			}

			parsed = new DateRange(p1, p2, unit);
			return true;
		}

		public static bool IsWellFormattedRangeGroupString(string value) { return IsWellFormattedRangeGroupString(value, null); }

		private static bool IsWellFormattedRangeGroupString(string value, ICollection<DateRange> parsed)
		{
			if (string.IsNullOrEmpty(value)) return false;

			if (!value.Contains(Range.GROUP))
			{
				if (!IsWellFormattedRangeString(value, out DateRange p)) return false;
				parsed?.Add(p);
				return true;
			}

			Func<string, bool> predicate;

			if (parsed == null)
				predicate = IsWellFormattedRangeString;
			else
			{
				predicate = s =>
							{
								if (!IsWellFormattedRangeString(s, out DateRange p)) return false;
								parsed.Add(p);
								return true;
							};
			}

			return value.All(Range.GROUP, predicate);
		}

		public static bool operator ==(DateRange x, DateRange y) { return DateRangeComparer.Default.Equals(x, y); }

		public static bool operator !=(DateRange x, DateRange y) { return !(x == y); }

		public static bool operator >([NotNull] DateRange x, DateRange y) { return x.IsIterable && DateRangeComparer.Default.Compare(x, y) == 1; }

		public static bool operator <([NotNull] DateRange x, DateRange y) { return x.IsIterable && DateRangeComparer.Default.Compare(x, y) == -1; }

		public static bool operator >=([NotNull] DateRange x, DateRange y) { return x.IsIterable && DateRangeComparer.Default.Compare(x, y) > -1; }

		public static bool operator <=([NotNull] DateRange x, DateRange y) { return x.IsIterable && DateRangeComparer.Default.Compare(x, y) < 1; }

		public static DateRange operator ++(DateRange value)
		{
			if (value == null) return null;
			value.InflateBy(1);
			return value;
		}

		public static DateRange operator --(DateRange value)
		{
			if (value == null) return null;
			value.InflateBy(-1);
			return value;
		}

		public static DateRange operator +(DateRange x, DateRange y)
		{
			if (x == null || y == null) return null;
			if (ReferenceEquals(x, y) || y.IsEmpty) return x;
			DateTime min = x.Minimum.Minimum(y.Minimum);
			DateTime max = x.Maximum.Maximum(y.Maximum);
			return new DateRange(min, max, x.Unit);
		}

		public static DateRange[] operator -(DateRange x, DateRange y)
		{
			if (x == null || y == null) return null;
			if (ReferenceEquals(x, y) || x.IsEmpty || y.IsEmpty || !x.Overlaps(y)) return new[] { x };

			if (x.Minimum.IsGreaterThanOrEqual(y.Minimum) && x.Maximum.IsLessThanOrEqual(y.Maximum)) return new[] { new DateRange(DateTimeUnit.None) };

			if (x.Minimum.IsLessThan(y.Minimum) && x.Maximum.IsGreaterThan(y.Maximum))
			{
				return new[]
				{
					new DateRange(x.Minimum, y.Minimum, x.Unit),
					new DateRange(y.Maximum, x.Maximum, x.Unit)
				};
			}

			return x.Minimum.IsLessThanOrEqual(y.Minimum)
				? new[] { new DateRange(x.Minimum, y.Minimum, x.Unit) }
				: new[] { new DateRange(y.Maximum, x.Maximum, x.Unit) };
		}
	}
}
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using asm.Helpers;
using asm.Patterns.DateTime;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class DateTimeExtension
	{ 
		private const string FMT_DATE_TIME = "{2:0000}{0}{3:00}{0}{4:00} {5:00}{1}{6:00}{1}{7:00}";
		private const string FMT_DATE_TIME_DAY_FIRST = "{4:00}{0}{3:00}{0}{2:0000} {5:00}{1}{6:00}{1}{7:00}";
		private const string FMT_DATE = "{1:0000}{0}{2:00}{0}{3:00}";
		private const string FMT_DATE_DAY_FIRST = "{3:00}{0}{2:00}{0}{1:0000}";

		private const string DATE_TIME = "{0:0000}{1:00}{2:00}.{3:00}{4:00}{5:00}";
		private const string DATE_TIME_DAY_FIRST = "{2:00}{1:00}{0:0000}.{3:00}{4:00}{5:00}";
		private const string DATE = "{0:0000}{1:00}{2:00}";
		private const string DATE_DAY_FIRST = "{2:00}{1:00}{0:0000}";

		public static bool IsGreaterThan(this DateTime thisValue, DateTime destination, double limit, DateTimeUnit dateTimeUnit)
		{
			double n = DateTimeHelper.Limit(limit, dateTimeUnit);
			DateTimeUnit unit = n > 0.0D ? dateTimeUnit : DateTimeUnit.None;

			switch (unit)
			{
				case DateTimeUnit.None:
					return thisValue > destination;
				case DateTimeUnit.Year:
					return thisValue > destination.AddYears((int)n);
				case DateTimeUnit.Month:
					return thisValue > destination.AddMonths((int)n);
				case DateTimeUnit.Day:
					return thisValue > destination.AddDays(n);
				case DateTimeUnit.Hour:
					return thisValue > destination.AddHours(n);
				case DateTimeUnit.Minute:
					return thisValue > destination.AddMinutes(n);
				case DateTimeUnit.Second:
					return thisValue > destination.AddSeconds(n);
				case DateTimeUnit.Millisecond:
					return thisValue > destination.AddMilliseconds(n);
				default:
					throw new ArgumentOutOfRangeException(nameof(dateTimeUnit));
			}
		}

		public static bool IsGreaterThanOrEqual(this DateTime thisValue, DateTime destination, double limit, DateTimeUnit dateTimeUnit)
		{
			double n = DateTimeHelper.Limit(limit, dateTimeUnit);
			DateTimeUnit unit = n > 0.0D ? dateTimeUnit : DateTimeUnit.None;

			switch (unit)
			{
				case DateTimeUnit.None:
					return thisValue >= destination;
				case DateTimeUnit.Year:
					return thisValue >= destination.AddYears((int)n);
				case DateTimeUnit.Month:
					return thisValue >= destination.AddMonths((int)n);
				case DateTimeUnit.Day:
					return thisValue >= destination.AddDays(n);
				case DateTimeUnit.Hour:
					return thisValue >= destination.AddHours(n);
				case DateTimeUnit.Minute:
					return thisValue >= destination.AddMinutes(n);
				case DateTimeUnit.Second:
					return thisValue >= destination.AddSeconds(n);
				case DateTimeUnit.Millisecond:
					return thisValue >= destination.AddMilliseconds(n);
				default:
					throw new ArgumentOutOfRangeException(nameof(dateTimeUnit));
			}
		}

		public static bool IsLessThan(this DateTime thisValue, DateTime destination, double limit, DateTimeUnit dateTimeUnit)
		{
			double n = DateTimeHelper.Limit(limit, dateTimeUnit) * -1;
			DateTimeUnit unit = n < 0.0D ? dateTimeUnit : DateTimeUnit.None;

			switch (unit)
			{
				case DateTimeUnit.None:
					return thisValue < destination;
				case DateTimeUnit.Year:
					return thisValue < destination.AddYears((int)n);
				case DateTimeUnit.Month:
					return thisValue < destination.AddMonths((int)n);
				case DateTimeUnit.Day:
					return thisValue < destination.AddDays(n);
				case DateTimeUnit.Hour:
					return thisValue < destination.AddHours(n);
				case DateTimeUnit.Minute:
					return thisValue < destination.AddMinutes(n);
				case DateTimeUnit.Second:
					return thisValue < destination.AddSeconds(n);
				case DateTimeUnit.Millisecond:
					return thisValue < destination.AddMilliseconds(n);
				default:
					throw new ArgumentOutOfRangeException(nameof(dateTimeUnit));
			}
		}

		public static bool IsLessThanOrEqual(this DateTime thisValue, DateTime destination, double limit, DateTimeUnit dateTimeUnit)
		{
			double n = DateTimeHelper.Limit(limit, dateTimeUnit) * -1;
			DateTimeUnit unit = n < 0.0D ? dateTimeUnit : DateTimeUnit.None;

			switch (unit)
			{
				case DateTimeUnit.None:
					return thisValue <= destination;
				case DateTimeUnit.Year:
					return thisValue <= destination.AddYears((int)n);
				case DateTimeUnit.Month:
					return thisValue <= destination.AddMonths((int)n);
				case DateTimeUnit.Day:
					return thisValue <= destination.AddDays(n);
				case DateTimeUnit.Hour:
					return thisValue <= destination.AddHours(n);
				case DateTimeUnit.Minute:
					return thisValue <= destination.AddMinutes(n);
				case DateTimeUnit.Second:
					return thisValue <= destination.AddSeconds(n);
				case DateTimeUnit.Millisecond:
					return thisValue <= destination.AddMilliseconds(n);
				default:
					throw new ArgumentOutOfRangeException(nameof(dateTimeUnit));
			}
		}

		[NotNull]
		public static string ToDateString(this DateTime thisValue) { return ToDateString(thisValue, false, true, false); }

		[NotNull]
		public static string ToDateString(this DateTime thisValue, bool format) { return ToDateString(thisValue, format, true); }

		[NotNull]
		public static string ToDateString(this DateTime thisValue, bool format, bool includeTime) { return ToDateString(thisValue, format, includeTime, false); }

		[NotNull]
		public static string ToDateString(this DateTime thisValue, bool format, bool includeTime, bool dayFirst)
		{
			if (format)
			{
				DateTimeFormatInfo dateTimeFormat = CultureInfoHelper.Default.DateTimeFormat;
				return includeTime
					? string.Format(dayFirst ? FMT_DATE_TIME_DAY_FIRST : FMT_DATE_TIME,
						dateTimeFormat.DateSeparator,
						dateTimeFormat.TimeSeparator,
						thisValue.Year,
						thisValue.Month,
						thisValue.Day,
						thisValue.Hour,
						thisValue.Minute,
						thisValue.Second)
					: string.Format(dayFirst ? FMT_DATE_DAY_FIRST : FMT_DATE,
						CultureInfoHelper.Default.DateTimeFormat.DateSeparator,
						thisValue.Year,
						thisValue.Month,
						thisValue.Day);
			}

			return includeTime
				? string.Format(dayFirst ? DATE_TIME_DAY_FIRST : DATE_TIME,
					thisValue.Year,
					thisValue.Month,
					thisValue.Day,
					thisValue.Hour,
					thisValue.Minute,
					thisValue.Second)
				: string.Format(dayFirst ? DATE_DAY_FIRST : DATE,
					thisValue.Year,
					thisValue.Month,
					thisValue.Day);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double OAValue(this DateTime thisValue)
		{
			if (thisValue < DateTimeHelper.OaEpoch) throw new ArgumentOutOfRangeException(nameof(thisValue));
			return Convert.ToDouble((thisValue - DateTimeHelper.OaEpoch).Ticks) / TimeSpan.TicksPerDay;
		}
		
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int GetWeekNumber(this DateTime thisValue, CultureInfo culture = null)
		{
			CultureInfo ci = culture ?? CultureInfoHelper.Default;
			return ci.Calendar.GetWeekOfYear(thisValue, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int GetWeekNumberOfMonth(this DateTime thisValue, CultureInfo culture = null)
		{
			DateTime first = new DateTime(thisValue.Year, thisValue.Month, 1);
			return thisValue.GetWeekNumber(culture) - first.GetWeekNumber(culture) + 1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int ToInt(this DateTime thisValue) { return thisValue.Year * 10_000 + thisValue.Month * 100 + thisValue.Day; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long ToLong(this DateTime thisValue) { return ToInt(thisValue) * 1_000_000 + thisValue.Hour * 10_000 + thisValue.Minute * 100 + thisValue.Second; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double ToDouble(this DateTime thisValue) { return ToInt(thisValue) + (thisValue.Hour * 10_000 + thisValue.Minute * 100 + thisValue.Second) / 1_000_000d; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Years(this DateTime thisValue, DateTime value) { return (ToInt(thisValue) - ToInt(value)) / 10_000d; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Months(this DateTime thisValue, DateTime value)
		{
			int diff = ToInt(thisValue) - ToInt(value);
			double years = Math.Floor(diff / 10_000d);
			double months = (diff / 10_000d).Fraction();
			return years * 12 + months;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Days(this DateTime thisValue, DateTime value) { return (thisValue - value).TotalDays; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Hours(this DateTime thisValue, DateTime value) { return (thisValue - value).TotalHours; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Minutes(this DateTime thisValue, DateTime value) { return (thisValue - value).TotalMinutes; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Seconds(this DateTime thisValue, DateTime value) { return (thisValue - value).TotalSeconds; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Milliseconds(this DateTime thisValue, DateTime value) { return (thisValue - value).TotalMilliseconds; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DaysInMonth(this DateTime thisValue) { return DateTime.DaysInMonth(thisValue.Year, thisValue.Month); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Quarter(this DateTime thisValue) { return (thisValue.Month - 1) / 3 + 1; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int SemiAnnual(this DateTime thisValue) { return (thisValue.Month - 1) / 6 + 1; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Century(this DateTime thisValue) { return thisValue.Year / 100 + 1; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Millennium(this DateTime thisValue) { return thisValue.Year / 1000 + 1; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime Yesterday(this DateTime thisValue) { return thisValue.AddDays(-1); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime Tomorrow(this DateTime thisValue) { return thisValue.AddDays(1); }

		public static (DateTime Start, DateTime End) PreviousHours(this DateTime thisValue, uint count)
		{
			DateTime end = new DateTime(thisValue.Year, thisValue.Month, thisValue.Day, thisValue.Hour, 0, 0);
			return (end.AddHours(-count), end.AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextHours(this DateTime thisValue, uint count)
		{
			DateTime start = new DateTime(thisValue.Year, thisValue.Month, thisValue.Day, thisValue.Hour, 0, 0).AddHours(1);
			return (start, start.AddHours(count).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousDays(this DateTime thisValue, uint count)
		{
			DateTime start = thisValue.Date;
			return (start.AddDays(-count), start.AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextDays(this DateTime thisValue, uint count)
		{
			DateTime start = thisValue.Date.AddDays(1);
			return (start, start.AddDays(count).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousWeeks(this DateTime thisValue, uint count)
		{
			DateTime start = thisValue.Date.AddDays(-(int)thisValue.DayOfWeek);
			return (start.AddDays(-(count * 7)), start.AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextWeeks(this DateTime thisValue, uint count)
		{
			DateTime start = thisValue.Date.AddDays(-(int)thisValue.DayOfWeek + 7);
			return (start, start.AddDays(count * 7).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousMonths(this DateTime thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime thisStart = new DateTime(thisValue.Year, thisValue.Month, 1);
			return (thisStart.AddMonths(-count), thisStart.AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextMonths(this DateTime thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = new DateTime(thisValue.Year, thisValue.Month, 1).AddMonths(1);
			return (start, start.AddMonths(count).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousYears(this DateTime thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime thisStart = new DateTime(thisValue.Year, thisValue.Month, 1);
			return (thisStart.AddYears(-count), thisStart.AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextYears(this DateTime thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = new DateTime(thisValue.Year, thisValue.Month, 1).AddMonths(1);
			return (start, start.AddYears(count).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousCenturies(this DateTime thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			int century = Century(thisValue) - (1 + count);
			DateTime thisStart = new DateTime(century * 100, 1, 1);
			return (thisStart, thisStart.AddYears(100).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextCenturies(this DateTime thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			int century = Century(thisValue) - 1 + count;
			DateTime thisStart = new DateTime(century * 100, 1, 1);
			return (thisStart, thisStart.AddYears(100).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousWeek(this DateTime thisValue, uint count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = thisValue.Date.AddDays(-((int)thisValue.DayOfWeek + count * 7));
			return (start, start.AddDays(7).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheWeek(this DateTime thisValue)
		{
			return thisValue.Date.AddDays(-(int)thisValue.DayOfWeek);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisWeek(this DateTime thisValue)
		{
			DateTime start = FirstOfTheWeek(thisValue);
			return (start, start.AddDays(7).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextWeek(this DateTime thisValue, uint count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = thisValue.Date.AddDays(-(int)thisValue.DayOfWeek + count * 7);
			return (start, start.AddDays(7).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousMonth(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime thisStart = new DateTime(thisValue.Year, thisValue.Month, 1).AddMonths(-count);
			return (thisStart, thisStart.AddMonths(1).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheMonth(this DateTime thisValue)
		{
			return new DateTime(thisValue.Year, thisValue.Month, 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime LastOfTheMonth(this DateTime thisValue)
		{
			int daysInMonth = DateTime.DaysInMonth(thisValue.Year, thisValue.Month);
			return thisValue.Date.AddDays(1 - thisValue.Day + daysInMonth).AddSeconds(-1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime LastOfTheMonth(this DateTime thisValue, DayOfWeek dayOfWeek)
		{
			DateTime last = LastOfTheMonth(thisValue);
			DayOfWeek[] daysOrder = DateTimeFormatInfo.CurrentInfo!.GetDaysOrder();
			int diff = Array.IndexOf(daysOrder, dayOfWeek) - Array.IndexOf(daysOrder, last.DayOfWeek);
			if (diff > 0) diff -= 7;
			return last.AddDays(diff);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisMonth(this DateTime thisValue)
		{
			DateTime start = FirstOfTheMonth(thisValue);
			return (start, start.AddMonths(1).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextMonth(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = new DateTime(thisValue.Year, thisValue.Month, 1).AddMonths(count);
			return (start, start.AddMonths(1).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousQuarter(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			int startMonth = (Quarter(thisValue) - 1) * 3 + 1;
			DateTime start = new DateTime(thisValue.Year, startMonth, 1).AddMonths(-(3 * count));
			return (start, start.AddMonths(3).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheQuarter(this DateTime thisValue)
		{
			int startMonth = (Quarter(thisValue) - 1) * 3 + 1;
			return new DateTime(thisValue.Year, startMonth, 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisQuarter(this DateTime thisValue)
		{
			DateTime start = FirstOfTheQuarter(thisValue);
			return (start, start.AddMonths(3).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextQuarter(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			int startMonth = (Quarter(thisValue) - 1) * 3 + 1;
			DateTime start = new DateTime(thisValue.Year, startMonth, 1).AddMonths(3 * count);
			return (start, start.AddMonths(3).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousSemiAnnual(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			int startMonth = (SemiAnnual(thisValue) - 1) * 6 + 1;
			DateTime start = new DateTime(thisValue.Year, startMonth, 1).AddMonths(-(6 * count));
			return (start, start.AddMonths(6).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheSemiAnnual(this DateTime thisValue)
		{
			int startMonth = (SemiAnnual(thisValue) - 1) * 6 + 1;
			return new DateTime(thisValue.Year, startMonth, 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisSemiAnnual(this DateTime thisValue)
		{
			DateTime start = FirstOfTheSemiAnnual(thisValue);
			return (start, start.AddMonths(6).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextSemiAnnual(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			int startMonth = (SemiAnnual(thisValue) - 1) * 6 + 1;
			DateTime start = new DateTime(thisValue.Year, startMonth, 1).AddMonths(6 * count);
			return (start, start.AddMonths(6).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousYear(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = new DateTime(thisValue.Year - count, 1, 1);
			return (start, start.AddYears(1).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheYear(this DateTime thisValue)
		{
			return new DateTime(thisValue.Year, 1, 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisYear(this DateTime thisValue)
		{
			DateTime start = FirstOfTheYear(thisValue);
			return (start, start.AddYears(1).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextYear(this DateTime thisValue, ushort count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			DateTime start = new DateTime(thisValue.Year + count, 1, 1);
			return (start, start.AddYears(1).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousCentury(this DateTime thisValue, byte count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			int century = Century(thisValue) - 2;
			DateTime thisStart = new DateTime(century * 100 * count, 1, 1);
			return (thisStart, thisStart.AddYears(100).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheCentury(this DateTime thisValue)
		{
			int century = Century(thisValue) - 1;
			return new DateTime(century * 100, 1, 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisCentury(this DateTime thisValue)
		{
			DateTime thisStart = FirstOfTheCentury(thisValue);
			return (thisStart, thisStart.AddYears(100).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextCentury(this DateTime thisValue, byte count = 1)
		{
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
			int century = Century(thisValue);
			DateTime thisStart = new DateTime(century * 100 * count, 1, 1);
			return (thisStart, thisStart.AddYears(100).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) PreviousMillennium(this DateTime thisValue)
		{
			int millennium = Millennium(thisValue) - 2;
			DateTime thisStart = new DateTime(millennium * 1000, 1, 1);
			return (thisStart, thisStart.AddYears(1000).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime FirstOfTheMillennium(this DateTime thisValue)
		{
			int millennium = Millennium(thisValue) - 1;
			return new DateTime(millennium * 1000, 1, 1);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) ThisMillennium(this DateTime thisValue)
		{
			DateTime thisStart = FirstOfTheMillennium(thisValue);
			return (thisStart, thisStart.AddYears(1000).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static (DateTime Start, DateTime End) NextMillennium(this DateTime thisValue)
		{
			int millennium = Millennium(thisValue);
			DateTime thisStart = new DateTime(millennium * 1000, 1, 1);
			return (thisStart, thisStart.AddYears(1000).AddSeconds(-1));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ClearMilliseconds(this DateTime thisValue)
		{
			return thisValue.AddMilliseconds(-1 * thisValue.Millisecond);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ClearSeconds(this DateTime thisValue)
		{
			return thisValue.AddSeconds(-1 * thisValue.Second);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ClearMinutes(this DateTime thisValue)
		{
			return thisValue.AddMinutes(-1 * thisValue.Minute);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ClearFractions(this DateTime thisValue)
		{
			return thisValue.AddSeconds(-1 * thisValue.Second).AddMilliseconds(-1 * thisValue.Millisecond);
		}

		public static int Weeks(this DateTime thisValue)
		{
			DayOfWeek[] daysOrder = DateTimeFormatInfo.CurrentInfo!.GetDaysOrder();
			(DateTime start, DateTime end) = ThisMonth(thisValue);
			int diffStart = Array.IndexOf(daysOrder, start.DayOfWeek);
			int diffEnd = 6 - Array.IndexOf(daysOrder, end.DayOfWeek);
			int daysInMonth = DateTime.DaysInMonth(thisValue.Year, thisValue.Month);
			return (int)Math.Ceiling((diffStart + daysInMonth + diffEnd) / 7.0d);
		}

		public static DateTime FirstOfWeek(this DateTime thisValue, int week)
		{
			if (week < 1) throw new ArgumentOutOfRangeException(nameof(week));
			week--;
			DateTime first = FirstOfTheMonth(thisValue);
			if (week == 0) return first;

			DayOfWeek[] daysOrder = DateTimeFormatInfo.CurrentInfo!.GetDaysOrder();

			if (week < 5)
			{
				int diffStart = Array.IndexOf(daysOrder, first.DayOfWeek);
				return first.AddDays(week * 7 - diffStart);
			}
	
			int diffEnd = 6 - Array.IndexOf(daysOrder, LastOfTheMonth(thisValue).DayOfWeek);
			DateTime date = first.AddDays(week * 7 - diffEnd - 1);
			if (date.Month != thisValue.Month) throw new ArgumentOutOfRangeException(nameof(week));
			return date;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ThisOrNext(this DateTime thisValue, DayOfWeek dayOfWeek)
		{
			DayOfWeek[] daysOrder = DateTimeFormatInfo.CurrentInfo!.GetDaysOrder();
			int offset = Array.IndexOf(daysOrder, dayOfWeek) - Array.IndexOf(daysOrder, thisValue.DayOfWeek);
			if (offset < 0) offset += 7;
			return thisValue.AddDays(offset);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime Next(this DateTime thisValue, DayOfWeek dayOfWeek)
		{
			DayOfWeek[] daysOrder = DateTimeFormatInfo.CurrentInfo!.GetDaysOrder();
			int offset = Array.IndexOf(daysOrder, dayOfWeek) - Array.IndexOf(daysOrder, thisValue.DayOfWeek);
			if (offset <= 0) offset += 7;
			return thisValue.AddDays(offset);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Elapsed(this DateTime thisValue)
		{
			return (thisValue.Kind == DateTimeKind.Utc
						? DateTime.UtcNow
						: DateTime.Now) -
					thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Remaining(this DateTime thisValue)
		{
			return thisValue -
					(thisValue.Kind == DateTimeKind.Utc
						? DateTime.UtcNow
						: DateTime.Now);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long ElapsedTicks(this DateTime thisValue)
		{
			return (thisValue.Kind == DateTimeKind.Utc
						? DateTime.UtcNow
						: DateTime.Now).Ticks -
					thisValue.Ticks;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long RemainingTicks(this DateTime thisValue)
		{
			return thisValue.Ticks -
					(thisValue.Kind == DateTimeKind.Utc
						? DateTime.UtcNow
						: DateTime.Now).Ticks;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long ToJavaScriptTimestamp(this DateTime thisValue)
		{
			return ((thisValue.Kind == DateTimeKind.Utc
						? thisValue
						: thisValue.ToUniversalTime()) -
					DateTimeHelper.UnixEpoch).TotalLongMilliseconds();
		}

		public static bool TryAdd(this DateTime thisValue, TimeSpan value, out DateTime result)
		{
			try
			{
				result = thisValue.Add(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TrySubtract(this DateTime thisValue, TimeSpan value, out DateTime result)
		{
			try
			{
				result = thisValue.Subtract(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddYears(this DateTime thisValue, int value, out DateTime result)
		{
			try
			{
				result = thisValue.AddYears(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddMonths(this DateTime thisValue, int value, out DateTime result)
		{
			try
			{
				result = thisValue.AddMonths(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddDays(this DateTime thisValue, double value, out DateTime result)
		{
			try
			{
				result = thisValue.AddDays(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddHours(this DateTime thisValue, double value, out DateTime result)
		{
			try
			{
				result = thisValue.AddHours(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddMinutes(this DateTime thisValue, double value, out DateTime result)
		{
			try
			{
				result = thisValue.AddMinutes(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddSeconds(this DateTime thisValue, double value, out DateTime result)
		{
			try
			{
				result = thisValue.AddSeconds(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddMilliseconds(this DateTime thisValue, double value, out DateTime result)
		{
			try
			{
				result = thisValue.AddMilliseconds(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}

		public static bool TryAddTicks(this DateTime thisValue, long value, out DateTime result)
		{
			try
			{
				result = thisValue.AddTicks(value);
				return true;
			}
			catch
			{
				result = thisValue;
				return false;
			}
		}
	}
}
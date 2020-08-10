using System;
using System.Globalization;
using asm.Extensions;
using asm.Patterns.DateTime;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class DateTimeHelper
	{
		public const string DATE_FORMAT = "yyyy-MM-dd";
		public const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm";
		public const string LONG_DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
		public const string TIME_FORMAT = "HH:mm";
		public const string LONG_TIME_FORMAT = "HH:mm:ss";

		private const char DATE_TIME_SPLIT = '.';

		public static DateTime OaEpoch { get; } = new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Utc);
		public static DateTime UnixEpoch { get; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static double Limit(double limit, DateTimeUnit unitToUse)
		{
			double n = limit.NotBelow(0.0D) * -1;

			switch (unitToUse)
			{
				case DateTimeUnit.Year:
				case DateTimeUnit.Month:
					n = n.ToNumber(0);
					break;
			}

			return n;
		}

		public static DateTime ParseDateString(string value) { return ParseDateString(value, false, false); }

		public static DateTime ParseDateString(string value, bool format) { return ParseDateString(value, format, true); }

		public static DateTime ParseDateString(string value, bool format, bool dayFirst)
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

			int timeSep = value.IndexOf(DATE_TIME_SPLIT);
			TimeSpan time = TimeSpan.Zero;
			string date;

			if (timeSep < 0)
			{
				date = value;
			}
			else
			{
				date = value.Left(timeSep);
				string tm = value.Right(value.Length - timeSep);
				if (tm.Length == 6) time = new TimeSpan(int.Parse(tm.Substring(0, 2)), int.Parse(tm.Substring(2, 2)), int.Parse(tm.Substring(4, 2)));
			}

			DateTime result;

			if (format)
			{
				DateTimeFormatInfo dateTimeFormat = CultureInfoHelper.Default.DateTimeFormat;
				if (!date.Contains(dateTimeFormat.DateSeparator)) throw new FormatException($"Invalid date format. Missing '{dateTimeFormat.DateSeparator}'.");

				string[] parts = date.Split(3, StringSplitOptions.RemoveEmptyEntries, dateTimeFormat.DateSeparator);
				if (parts.Length != 3) throw new FormatException("Invalid date string.");
				result = dayFirst
					? new DateTime(int.Parse(parts[2]), int.Parse(parts[1]), int.Parse(parts[0]))
					: new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
			}
			else
			{
				result = dayFirst
					? new DateTime(int.Parse(date.Substring(0, 2)), int.Parse(date.Substring(2, 2)), int.Parse(date.Substring(4, 4)))
					: new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));
			}

			if (time != TimeSpan.Zero) result = result.Add(time);
			return result;
		}

		public static bool TryParseDateString(string value, out DateTime result) { return TryParseDateString(value, false, false, out result); }

		public static bool TryParseDateString(string value, bool format, out DateTime result) { return TryParseDateString(value, format, true, out result); }

		public static bool TryParseDateString(string value, bool format, bool dayFirst, out DateTime result)
		{
			try
			{
				result = ParseDateString(value, format, dayFirst);
				return true;
			}
			catch
			{
				result = default(DateTime);
				return false;
			}
		}

		public static bool TryParseDate(string dateStr, out DateTime date, CultureInfo culture = null)
		{
			if (string.IsNullOrEmpty(dateStr))
			{
				date = DateTime.MinValue;
				return false;
			}

			culture ??= CultureInfoHelper.Default;
			return DateTime.TryParseExact(dateStr, culture.DateTimeFormat.ShortDatePattern, culture, DateTimeStyles.AllowWhiteSpaces, out date)
							|| DateTime.TryParse(dateStr, culture, DateTimeStyles.AllowWhiteSpaces, out date)
							|| DateTime.TryParse(dateStr, out date);
		}

		public static DateTime? TryParseDate(string dateStr)
		{
			if (string.IsNullOrEmpty(dateStr)) return null;
			return TryParseDate(dateStr, out DateTime date) ? date : (DateTime?)null;
		}

		public static string FormatShortDate(string dateStr)
		{
			return TryParseDate(dateStr, out DateTime date) ? FormatShortDate(date) : null;
		}

		[NotNull]
		public static string FormatShortDate(DateTime date, CultureInfo culture = null)
		{
			culture ??= CultureInfoHelper.Default;
			return date.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
		}

		public static string FixedShortDate(string dateStr, CultureInfo culture = null)
		{
			if (culture == null) culture = CultureInfoHelper.Get(null, false);
			else if (culture.UseUserOverride) culture = CultureInfoHelper.Get(culture.Name, false);

			return TryParseDate(dateStr, out DateTime date, culture) ? FixedShortDate(date, culture) : null;
		}

		[NotNull]
		public static string FixedShortDate(DateTime date, CultureInfo culture = null)
		{
			if (culture == null) culture = CultureInfoHelper.Get(null, false);
			else if (culture.UseUserOverride) culture = CultureInfoHelper.Get(culture.Name, false);

			return date.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
		}

		public static bool IsYear(int year) { return year.InRange(DateTime.MinValue.Year, DateTime.MaxValue.Year); }

		public static bool IsMonth(int month) { return month.InRange(1, 12); }

		public static int Year(int year) { return year.Within(DateTime.MinValue.Year, DateTime.MaxValue.Year); }
		
		public static int Month(int month) { return month.Within(1, 12); }

		public static int Day(int day, int year, int month)
		{
			if (!IsYear(year)) throw new ArgumentOutOfRangeException(nameof(year));
			if (!IsMonth(month)) throw new ArgumentOutOfRangeException(nameof(month));
			DateTime date = new DateTime(year, month, 1);
			return day.Within(1, date.DaysOfMonth());
		}

		public static int RandomAge(int minimum, int maximum)
		{
			if (!minimum.InRange(1, byte.MaxValue)) throw new ArgumentOutOfRangeException(nameof(minimum));
			if (!maximum.InRange(1, byte.MaxValue)) throw new ArgumentOutOfRangeException(nameof(maximum));
			if (maximum < minimum) throw new ArgumentOutOfRangeException(nameof(maximum));
			return RNGRandomHelper.Next(minimum, maximum);
		}

		public static DateTime RandomDate(int minimum, int maximum, bool future = false)
		{
			int year = DateTime.Today.Year + RandomAge(minimum, maximum) * (future ? 1 : -1);
			int month = RNGRandomHelper.Next(1, 12);
			int day = RandomDay(year, month);
			return new DateTime(year, month, day, RNGRandomHelper.Next(0, 23), RNGRandomHelper.Next(0, 59), RNGRandomHelper.Next(0, 59));
		}

		public static int RandomDay(int year, int month)
		{
			if (!IsYear(year)) throw new ArgumentOutOfRangeException(nameof(year));
			if (!IsMonth(month)) throw new ArgumentOutOfRangeException(nameof(month));
			DateTime date = new DateTime(year, month, 1);
			return RNGRandomHelper.Next(1, date.DaysOfMonth());
		}

		public static DateTimeRange GetRange(DateTime? fromDate, DateTime? toDate)
		{
			bool hasStart = fromDate.HasValue && fromDate > DateTime.MinValue && fromDate < DateTime.MaxValue;
			bool hasEnd = toDate.HasValue && toDate > DateTime.MinValue && toDate < DateTime.MaxValue;
			DateTime end = toDate ?? DateTime.Today;
			if (end == DateTime.MinValue || end == DateTime.MaxValue) end = DateTime.Today;

			DateTime start = fromDate ?? end;
			if (!hasStart && !hasEnd || start == DateTime.MinValue || start == DateTime.MaxValue) start = (hasEnd ? end : DateTime.Today).PreviousMonth().Start;
			return new DateTimeRange(start, end);
		}
	}
}
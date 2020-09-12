using System;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Patterns.DateTime;

namespace asm.Helpers
{
	public static class TimeSpanHelper
	{
		private enum TimeSpanSign
		{
			User = 0,
			InternalPositive = 1,
			InternalNegative = 2
		}
		
		public const int MINIMUM = 0;
		public const int MAXIMUM = int.MaxValue;
		public const int INFINITE = Timeout.Infinite;

		public const int MINIMUM_SCHEDULE = 50;
		public const int FAST_SCHEDULE = 250;
		public const int SCHEDULE = 500;

		public const int SECOND = 1000;
		public const int MINUTE = SECOND * 60;
		public const int HOUR = MINUTE * 60;
		public const int DAY = HOUR * 24;
		public const int WEEK = DAY * 7;

		public static readonly TimeSpan Minimum = TimeSpan.Zero;
		public static readonly TimeSpan Maximum = TimeSpan.FromMilliseconds(MAXIMUM);
		public static readonly TimeSpan Infinite = Timeout.InfiniteTimeSpan;

		public static readonly TimeSpan MinimumSchedule = TimeSpan.FromMilliseconds(MINIMUM_SCHEDULE);
		public static readonly TimeSpan FastSchedule = TimeSpan.FromMilliseconds(FAST_SCHEDULE);
		public static readonly TimeSpan Schedule = TimeSpan.FromMilliseconds(SCHEDULE);

		public static readonly TimeSpan Second = TimeSpan.FromSeconds(1);
		public static readonly TimeSpan Minute = TimeSpan.FromMinutes(1);
		public static readonly TimeSpan Hour = TimeSpan.FromHours(1);
		public static readonly TimeSpan Day = TimeSpan.FromDays(1);
		public static readonly TimeSpan Week = TimeSpan.FromDays(7);
		public static readonly TimeSpan Month = TimeSpan.FromDays(30);
		public static readonly TimeSpan Year = TimeSpan.FromDays(365);

		private const int NEGATIVE_ZERO_VALUE = int.MinValue;

		private const char TIME_SEPARATOR = ':';
		private const char DAY_SEPARATOR = '.';

		public static int From(int value, int minimum = INFINITE) { return value.NotBelow(minimum.NotBelow(INFINITE)); }

		public static long From(long value, long minimum = INFINITE) { return value.NotBelow(minimum.NotBelow(INFINITE)); }

		public static long From(TimeSpan value, long minimum = INFINITE)
		{
			return value.IsValid(true, true)
						? value.TotalLongMilliseconds()
						: minimum.NotBelow(INFINITE);
		}

		public static int FromNullable(int? value, int minimum = INFINITE)
		{
			return value.HasValue
						? From(value.Value, minimum)
						: minimum.NotBelow(INFINITE);
		}

		public static long FromNullable(long? value, long minimum = INFINITE)
		{
			return value.HasValue
						? From(value.Value, minimum)
						: minimum.NotBelow(INFINITE);
		}

		public static long FromNullable(TimeSpan? value, long minimum = INFINITE)
		{
			return value.HasValue
						? From(value.Value, minimum)
						: minimum.NotBelow(INFINITE);
		}

		public static TimeSpan NeutralUserOperationValue { get; } = TimeSpan.Zero;

		public static TimeSpan MinUserOperationValue { get; } =
			ClearTimeSpanSign(new TimeSpan(Convert.ToInt32(Math.Truncate(Convert.ToDouble(int.MinValue) / 60 / 60 / 24)), 0, 0, 0) + Second);

		public static TimeSpan MaxUserOperationValue { get; } =
			ClearTimeSpanSign(new TimeSpan(Convert.ToInt32(Math.Truncate(Convert.ToDouble(int.MaxValue) / 60 / 60 / 24)), 0, 0, 0) - Second);

		public static int NegativeZeroValue => NEGATIVE_ZERO_VALUE;

		public static char TimeSeparator => TIME_SEPARATOR;

		public static char DaySeparator => DAY_SEPARATOR;

		public static TimeSpan Parse(string str)
		{
			TimeSpan ts;

			try
			{
				ts = TimeSpan.Parse(str);
			}
			catch (OverflowException)
			{
				int days = 0;
				int indexDay = str.IndexOf(DaySeparator);
				if (indexDay >= 0)
				{
					string daysStr = str.Substring(0, indexDay);
					str = str.Remove(0, indexDay + 1);
					try
					{
						days = int.Parse(daysStr);
					}
					catch
					{
						return new TimeSpan(0);
					}
				}

				int hours;
				int index = str.IndexOf(TimeSeparator);
				string hoursStr = str.Substring(0, index);
				str = str.Remove(0, index);
				str = str.Insert(0, "00");
				try
				{
					hours = int.Parse(hoursStr);
				}
				catch
				{
					return new TimeSpan(0);
				}

				try
				{
					ts = TimeSpan.Parse(str);
				}
				catch
				{
					return new TimeSpan(0);
				}

				ts = new TimeSpan(days, hours, ts.Minutes, ts.Seconds);
			}
			catch
			{
				ts = new TimeSpan(0, 0, 0);
			}

			return ts;
		}

		public static TimeSpan ChangePart(TimeSpan timeSpan, TimeUnit format, TimeUnit part, int partValue)
		{
			bool isNegativeZero = false;
			bool isHighestPart = !ContainsNextPart(format, part);
			bool isUserOperationValue = GetTimeSpanSign(timeSpan) == TimeSpanSign.User;
			TimeSpan previousUserTimeSpan = isUserOperationValue
												? timeSpan
												: ToUserOperationValue(timeSpan, out isNegativeZero);
			bool setNegativeZero;
			bool isPartValueNegative;

			if (partValue == NEGATIVE_ZERO_VALUE)
			{
				setNegativeZero = true;
				isPartValueNegative = true;
				partValue = 0;
			}
			else
			{
				setNegativeZero = isNegativeZero;
				isPartValueNegative = partValue < 0;
				if (isPartValueNegative) partValue = -partValue;
			}

			bool isPreviousUserTimeSpanNegative = previousUserTimeSpan.Ticks < 0 || isNegativeZero;
			if (isPreviousUserTimeSpanNegative) previousUserTimeSpan = previousUserTimeSpan.Negate();

			TimeSpan previousUserTimeSpanPart = GetTimeSpanPart(previousUserTimeSpan, format, part);
			TimeSpan newUserTimeSpanPart = GetTimeSpanPart(part, partValue);
			TimeSpan newUserTimeSpan = previousUserTimeSpan - previousUserTimeSpanPart + newUserTimeSpanPart;

			if (isHighestPart)
			{
				if (isPartValueNegative) newUserTimeSpan = newUserTimeSpan.Negate();
			}
			else
			{
				if (isPreviousUserTimeSpanNegative) newUserTimeSpan = newUserTimeSpan.Negate();
			}

			return isUserOperationValue
						? newUserTimeSpan
						: ToInternalOperationValue(newUserTimeSpan, setNegativeZero);
		}

		public static TimeSpan GetTimeSpanPart(TimeSpan timeSpan, TimeUnit format, TimeUnit part) { return GetTimeSpanPart(part, GetPart(timeSpan, format, part)); }

		public static TimeSpan GetTimeSpanPart(TimeUnit part, int partValue)
		{
			switch (part)
			{
				case TimeUnit.Second:
					return new TimeSpan(0, 0, 0, partValue);
				case TimeUnit.Minute:
					return new TimeSpan(0, 0, partValue, 0);
				case TimeUnit.Hour:
					return new TimeSpan(0, partValue, 0, 0);
				case TimeUnit.Day:
					return new TimeSpan(partValue, 0, 0, 0);
				default:
					throw new ArgumentOutOfRangeException(nameof(part));
			}
		}

		public static int GetPart(TimeSpan timeSpan, TimeUnit format, TimeUnit part)
		{
			return ContainsNextPart(format, part)
						? GetStandardPart(timeSpan, part)
						: GetTotalPart(timeSpan, part);
		}

		public static int GetStandardPart(TimeSpan timeSpan, TimeUnit part)
		{
			switch (part)
			{
				case TimeUnit.Second:
					return timeSpan.Seconds;
				case TimeUnit.Minute:
					return timeSpan.Minutes;
				case TimeUnit.Hour:
					return timeSpan.Hours;
				case TimeUnit.Day:
					return timeSpan.Days;
				default:
					throw new ArgumentOutOfRangeException(nameof(part));
			}
		}

		public static int GetTotalPart(TimeSpan timeSpan, TimeUnit part)
		{
			switch (part)
			{
				case TimeUnit.Second:
					return Convert.ToInt32(Math.Truncate(timeSpan.TotalSeconds));
				case TimeUnit.Minute:
					return Convert.ToInt32(Math.Truncate(timeSpan.TotalMinutes));
				case TimeUnit.Hour:
					return Convert.ToInt32(Math.Truncate(timeSpan.TotalHours));
				case TimeUnit.Day:
					return Convert.ToInt32(Math.Truncate(timeSpan.TotalDays));
				default:
					throw new ArgumentOutOfRangeException(nameof(part));
			}
		}

		[NotNull]
		public static string GetMask(TimeUnit format, TimeUnit part)
		{
			switch (part)
			{
				case TimeUnit.Second:
					return format.HasFlag(TimeUnit.Minute)
								? "ss"
								: "%s";
				case TimeUnit.Minute:
					return format.HasFlag(TimeUnit.Hour)
								? "mm"
								: "%m";
				case TimeUnit.Hour:
					return format.HasFlag(TimeUnit.Day)
								? "HH"
								: "%H";
				case TimeUnit.Day:
					return "%d";
				default:
					throw new ArgumentOutOfRangeException(nameof(part));
			}
		}

		public static bool ContainsNextPart(TimeUnit format, TimeUnit part)
		{
			switch (part)
			{
				case TimeUnit.Second:
					return format.HasFlag(TimeUnit.Minute);
				case TimeUnit.Minute:
					return format.HasFlag(TimeUnit.Hour);
				case TimeUnit.Hour:
					return format.HasFlag(TimeUnit.Day);
				case TimeUnit.Day:
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(part));
			}
		}

		public static int GetNeutralPartValue(TimeUnit format, TimeUnit part)
		{
			TimeSpan neutralUserOperationValue = NeutralUserOperationValue;
			return ContainsNextPart(format, part)
						? GetStandardPart(neutralUserOperationValue, part)
						: GetTotalPart(neutralUserOperationValue, part);
		}

		public static int GetMinPartValue(TimeUnit format, TimeUnit part, bool allowNegativeValues)
		{
			return ContainsNextPart(format, part)
						? GetNeutralPartValue(format, part)
						: GetTotalPart(GetMinUserOperationValue(allowNegativeValues), part);
		}

		public static int GetMaxPartValue(TimeUnit format, TimeUnit part)
		{
			if (!ContainsNextPart(format, part)) return GetTotalPart(MaxUserOperationValue, part);
			TimeSpan maxADayTimeSpan = Day - GetTimeSpanPart(TimeUnit.Second, 1);
			return GetStandardPart(maxADayTimeSpan, part);
		}

		public static int GetMinNumDigits(TimeUnit format, TimeUnit part, bool allowNegativeValues)
		{
			return ContainsNextPart(format, part)
						? GetMaxNumDigits(format, part, allowNegativeValues)
						: 1;
		}

		public static int GetMaxNumDigits(TimeUnit format, TimeUnit part, bool allowNegativeValues)
		{
			int minPartValueDigits = Math.Abs(GetMinPartValue(format, part, allowNegativeValues)).ToString("0").Length;
			int maxPartValueDigits = Math.Abs(GetMaxPartValue(format, part)).ToString("0").Length;
			return Math.Max(minPartValueDigits, maxPartValueDigits);
		}

		public static void AssertInUserOperationRange(TimeSpan timeSpan, bool allowNegativeValues)
		{
			TimeSpan minUserOperationValue = GetMinUserOperationValue(allowNegativeValues);
			if (timeSpan < minUserOperationValue || timeSpan > MaxUserOperationValue)
			{
				throw new
					NotSupportedException($"The value {timeSpan} is outside the valid range. The user operation value must be between {minUserOperationValue} and {MaxUserOperationValue} inclusive.");
			}
		}

		public static void AssertInInternalOperationRange(TimeSpan timeSpan)
		{
			TimeSpan minInternalOperationValue = new TimeSpan(Math.Min(ToInternalOperationValue(NeutralUserOperationValue, true).Ticks,
																		ToInternalOperationValue(NeutralUserOperationValue).Ticks));
			TimeSpan maxInternalOperationValue = new TimeSpan(Math.Max(ToInternalOperationValue(MinUserOperationValue).Ticks,
																		ToInternalOperationValue(MaxUserOperationValue).Ticks));
			if (timeSpan < minInternalOperationValue || timeSpan > maxInternalOperationValue)
			{
				throw new
					NotSupportedException($"The value {timeSpan} is outside the valid range. Internal working value must be between {minInternalOperationValue} and {maxInternalOperationValue} inclusive.");
			}
		}

		public static TimeSpan ToUserOperationValue(TimeSpan value, out bool isNegativeZero)
		{
			TimeSpanSign sign = GetTimeSpanSign(value);

			switch (sign)
			{
				case TimeSpanSign.User:
					isNegativeZero = false;
					return value;
				case TimeSpanSign.InternalPositive:
					TimeSpan userPositiveOperationValue = ClearTimeSpanSign(value);
					isNegativeZero = false;
					return userPositiveOperationValue;
				case TimeSpanSign.InternalNegative:
					TimeSpan userNegativeOperationValue = ClearTimeSpanSign(value).Negate();
					isNegativeZero = userNegativeOperationValue == NeutralUserOperationValue;
					return userNegativeOperationValue;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static TimeSpan ToInternalOperationValue(TimeSpan value, bool setNegativeZero = false)
		{
			TimeSpanSign sign = GetTimeSpanSign(value);
			switch (sign)
			{
				case TimeSpanSign.User:
					TimeSpanSign internalSign = GetInternalTimeSpanSign(value, setNegativeZero);
					TimeSpan positiveValue = value;
					if (value.Ticks < 0) positiveValue = positiveValue.Negate();
					return SetInternalTimeSpanSign(positiveValue, internalSign);
				case TimeSpanSign.InternalPositive:
				case TimeSpanSign.InternalNegative:
					return value;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static TimeSpan GetMinUserOperationValue(bool allowNegativeValues)
		{
			return allowNegativeValues
						? MinUserOperationValue
						: NeutralUserOperationValue;
		}

		[NotNull]
		public static string TimeSpanToString(TimeSpan value, TimeUnit allowedInput)
		{
			TimeSpan userOperationValue = ToUserOperationValue(value, out bool isNegativeZero);

			string displayText = string.Empty;
			if (allowedInput.HasFlag(TimeUnit.Day)) displayText = GetHigherPartToString(userOperationValue.TotalDays);

			if (allowedInput.HasFlag(TimeUnit.Hour))
			{
				if (allowedInput.HasFlag(TimeUnit.Day)) displayText += $"{DaySeparator}{GetNoHigherPartToString(userOperationValue.Hours)}";
				else displayText += GetHigherPartToString(userOperationValue.TotalHours);
			}

			if (allowedInput.HasFlag(TimeUnit.Minute))
			{
				if (allowedInput.HasFlag(TimeUnit.Hour)) displayText += $"{TimeSeparator}{GetNoHigherPartToString(userOperationValue.Minutes)}";
				else displayText += GetHigherPartToString(userOperationValue.TotalMinutes);
			}

			if (allowedInput.HasFlag(TimeUnit.Second))
			{
				if (allowedInput.HasFlag(TimeUnit.Minute)) displayText += $"{TimeSeparator}{GetNoHigherPartToString(userOperationValue.Seconds)}";
				else displayText += GetHigherPartToString(userOperationValue.TotalSeconds);
			}

			if (isNegativeZero) displayText = $"-{displayText}";

			return displayText;
		}

		[NotNull]
		private static string GetHigherPartToString(double part)
		{
			int value = Convert.ToInt32(Math.Truncate(part));
			return $"{(value == 0 && part < 0 ? "-" : string.Empty)}{value:0}";
		}

		[NotNull]
		private static string GetNoHigherPartToString(int part) { return Math.Abs(part).ToString("00"); }

		private static TimeSpan ClearTimeSpanSign(TimeSpan timeSpan) { return new TimeSpan(timeSpan.Ticks - TimeSpan.TicksPerMillisecond * timeSpan.Milliseconds); }

		private static TimeSpanSign GetTimeSpanSign(TimeSpan value)
		{
			int milliseconds = value.Milliseconds;
			TimeSpanSign sign = (TimeSpanSign)milliseconds;
			return sign;
		}

		private static TimeSpanSign GetInternalTimeSpanSign(TimeSpan userValue, bool setNegativeZero)
		{
			if (userValue.Ticks == 0 && setNegativeZero || userValue.Ticks < 0) return TimeSpanSign.InternalNegative;
			return TimeSpanSign.InternalPositive;
		}

		private static TimeSpan SetInternalTimeSpanSign(TimeSpan positiveValue, TimeSpanSign internalSign)
		{
			return new TimeSpan(Convert.ToInt32(Math.Truncate(positiveValue.TotalDays)), positiveValue.Hours, positiveValue.Minutes, positiveValue.Seconds,
								(int)internalSign);
		}
	}
}
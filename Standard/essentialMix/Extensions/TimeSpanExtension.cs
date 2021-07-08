using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.DateTime;

namespace essentialMix.Extensions
{
	public static class TimeSpanExtension
	{
		private const string DOT_SEP = @"\.";
		private const string COL_SEP = @"\:";
		private const string HOURS_MINUTES = @"hh\:mm";
		private const string DAYS_HOURS_MINUTES = @"d\.hh\:mm";
		private const string HOURS_MINUTES_SECONDS = @"hh\:mm\:ss";
		private const string DAYS_HOURS_MINUTES_SECONDS = @"d\.hh\:mm\:ss";
		private const string SHORT_STRING = @"d\.hh\:mm\:ss\.fff";
		private const string MEDIUM_STRING = @"d\.hh\:mm\:ss\.fffff";
		private const string LONG_STRING = @"d\.hh\:mm\:ss\.fffffff";

		private static readonly Lazy<TimeUnit[]> __timeUnitValues = new Lazy<TimeUnit[]>(EnumHelper<TimeUnit>.GetValues, LazyThreadSafetyMode.PublicationOnly);

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsValid(this TimeSpan thisValue, bool zeroIsValid = false, bool infiniteIsValid = false)
		{
			if (!zeroIsValid && thisValue == TimeSpan.Zero) return false;
			if (!infiniteIsValid && thisValue.TotalMilliseconds.Equals(TimeSpanHelper.INFINITE)) return false;
			return thisValue != TimeSpan.MinValue && thisValue != TimeSpan.MaxValue;
		}

		public static double GetValue(this TimeSpan thisValue, TimeUnit unit)
		{
			unit = unit.HighestFlag();

			switch (unit)
			{
				case TimeUnit.Millisecond:
					return thisValue.Milliseconds;
				case TimeUnit.Second:
					return thisValue.Seconds * 1000;
				case TimeUnit.Minute:
					return thisValue.Minutes * 60 * 1000;
				case TimeUnit.Hour:
					return thisValue.Hours * 60 * 60 * 1000;
				case TimeUnit.Day:
					return thisValue.Days * 24 * 60 * 60 * 1000;
				default:
					return 0;
			}
		}

		public static double GetValueAt(this TimeSpan thisValue, TimeUnit unit)
		{
			unit = unit.HighestFlag();
			int max = __timeUnitValues.Value.IndexOf(unit);
			if (max < 0) throw new ArgumentOutOfRangeException(nameof(unit));

			double value = 0;

			for (int i = 0; i <= max; i++)
				value += GetValue(thisValue, __timeUnitValues.Value[i]);

			return value;
		}

		public static double GetValueBelow(this TimeSpan thisValue, TimeUnit unit)
		{
			int max = __timeUnitValues.Value.IndexOf(unit);
			if (max < 0) throw new ArgumentOutOfRangeException(nameof(unit));

			double value = 0;

			for (int i = 0; i < max; i++)
				value += GetValue(thisValue, __timeUnitValues.Value[i]);

			return value;
		}

		public static bool InRange(this TimeSpan thisValue, double minimum, double maximum, TimeUnit unit) { return GetValueAt(thisValue, unit).InRange(minimum, maximum); }

		public static bool InRangeEx(this TimeSpan thisValue, double minimum, double maximum, TimeUnit unit) { return GetValueAt(thisValue, unit).InRangeEx(minimum, maximum); }

		public static bool InRangeLx(this TimeSpan thisValue, double minimum, double maximum, TimeUnit unit) { return GetValueAt(thisValue, unit).InRangeLx(minimum, maximum); }

		public static bool InRangeRx(this TimeSpan thisValue, double minimum, double maximum, TimeUnit unit) { return GetValueAt(thisValue, unit).InRangeRx(minimum, maximum); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRange(this TimeSpan thisValue, TimeSpan minimum, TimeSpan maximum)
		{
			return thisValue.TotalMilliseconds.InRange(minimum.TotalMilliseconds, maximum.TotalMilliseconds);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeEx(this TimeSpan thisValue, TimeSpan minimum, TimeSpan maximum)
		{
			return thisValue.TotalMilliseconds.InRangeEx(minimum.TotalMilliseconds, maximum.TotalMilliseconds);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeLx(this TimeSpan thisValue, TimeSpan minimum, TimeSpan maximum)
		{
			return thisValue.TotalMilliseconds.InRangeLx(minimum.TotalMilliseconds, maximum.TotalMilliseconds);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeRx(this TimeSpan thisValue, TimeSpan minimum, TimeSpan maximum)
		{
			return thisValue.TotalMilliseconds.InRangeRx(minimum.TotalMilliseconds, maximum.TotalMilliseconds);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Within(this TimeSpan thisValue, TimeSpan? minimum = null, TimeSpan? maximum = null)
		{
			if (minimum.HasValue && maximum.HasValue)
			{
				if (minimum.Value > maximum.Value) throw new InvalidOperationException($"{nameof(minimum)} cannot be greater than {nameof(maximum)}.");
				if (thisValue < minimum.Value) return minimum.Value;
				return thisValue > maximum.Value ? maximum.Value : thisValue;
			}

			if (minimum.HasValue && thisValue < minimum.Value) return minimum.Value;
			if (maximum.HasValue && thisValue > maximum.Value) return maximum.Value;
			return thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan NotBelow(this TimeSpan thisValue, TimeSpan minimum)
		{
			return thisValue < minimum
						? minimum
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan NotAbove(this TimeSpan thisValue, TimeSpan maximum)
		{
			return thisValue > maximum
						? maximum
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Minimum(this TimeSpan thisValue, TimeSpan value)
		{
			return thisValue < value
						? value
						: thisValue;
		}

		public static TimeSpan Minimum(this TimeSpan thisValue, [NotNull] params TimeSpan[] minimum)
		{
			if (minimum.Length == 0) return thisValue;

			TimeSpan value = thisValue;

			foreach (TimeSpan m in minimum)
			{
				if (value < m) continue;
				value = m;
			}

			return value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Maximum(this TimeSpan thisValue, TimeSpan value)
		{
			return thisValue > value
						? value
						: thisValue;
		}

		public static TimeSpan Maximum(this TimeSpan thisValue, [NotNull] params TimeSpan[] maximum)
		{
			if (maximum.IsNullOrEmpty()) return thisValue;

			TimeSpan value = thisValue;

			foreach (TimeSpan m in maximum)
			{
				if (value.CompareTo(m) >= 0) continue;
				value = m;
			}

			return value;
		}

		public static TimeSpan MinimumNotBelow(this TimeSpan thisValue, TimeSpan bound, [NotNull] params TimeSpan[] minimum)
		{
			TimeSpan value = NotBelow(thisValue, bound);
			if (value.CompareTo(bound) == 0 || minimum.IsNullOrEmpty()) return value;

			foreach (TimeSpan m in minimum)
			{
				if (value.CompareTo(m) <= 0) continue;
				value = m;
				if (value.CompareTo(bound) == 0) break;
			}

			return value;
		}

		public static TimeSpan MaximumNotAbove(this TimeSpan thisValue, TimeSpan bound, [NotNull] params TimeSpan[] maximum)
		{
			TimeSpan value = NotAbove(thisValue, bound);
			if (value.CompareTo(bound) == 0 || maximum.IsNullOrEmpty()) return value;

			foreach (TimeSpan m in maximum)
			{
				if (value.CompareTo(m) >= 0) continue;
				value = m;
				if (value.CompareTo(bound) == 0) break;
			}

			return value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short TotalShortMilliseconds(this TimeSpan thisValue) { return (short)thisValue.TotalMilliseconds.Within(short.MinValue, short.MaxValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ushort TotalUShortMilliseconds(this TimeSpan thisValue) { return (ushort)thisValue.TotalMilliseconds.Within(ushort.MinValue, ushort.MaxValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int TotalIntMilliseconds(this TimeSpan thisValue) { return (int)thisValue.TotalMilliseconds.Within(int.MinValue, int.MaxValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static uint TotalUIntMilliseconds(this TimeSpan thisValue) { return (uint)thisValue.TotalMilliseconds.Within(uint.MinValue, uint.MaxValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long TotalLongMilliseconds(this TimeSpan thisValue) { return (long)thisValue.TotalMilliseconds.Within(long.MinValue, long.MaxValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ulong TotalULongMilliseconds(this TimeSpan thisValue) { return (ulong)thisValue.TotalMilliseconds.Within(ulong.MinValue, ulong.MaxValue); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string HoursMinutes(this TimeSpan thisValue) { return thisValue.ToString(HOURS_MINUTES); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string DaysHoursMinutes(this TimeSpan thisValue) { return thisValue.ToString(DAYS_HOURS_MINUTES); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string HoursMinutesSeconds(this TimeSpan thisValue) { return thisValue.ToString(HOURS_MINUTES_SECONDS); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string DaysHoursMinutesSeconds(this TimeSpan thisValue) { return thisValue.ToString(DAYS_HOURS_MINUTES_SECONDS); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string HoursMinutesSecondsMilliseconds(this TimeSpan thisValue, byte millisecondsLength = 3, bool millisecondsAreOptional = false)
		{
			millisecondsLength = millisecondsLength.Within((byte)0, (byte)7);
			return millisecondsLength == 0 
				? thisValue.ToString(HOURS_MINUTES_SECONDS)
				: thisValue.ToString(HOURS_MINUTES_SECONDS + DOT_SEP + new string(millisecondsAreOptional ? 'F' : 'f', millisecondsLength));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string DaysHoursMinutesSecondsMilliseconds(this TimeSpan thisValue, byte millisecondsLength = 3, bool millisecondsAreOptional = false)
		{
			millisecondsLength = millisecondsLength.Within((byte)0, (byte)7);
			return millisecondsLength == 0 
				? thisValue.ToString(DAYS_HOURS_MINUTES_SECONDS)
				: thisValue.ToString(DAYS_HOURS_MINUTES_SECONDS + DOT_SEP + new string(millisecondsAreOptional ? 'F' : 'f', millisecondsLength));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToShortString(this TimeSpan thisValue) { return thisValue.ToString(SHORT_STRING); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToMediumString(this TimeSpan thisValue) { return thisValue.ToString(MEDIUM_STRING); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToLongString(this TimeSpan thisValue) { return thisValue.ToString(LONG_STRING); }

		[NotNull]
		public static string ToString(this TimeSpan thisValue, TimeUnit input, byte millisecondsLength = 3, bool millisecondsAreOptional = false)
		{
			StringBuilder sb = new StringBuilder(16);
			millisecondsLength = millisecondsLength.Within((byte)0, (byte)7);

			switch (input)
			{
				case TimeUnit.None:
					break;
				case TimeUnit.Millisecond:
					if (millisecondsLength > 0)
					{
						char f = millisecondsAreOptional ? 'F' : 'f';
						string fs = new string(f, millisecondsLength);
						sb.Append(fs);
					}
					break;
				case TimeUnit.Second:
					sb.Append("ss");
					break;
				case TimeUnit.Minute:
					sb.Append("mm");
					break;
				case TimeUnit.Hour:
					sb.Append("hh");
					break;
				case TimeUnit.Day:
					sb.Append("dd");
					break;
				case TimeUnit.Hour | TimeUnit.Minute:
					sb.Append(HOURS_MINUTES);
					break;
				case TimeUnit.Day | TimeUnit.Hour | TimeUnit.Minute:
					sb.Append(DAYS_HOURS_MINUTES);
					break;
				case TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second:
					sb.Append(HOURS_MINUTES_SECONDS);
					break;
				case TimeUnit.Day | TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second:
					sb.Append(DAYS_HOURS_MINUTES_SECONDS);
					break;
				case TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second | TimeUnit.Millisecond:
					sb.Append(HOURS_MINUTES_SECONDS);

					if (millisecondsLength > 0)
					{
						char f = millisecondsAreOptional ? 'F' : 'f';
						string fs = new string(f, millisecondsLength);
						sb.Append(DOT_SEP);
						sb.Append(fs);
					}
					break;
				case TimeUnit.Day | TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second | TimeUnit.Millisecond:
					sb.Append(DAYS_HOURS_MINUTES_SECONDS);

					if (millisecondsLength > 0)
					{
						char f = millisecondsAreOptional ? 'F' : 'f';
						string fs = new string(f, millisecondsLength);
						sb.Append(DOT_SEP);
						sb.Append(fs);
					}
					break;
				default:
					string separator = COL_SEP;

					if (input.FastHasFlag(TimeUnit.Day))
					{
						sb.Append('d');
						separator = DOT_SEP;
					}

					if (input.FastHasFlag(TimeUnit.Hour))
					{
						if (sb.Length > 0)
						{
							sb.Append(separator);
							separator = COL_SEP;
						}

						sb.Append("hh");
					}

					if (input.FastHasFlag(TimeUnit.Minute))
					{
						if (sb.Length > 0)
						{
							sb.Append(separator);
							separator = COL_SEP;
						}

						sb.Append("mm");
					}

					if (input.FastHasFlag(TimeUnit.Second))
					{
						if (sb.Length > 0) sb.Append(separator);
						sb.Append("ss");
					}

					if (input.FastHasFlag(TimeUnit.Millisecond))
					{
						if (millisecondsLength > 0)
						{
							char f = millisecondsAreOptional ? 'F' : 'f';
							string fs = new string(f, millisecondsLength);
							if (sb.Length > 0) sb.Append(DOT_SEP);
							sb.Append(fs);
						}
					}
					break;
			}

			if (sb.Length == 1) sb.Insert(0, '%');
			return thisValue.ToString(sb.ToString());
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ToDateTime(this TimeSpan thisValue) { return DateTime.MinValue.Add(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int ToInt(this TimeSpan thisValue) { return ToDateTime(thisValue).ToInt(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long ToLong(this TimeSpan thisValue) { return ToDateTime(thisValue).ToLong(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double ToDouble(this TimeSpan thisValue) { return ToDateTime(thisValue).ToDouble(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Years(this TimeSpan thisValue) { return ToDateTime(thisValue).Years(DateTime.MinValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Months(this TimeSpan thisValue) { return ToDateTime(thisValue).Months(DateTime.MinValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Days(this TimeSpan thisValue) { return ToDateTime(thisValue).Days(DateTime.MinValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Hours(this TimeSpan thisValue) { return ToDateTime(thisValue).Hours(DateTime.MinValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Minutes(this TimeSpan thisValue) { return ToDateTime(thisValue).Minutes(DateTime.MinValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Seconds(this TimeSpan thisValue) { return ToDateTime(thisValue).Seconds(DateTime.MinValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Milliseconds(this TimeSpan thisValue) { return ToDateTime(thisValue).Milliseconds(DateTime.MinValue); }
	}
}
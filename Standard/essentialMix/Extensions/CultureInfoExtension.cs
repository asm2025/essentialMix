using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class CultureInfoExtension
	{ 
		private static readonly ConcurrentDictionary<int, RegionInfo> __regionCache = new ConcurrentDictionary<int, RegionInfo>();

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ListSeparator([NotNull] this CultureInfo thisValue) { return thisValue.TextInfo.ListSeparator; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsRightToLeft([NotNull] this CultureInfo thisValue) { return thisValue.TextInfo.IsRightToLeft; }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static CultureInfo Neutral([NotNull] this CultureInfo thisValue)
		{
			return thisValue.IsNeutralCulture
						? thisValue
						: thisValue.Parent;
		}

		public static DateTime MinDateTime([NotNull] this CultureInfo thisValue) { return thisValue.Calendar.MinSupportedDateTime; }
		
		public static DateTime MaxDateTime([NotNull] this CultureInfo thisValue) { return thisValue.Calendar.MaxSupportedDateTime; }

		public static RegionInfo Region([NotNull] this CultureInfo thisValue)
		{
			return __regionCache.GetOrAdd(thisValue.LCID, lcid => new RegionInfo(lcid));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DayOfWeek[] GetDaysOrder([NotNull] this CultureInfo thisValue)
		{
			return thisValue.DateTimeFormat.GetDaysOrder();
		}
	}
}
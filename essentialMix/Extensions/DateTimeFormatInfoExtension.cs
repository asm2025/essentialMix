using System;
using System.Collections.Concurrent;
using System.Globalization;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class DateTimeFormatInfoExtension
{ 
	private	static readonly ConcurrentDictionary<DayOfWeek, DayOfWeek[]> __dayOfWeekMap = new ConcurrentDictionary<DayOfWeek, DayOfWeek[]>();

	[NotNull]
	public static DayOfWeek[] GetDaysOrder([NotNull] this DateTimeFormatInfo thisValue)
	{
		return __dayOfWeekMap.GetOrAdd(thisValue.FirstDayOfWeek, dayOfWeek =>
		{
			DayOfWeek[] days = new DayOfWeek[7];

			for (int i = 0; i < days.Length; i++)
			{
				days[i] = dayOfWeek;
				dayOfWeek = (DayOfWeek)(((int)dayOfWeek + 1) % 7);
			}

			return days;
		});
	}
}
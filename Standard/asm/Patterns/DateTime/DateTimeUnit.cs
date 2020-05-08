using System;

namespace asm.Patterns.DateTime
{
	[Flags]
	public enum DateTimeUnit
	{
		None = TimeUnit.None,
		Millisecond = TimeUnit.Millisecond,
		Second = TimeUnit.Second,
		Minute = TimeUnit.Minute,
		Hour = TimeUnit.Hour,
		Day = DateUnit.Day,
		Month = DateUnit.Month,
		Year = DateUnit.Year
	}
}
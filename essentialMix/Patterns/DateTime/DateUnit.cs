using System;

namespace essentialMix.Patterns.DateTime;

[Flags]
public enum DateUnit
{
	None = TimeUnit.None,
	Day = TimeUnit.Day,
	Month = Day << 1,
	Year = Day << 2
}
using System;

namespace asm.Patterns.DateTime
{
	[Flags]
	public enum TimeUnit
	{
		None = 0,
		Millisecond = 1,
		Second = 1 << 1,
		Minute = 1 << 2,
		Hour = 1 << 3,
		Day = 1 << 4
	}
}
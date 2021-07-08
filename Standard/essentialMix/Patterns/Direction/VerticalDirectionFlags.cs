using System;
using essentialMix.Extensions;

namespace essentialMix.Patterns.Direction
{
	[Flags]
	public enum VerticalDirectionFlags
	{
		Default = 0,
		Up = 1,
		Down = 1 << 1
	}

	public static class VerticalDirectionFlagsExtension
	{
		public static (bool Up, bool Down) GetDirections(this VerticalDirectionFlags direction)
		{
			if (direction == VerticalDirectionFlags.Default) direction = VerticalDirectionFlags.Up | VerticalDirectionFlags.Down;
			return (direction.FastHasFlag(VerticalDirectionFlags.Up), direction.FastHasFlag(VerticalDirectionFlags.Down));
		}
	}
}
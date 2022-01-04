using System;
using essentialMix.Extensions;

namespace essentialMix.Patterns.Direction;

[Flags]
public enum HorizontalDirectionFlags
{
	Default = 0,
	Left = 1,
	Right = 1 << 1
}

public static class HorizontalDirectionFlagsExtension
{
	public static (bool Left, bool Right) GetDirections(this HorizontalDirectionFlags direction)
	{
		if (direction == HorizontalDirectionFlags.Default) direction = HorizontalDirectionFlags.Left | HorizontalDirectionFlags.Right;
		return (direction.FastHasFlag(HorizontalDirectionFlags.Left), direction.FastHasFlag(HorizontalDirectionFlags.Right));
	}
}
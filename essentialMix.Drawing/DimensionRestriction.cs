using System;

namespace essentialMix.Drawing;

[Flags]
public enum DimensionRestriction
{
	None = 0,
	KeepWidth = 1,
	KeepHeight = 1 << 1
}
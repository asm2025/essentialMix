using System.Runtime.InteropServices;

namespace essentialMix.Numeric;

[StructLayout(LayoutKind.Explicit)]
public struct UnionLong
{
	[FieldOffset(0)]
	public long Value;

	[FieldOffset(0)]
	public int Low;

	[FieldOffset(sizeof(int))]
	public int High;

	public static explicit operator UnionLong(long value)
	{
		return new UnionLong
		{
			Value = value
		};
	}

	public static implicit operator long(UnionLong value) { return value.Value; }

	public static UnionLong FromHiLo(int low, int high)
	{
		return new UnionLong
		{
			Low = low,
			High = high
		};
	}
}
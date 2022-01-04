using System.Runtime.InteropServices;

namespace essentialMix.Numeric;

[StructLayout(LayoutKind.Explicit)]
public struct UnionInteger
{
	[FieldOffset(0)]
	public int Value;

	[FieldOffset(0)]
	public short Low;

	[FieldOffset(2)]
	public short High;

	public static explicit operator UnionInteger(int value)
	{
		return new UnionInteger
		{
			Value = value
		};
	}

	public static implicit operator int(UnionInteger value) { return value.Value; }

	public static UnionInteger FromHiLo(short low, short high)
	{
		return new UnionInteger
		{
			Low = low,
			High = high
		};
	}
}
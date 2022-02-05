using System.Runtime.InteropServices;

namespace essentialMix.Numeric;

[StructLayout(LayoutKind.Explicit)]
public struct UnionUnsignedInteger
{
	[FieldOffset(0)]
	public uint Value;

	[FieldOffset(0)]
	public ushort Low;

	[FieldOffset(2)]
	public ushort High;

	public static explicit operator UnionUnsignedInteger(uint value)
	{
		return new UnionUnsignedInteger
		{
			Value = value
		};
	}

	public static implicit operator uint(UnionUnsignedInteger value) { return value.Value; }

	public static UnionUnsignedInteger FromHiLo(ushort low, ushort high)
	{
		return new UnionUnsignedInteger
		{
			Low = low,
			High = high
		};
	}
}
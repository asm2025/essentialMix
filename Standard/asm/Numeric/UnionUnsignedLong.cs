using System.Runtime.InteropServices;

namespace asm.Numeric
{
	[StructLayout(LayoutKind.Explicit)]
	public struct UnionUnsignedLong
	{
		[FieldOffset(0)]
		public ulong Value;

		[FieldOffset(0)]
		public uint Low;

		[FieldOffset(8)]
		public uint High;

		public static explicit operator UnionUnsignedLong(ulong value)
		{
			return new UnionUnsignedLong
					{
						Value = value
					};
		}

		public static implicit operator ulong(UnionUnsignedLong value) { return value.Value; }

		public static UnionUnsignedLong FromHiLo(uint low, uint high)
		{
			return new UnionUnsignedLong
			{
				Low = low,
				High = high
			};
		}
	}
}
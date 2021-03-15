using System.Runtime.InteropServices;

namespace essentialMix.Numeric
{
	[StructLayout(LayoutKind.Explicit)]
	public struct UnionUnsignedShort
	{
		[FieldOffset(0)]
		public ushort Value;

		[FieldOffset(0)]
		public byte Low;

		[FieldOffset(1)]
		public byte High;

		public static explicit operator UnionUnsignedShort(ushort value)
		{
			return new UnionUnsignedShort
					{
						Value = value
					};
		}

		public static implicit operator ushort(UnionUnsignedShort value) { return value.Value; }

		public static UnionUnsignedShort FromHiLo(byte low, byte high)
		{
			return new UnionUnsignedShort
			{
				Low = low,
				High = high
			};
		}
	}
}
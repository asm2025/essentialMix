using System.Numerics;
using System.Runtime.InteropServices;

namespace essentialMix.Numeric
{
	[StructLayout(LayoutKind.Explicit)]
	public struct UnionBigInteger
	{
		[FieldOffset(0)]
		public BigInteger Value;

		[FieldOffset(0)]
		public uint Low;

		[FieldOffset(32)]
		public uint High;

		public static explicit operator UnionBigInteger(BigInteger value)
		{
			return new UnionBigInteger
					{
						Value = value
					};
		}

		public static implicit operator BigInteger(UnionBigInteger value) { return value.Value; }

		public static UnionBigInteger FromHiLo(uint low, uint high)
		{
			return new UnionBigInteger
			{
				Low = low,
				High = high
			};
		}
	}
}
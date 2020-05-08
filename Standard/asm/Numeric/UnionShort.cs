using System.Runtime.InteropServices;

namespace asm.Numeric
{
	[StructLayout(LayoutKind.Explicit)]
	public struct UnionShort
	{
		[FieldOffset(0)]
		public short Value;

		[FieldOffset(0)]
		public sbyte Low;

		[FieldOffset(1)]
		public sbyte High;

		public static explicit operator UnionShort(short value)
		{
			return new UnionShort
					{
						Value = value
					};
		}

		public static implicit operator short(UnionShort value) { return value.Value; }

		public static UnionShort FromHiLo(sbyte low, sbyte high)
		{
			return new UnionShort
			{
				Low = low,
				High = high
			};
		}
	}
}
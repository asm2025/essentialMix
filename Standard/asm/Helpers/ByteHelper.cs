using System;

namespace asm.Helpers
{
	public static class ByteHelper
	{
		public static int ToBase64Size(int value)
		{
			return value < 1
						? 0
						: 4 * (int)Math.Ceiling(value / 3.0d);
		}

		public static int ToBase64Size(byte[] value) { return ToBase64Size(value?.Length ?? 0); }

		public static int FromBase64Size(int value)
		{
			return value < 1
						? 0
						: value * 3 / 4;
		}

		public static int FromBase64Size(byte[] value) { return FromBase64Size(value?.Length ?? 0); }

		public static short GetByteSize(short value)
		{
			return value < 1
						? (short)0
						: (short)Math.Ceiling(value / 8.0d);
		}

		public static ushort GetByteSize(ushort value)
		{
			return value < 1
						? (ushort)0
						: (ushort)Math.Ceiling(value / 8.0d);
		}

		public static int GetByteSize(int value)
		{
			return value < 1
						? 0
						: (int)Math.Ceiling(value / 8.0d);
		}

		public static uint GetByteSize(uint value)
		{
			return value < 1
						? 0U
						: (uint)Math.Ceiling(value / 8.0d);
		}

		public static long GetByteSize(long value)
		{
			return value < 1L
						? 0L
						: (long)Math.Ceiling(value / 8.0d);
		}

		public static ulong GetByteSize(ulong value)
		{
			return value < 1
						? 0UL
						: (ulong)Math.Ceiling(value / 8.0d);
		}
	}
}
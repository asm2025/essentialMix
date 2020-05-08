using System;

namespace asm.Extensions
{
	public static class IntPtrExtension
	{
		private static readonly IntPtr INT_ONE = new IntPtr(1);
		private static readonly IntPtr INT_MINUS_ONE = new IntPtr(-1);

		public static bool IsZero(this IntPtr thisValue) { return thisValue == IntPtr.Zero; }

		public static bool IsOne(this IntPtr thisValue) { return thisValue == INT_ONE; }

		public static bool IsMinusOne(this IntPtr thisValue) { return thisValue == INT_MINUS_ONE; }

		public static bool IsInvalidHandle(this IntPtr thisValue) { return thisValue == IntPtr.Zero || thisValue == Win32.INVALID_HANDLE_VALUE; }

		public static int AsInt(this IntPtr thisValue) { return thisValue.ToInt32(); }

		public static long AsLong(this IntPtr thisValue) { return thisValue.ToInt64(); }
	}
}
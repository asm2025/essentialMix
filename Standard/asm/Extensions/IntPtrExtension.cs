using System;
using asm.Helpers;

namespace asm.Extensions
{
	public static class IntPtrExtension
	{
		public static bool IsZero(this IntPtr thisValue) { return thisValue == IntPtr.Zero; }

		public static bool IsOne(this IntPtr thisValue) { return thisValue == IntPtrHelper.One; }

		public static bool IsMinusOne(this IntPtr thisValue) { return thisValue == IntPtrHelper.MinusOne; }

		public static bool IsInvalidHandle(this IntPtr thisValue) { return thisValue == IntPtr.Zero || thisValue == Win32.INVALID_HANDLE_VALUE; }

		public static int AsInt(this IntPtr thisValue) { return thisValue.ToInt32(); }

		public static long AsLong(this IntPtr thisValue) { return thisValue.ToInt64(); }
	}
}
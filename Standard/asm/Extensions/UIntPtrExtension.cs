using System;

namespace asm.Extensions
{
	public static class UIntPtrExtension
	{
		private static readonly UIntPtr UINT_ONE = new UIntPtr(1);

		public static bool IsZero(this UIntPtr thisValue) { return thisValue == UIntPtr.Zero; }

		public static bool IsOne(this UIntPtr thisValue) { return thisValue == UINT_ONE; }

		public static uint AsInt(this UIntPtr thisValue) { return thisValue.ToUInt32(); }

		public static ulong AsLong(this UIntPtr thisValue) { return thisValue.ToUInt64(); }
	}
}
using System;
using asm.Helpers;

namespace asm.Extensions
{
	public static class UIntPtrExtension
	{
		public static bool IsZero(this UIntPtr thisValue) { return thisValue == UIntPtr.Zero; }

		public static bool IsOne(this UIntPtr thisValue) { return thisValue == UIntPtrHelper.One; }

		public static uint AsInt(this UIntPtr thisValue) { return thisValue.ToUInt32(); }

		public static ulong AsLong(this UIntPtr thisValue) { return thisValue.ToUInt64(); }
	}
}
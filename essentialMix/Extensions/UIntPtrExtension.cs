using System;
using System.Runtime.CompilerServices;
using essentialMix.Helpers;

namespace essentialMix.Extensions;

public static class UIntPtrExtension
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsZero(this UIntPtr thisValue) { return thisValue == UIntPtr.Zero; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsOne(this UIntPtr thisValue) { return thisValue == UIntPtrHelper.One; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static uint AsInt(this UIntPtr thisValue) { return thisValue.ToUInt32(); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static ulong AsLong(this UIntPtr thisValue) { return thisValue.ToUInt64(); }
}
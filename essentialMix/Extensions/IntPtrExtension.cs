using System;
using System.Runtime.CompilerServices;
using essentialMix.Helpers;

namespace essentialMix.Extensions;

public static class IntPtrExtension
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsZero(this IntPtr thisValue) { return thisValue == IntPtr.Zero; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsOne(this IntPtr thisValue) { return thisValue == IntPtrHelper.One; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsMinusOne(this IntPtr thisValue) { return thisValue == IntPtrHelper.MinusOne; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsInvalidHandle(this IntPtr thisValue) { return thisValue == IntPtr.Zero || thisValue == Win32.INVALID_HANDLE_VALUE; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int AsInt(this IntPtr thisValue) { return thisValue.ToInt32(); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static long AsLong(this IntPtr thisValue) { return thisValue.ToInt64(); }
}
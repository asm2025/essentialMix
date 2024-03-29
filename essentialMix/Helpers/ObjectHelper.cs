﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace essentialMix.Helpers;

public static class ObjectHelper
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static void Dispose<T>(ref T value)
		where T : IDisposable
	{
		Dispose(value);
		value = default(T);
	}

	public static void Dispose<T>(T value)
		where T : IDisposable
	{
		try
		{
			value?.Dispose();
		}
		catch (ObjectDisposedException)
		{
			// ignored
		}
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static void MarshalDispose<T>(ref T value)
	{
		MarshalDispose(value);
		value = default(T);
	}

	public static void MarshalDispose<T>(T value)
	{
		if (value is null) return;

		if (Marshal.IsComObject(value))
		{
			int ret = Marshal.ReleaseComObject(value);
			Debug.Assert(ret >= 0);
			return;
		}

		if (value is not IDisposable disposable) return;
		Dispose(disposable);
	}

	public static bool IsCompatible<T>(object value)
	{
		// Non-null values are fine. Only accept nulls if T is a class or Nullable<U>.
		// Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
		return value is T || value is null && !typeof(T).IsValueType;
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class ValueTypeHelper
{
	[NotNull]
	public static ISet<TypeCode> NumericTypes =>
		new HashSet<TypeCode>
		{
			TypeCode.Boolean,
			TypeCode.Char,
			TypeCode.SByte,
			TypeCode.Byte,
			TypeCode.Int16,
			TypeCode.UInt16,
			TypeCode.Int32,
			TypeCode.UInt32,
			TypeCode.Int64,
			TypeCode.UInt64,
			TypeCode.Single,
			TypeCode.Double,
			TypeCode.Decimal
		};

	[NotNull]
	public static ISet<TypeCode> IntegralTypes =>
		new HashSet<TypeCode>
		{
			TypeCode.Boolean,
			TypeCode.Char,
			TypeCode.SByte,
			TypeCode.Byte,
			TypeCode.Int16,
			TypeCode.UInt16,
			TypeCode.Int32,
			TypeCode.UInt32,
			TypeCode.Int64,
			TypeCode.UInt64
		};

	[NotNull]
	public static ISet<TypeCode> FloatingTypes =>
		new HashSet<TypeCode>
		{
			TypeCode.Single,
			TypeCode.Double,
			TypeCode.Decimal
		};

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(sbyte value, sbyte total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(byte value, byte total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(short value, short total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(ushort value, ushort total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(int value, int total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(uint value, uint total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(long value, long total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(ulong value, ulong total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(float value, float total) { return Percent((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(decimal value, decimal total) { return Percent((double)value, (double)total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int Percent(double value, double total) { return ((int)(value / total.NotBelow(1.0d) * 100.0d)).Within(0, 100); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(sbyte value, sbyte total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(byte value, byte total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(short value, short total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(ushort value, ushort total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(int value, int total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(uint value, uint total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(long value, long total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(ulong value, ulong total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(float value, float total) { return PercentF((double)value, total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(decimal value, decimal total) { return PercentF((double)value, (double)total); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static double PercentF(double value, double total) { return (value / total.NotBelow(1.0d) * 100.0d).Within(0.0d, 100.0d); }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static short MakeWord(byte x, byte y)
	{
		return (short)((byte)(x & 0xff) | ((byte)(y & 0xff) << 8));
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int MakeDWord(short x, short y)
	{
		return (x & 0xffff) | ((y & 0xffff) << 16);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static long MakeLong(int x, int y)
	{
		return (x & 0xffffffff) | ((y & 0xffffffff) << 32);
	}
}
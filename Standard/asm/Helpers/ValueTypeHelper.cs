using System;
using System.Collections.Generic;
using asm.Extensions;
using asm.Collections;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class ValueTypeHelper
	{
		[NotNull]
		public static IReadOnlySet<TypeCode> NumericTypes =>
			new ReadOnlySet<TypeCode>(new HashSet<TypeCode>
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
			});

		[NotNull]
		public static IReadOnlySet<TypeCode> IntegralTypes =>
			new ReadOnlySet<TypeCode>(new HashSet<TypeCode>
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
			});

		[NotNull]
		public static IReadOnlySet<TypeCode> FloatingTypes =>
			new ReadOnlySet<TypeCode>(new HashSet<TypeCode>
			{
				TypeCode.Single,
				TypeCode.Double,
				TypeCode.Decimal
			});

		public static int Percent(sbyte value, sbyte total) { return Percent((double)value, total); }

		public static int Percent(byte value, byte total) { return Percent((double)value, total); }

		public static int Percent(short value, short total) { return Percent((double)value, total); }

		public static int Percent(ushort value, ushort total) { return Percent((double)value, total); }

		public static int Percent(int value, int total) { return Percent((double)value, total); }

		public static int Percent(uint value, uint total) { return Percent((double)value, total); }

		public static int Percent(long value, long total) { return Percent((double)value, total); }

		public static int Percent(ulong value, ulong total) { return Percent((double)value, total); }

		public static int Percent(float value, float total) { return Percent((double)value, total); }

		public static int Percent(decimal value, decimal total) { return Percent((double)value, (double)total); }

		public static int Percent(double value, double total) { return ((int)(value / total.NotBelow(1.0d) * 100.0d)).Within(0, 100); }

		public static double PercentF(sbyte value, sbyte total) { return PercentF((double)value, total); }

		public static double PercentF(byte value, byte total) { return PercentF((double)value, total); }

		public static double PercentF(short value, short total) { return PercentF((double)value, total); }

		public static double PercentF(ushort value, ushort total) { return PercentF((double)value, total); }

		public static double PercentF(int value, int total) { return PercentF((double)value, total); }

		public static double PercentF(uint value, uint total) { return PercentF((double)value, total); }

		public static double PercentF(long value, long total) { return PercentF((double)value, total); }

		public static double PercentF(ulong value, ulong total) { return PercentF((double)value, total); }

		public static double PercentF(float value, float total) { return PercentF((double)value, total); }

		public static double PercentF(decimal value, decimal total) { return PercentF((double)value, (double)total); }

		public static double PercentF(double value, double total) { return (value / total.NotBelow(1.0d) * 100.0d).Within(0.0d, 100.0d); }

		public static short MakeWord(byte x, byte y)
		{
			return (short)((byte)(x & 0xff) | ((byte)(y & 0xff) << 8));
		}

		public static int MakeDWord(short x, short y)
		{
			return (x & 0xffff) | ((y & 0xffff) << 16);
		}

		public static long MakeLong(int x, int y)
		{
			return (x & 0xffffffff) | ((y & 0xffffffff) << 32);
		}
	}
}
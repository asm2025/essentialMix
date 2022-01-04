using System;
using System.Windows;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF
{
	public enum HeaderSize
	{
		H6,
		H5,
		H4,
		H3,
		H2,
		H1
	}

	public static class HeaderSizeExtension
	{
		public static double ToFontSize(this HeaderSize thisValue)
		{
			return Math.Round(SystemFonts.MessageFontSize * (1d + ((double)thisValue + 1d) / 10d));
		}
	}

	public static class HeaderSizeHelper
	{
		public static double H1 => HeaderSize.H1.ToFontSize();
		public static double H2 => HeaderSize.H2.ToFontSize();
		public static double H3 => HeaderSize.H3.ToFontSize();
		public static double H4 => HeaderSize.H4.ToFontSize();
		public static double H5 => HeaderSize.H5.ToFontSize();
		public static double H6 => HeaderSize.H6.ToFontSize();

		public static HeaderSize From([NotNull] string value)
		{
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			return (HeaderSize)Enum.Parse(typeof(HeaderSize), value, true);
		}

		public static HeaderSize From(short value)
		{
			if (!value.InRange((short)HeaderSize.H6, (short)HeaderSize.H1)) throw new ArgumentOutOfRangeException(nameof(value));
			return (HeaderSize)value;
		}

		public static HeaderSize From(ushort value)
		{
			if (!value.InRange((ushort)HeaderSize.H6, (ushort)HeaderSize.H1)) throw new ArgumentOutOfRangeException(nameof(value));
			return (HeaderSize)value;
		}

		public static HeaderSize From(int value)
		{
			if (!value.InRange((int)HeaderSize.H6, (int)HeaderSize.H1)) throw new ArgumentOutOfRangeException(nameof(value));
			return (HeaderSize)value;
		}

		public static HeaderSize From(uint value)
		{
			if (!value.InRange((uint)HeaderSize.H6, (uint)HeaderSize.H1)) throw new ArgumentOutOfRangeException(nameof(value));
			return (HeaderSize)value;
		}

		public static HeaderSize From(long value)
		{
			if (!value.InRange((long)HeaderSize.H6, (long)HeaderSize.H1)) throw new ArgumentOutOfRangeException(nameof(value));
			return (HeaderSize)value;
		}

		public static HeaderSize From(ulong value)
		{
			if (!value.InRange((ulong)HeaderSize.H6, (ulong)HeaderSize.H1)) throw new ArgumentOutOfRangeException(nameof(value));
			return (HeaderSize)value;
		}

		public static HeaderSize From(object value)
		{
			return value switch
			{
				HeaderSize hs => hs,
				string s => From(s),
				short i => From(i),
				ushort i => From(i),
				int i => From(i),
				uint i => From(i),
				long i => From(i),
				ulong i => From(i),
				null => HeaderSize.H6,
				_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
			};
		}
	}
}
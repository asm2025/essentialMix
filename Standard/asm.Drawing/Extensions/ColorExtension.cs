using System;
using System.Drawing;
using asm.Collections;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ColorExtension
	{
		public static uint ToDWORD(this Color thisValue) { return thisValue.R + ((uint)thisValue.G << 8) + ((uint)thisValue.B << 16); }

		public static COLORREF ToColorRef(this Color thisValue) { return new COLORREF(ToDWORD(thisValue)); }

		[NotNull]
		public static string ToHex(this Color thisValue)
		{
			return thisValue.A < byte.MaxValue
						? $"#{thisValue.A:X2}{thisValue.R:X2}{thisValue.G:X2}{thisValue.B:X2}"
						: $"#{thisValue.R:X2}{thisValue.G:X2}{thisValue.B:X2}";
		}

		[NotNull]
		public static string ToRGB(this Color thisValue)
		{
			return thisValue.A < byte.MaxValue
						? $"rgba({thisValue.A},{thisValue.R},{thisValue.G},{thisValue.B})"
						: $"rgb({thisValue.R},{thisValue.G},{thisValue.B})";
		}

		public static ConsoleColor ToConsoleColor(this Color thisValue)
		{
			EnumRange<ConsoleColor> consoleColors = new EnumRange<ConsoleColor>();
			ConsoleColor result = consoleColors.Minimum;
			double red = thisValue.R, green = thisValue.G, blue = thisValue.B, delta = double.MaxValue;

			foreach (ConsoleColor consoleColor in consoleColors)
			{
				Color color = Color.FromName(consoleColor == ConsoleColor.DarkYellow 
					? nameof(Color.Orange) 
					: consoleColor.GetName());
				double t = Math.Pow(color.R - red, 2.0) + Math.Pow(color.G - green, 2.0) + Math.Pow(color.B - blue, 2.0);
				if (t.IsEqual(0.0d)) return consoleColor;

				if (t < delta)
				{
					delta = t;
					result = consoleColor;
				}
			}

			return result;
		}

		public static uint ToUInt(this Color thisValue) { return (uint)((thisValue.A << 24) | (thisValue.R << 16) | (thisValue.G << 8) | thisValue.B); }

		public static bool IsSame(this Color thisValue, Color other, float threshold = 0.1f)
		{
			if (!threshold.InRange(0.0f, 1.0f)) throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be in the range of 0.0 to 1.0");

			float lowerBound = 1.0f - threshold;
			float upperBound = 1.0f + threshold;
			return ((float)thisValue.R).InRange(other.R * lowerBound, other.R * upperBound)
				&& ((float)thisValue.G).InRange(other.G * lowerBound, other.G * upperBound)
				&& ((float)thisValue.B).InRange(other.B * lowerBound, other.B * upperBound);
		}
	}
}
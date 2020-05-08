using System;
using JetBrains.Annotations;

namespace asm.Drawing
{
	/// <summary>
	///     HSL components.
	/// </summary>
	/// <remarks>The class encapsulates <b>HSL</b> color components.</remarks>
	public class Hsl
	{
		/// <summary>
		///     Hue component.
		/// </summary>
		/// <remarks>Hue is measured in the range of [0, 359].</remarks>
		public int Hue;

		/// <summary>
		///     Saturation component.
		/// </summary>
		/// <remarks>Saturation is measured in the range of [0, 1].</remarks>
		public float Saturation;

		/// <summary>
		///     Luminance value.
		/// </summary>
		/// <remarks>Luminance is measured in the range of [0, 1].</remarks>
		public float Luminance;

		/// <summary>
		///     Initializes a new instance of the <see cref="Hsl" /> class.
		/// </summary>
		public Hsl() { }

		/// <summary>
		///     Initializes a new instance of the <see cref="Hsl" /> class.
		/// </summary>
		/// <param name="hue">Hue component.</param>
		/// <param name="saturation">Saturation component.</param>
		/// <param name="luminance">Luminance component.</param>
		public Hsl(int hue, float saturation, float luminance)
		{
			Hue = hue;
			Saturation = saturation;
			Luminance = luminance;
		}

		/// <summary>
		///     Convert the color to <b>RGB</b> color space.
		/// </summary>
		/// <returns>Returns <see cref="Rgb" /> instance, which represents converted color value.</returns>
		[NotNull]
		public Rgb ToRGB()
		{
			Rgb rgb = new Rgb();
			ToRGB(this, rgb);
			return rgb;
		}

		/// <summary>
		///     Convert from RGB to HSL color space.
		/// </summary>
		/// <param name="rgb">Source color in <b>RGB</b> color space.</param>
		/// <param name="hsl">Destination color in <b>HSL</b> color space.</param>
		/// <remarks>
		///     <para>
		///         See
		///         <a href="http://en.wikipedia.org/wiki/HSI_color_space#Conversion_from_RGB_to_HSL_or_HSV">HSL and HSV Wiki</a>
		///         for information about the algorithm to convert from RGB to HSL.
		///     </para>
		/// </remarks>
		public static void FromRGB([NotNull] Rgb rgb, [NotNull] Hsl hsl)
		{
			float r = rgb.Red / 255.0f;
			float g = rgb.Green / 255.0f;
			float b = rgb.Blue / 255.0f;

			float min = Math.Min(Math.Min(r, g), b);
			float max = Math.Max(Math.Max(r, g), b);
			float delta = max - min;

			// get luminance value
			hsl.Luminance = (max + min) / 2;

			if (Math.Abs(delta) < float.Epsilon)
			{
				// gray color
				hsl.Hue = 0;
				hsl.Saturation = 0.0f;
			}
			else
			{
				// get saturation value
				hsl.Saturation = hsl.Luminance <= 0.5
									? delta / (max + min)
									: delta / (2 - max - min);

				// get hue value
				float hue = Math.Abs(r - max) < float.Epsilon
								? (g - b) / 6 / delta
								: Math.Abs(g - max) < float.Epsilon
									? 1.0f / 3 + (b - r) / 6 / delta
									: 2.0f / 3 + (r - g) / 6 / delta;

				// correct hue if needed
				if (hue < 0) hue += 1;
				if (hue > 1) hue -= 1;
				hsl.Hue = (int)(hue * 360);
			}
		}

		/// <summary>
		///     Convert from RGB to HSL color space.
		/// </summary>
		/// <param name="rgb">Source color in <b>RGB</b> color space.</param>
		/// <returns>Returns <see cref="Hsl" /> instance, which represents converted color value.</returns>
		[NotNull]
		public static Hsl FromRGB([NotNull] Rgb rgb)
		{
			Hsl hsl = new Hsl();
			FromRGB(rgb, hsl);
			return hsl;
		}

		/// <summary>
		///     Convert from HSL to RGB color space.
		/// </summary>
		/// <param name="hsl">Source color in <b>HSL</b> color space.</param>
		/// <param name="rgb">Destination color in <b>RGB</b> color space.</param>
		public static void ToRGB([NotNull] Hsl hsl, [NotNull] Rgb rgb)
		{
			if (Math.Abs(hsl.Saturation) < float.Epsilon)
			{
				// gray values
				rgb.Red = rgb.Green = rgb.Blue = (byte)(hsl.Luminance * 255);
			}
			else
			{
				float hue = (float)hsl.Hue / 360;
				float v2 = hsl.Luminance < 0.5
								? hsl.Luminance * (1 + hsl.Saturation)
								: hsl.Luminance + hsl.Saturation - hsl.Luminance * hsl.Saturation;
				float v1 = 2 * hsl.Luminance - v2;
				rgb.Red = (byte)(255 * Hue_2_RGB(v1, v2, hue + 1.0f / 3));
				rgb.Green = (byte)(255 * Hue_2_RGB(v1, v2, hue));
				rgb.Blue = (byte)(255 * Hue_2_RGB(v1, v2, hue - 1.0f / 3));
			}

			rgb.Alpha = 255;
		}

		#region Private members

		// HSL to RGB helper routine
		private static float Hue_2_RGB(float v1, float v2, float vH)
		{
			if (vH < 0) vH += 1;
			if (vH > 1) vH -= 1;
			if (6 * vH < 1) return v1 + (v2 - v1) * 6 * vH;
			if (2 * vH < 1) return v2;
			if (3 * vH < 2) return v1 + (v2 - v1) * (2.0f / 3 - vH) * 6;
			return v1;
		}

		#endregion
	}
}
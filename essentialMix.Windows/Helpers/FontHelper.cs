using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using essentialMix.Comparers;
using essentialMix.Extensions;
using JetBrains.Annotations;
using SysFontFamily = System.Drawing.FontFamily;

namespace essentialMix.Windows.Helpers;

public static class FontHelper
{
	private static readonly ISet<string> __pannedMonospaceFontNames = new HashSet<string>(StringFunctionalComparer.StartsWithOrdinalIgnoreCase)
	{
		"ESRI",
		"Oc_"
	};

	private static ISet<string> __cachedMonospacedFontNames;

	public static bool IsMonospaced([NotNull] Font value, [NotNull] Graphics g) { return g.MeasureString("i", value).Width.IsEqual(g.MeasureString("W", value).Width); }

	public static bool IsSymbolFont([NotNull] Font font)
	{
		const byte SYMBOL_FONT = 2;

		LOGFONT logicalFont = new LOGFONT();
		font.ToLogFont(logicalFont);
		return logicalFont.lfCharSet == SYMBOL_FONT;
	}

	[NotNull]
	public static ISet<string> GetMonospacedFontNames()
	{
		if (__cachedMonospacedFontNames != null) return __cachedMonospacedFontNames;

		FontStyle[] requiredStyles =
		[
			FontStyle.Regular,
			FontStyle.Bold,
			FontStyle.Italic
		];
		ISet<string> cachedMonospacedFontNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		__cachedMonospacedFontNames = new HashSet<string>(cachedMonospacedFontNames);

		using (InstalledFontCollection ifc = new InstalledFontCollection())
		{
			using (Bitmap bmp = new Bitmap(1, 1))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					foreach (SysFontFamily fontFamily in ifc.Families)
					{
						if (__pannedMonospaceFontNames.Contains(fontFamily.Name)) continue;
						if (!HasAllStyles(fontFamily, requiredStyles)) continue;

						using (Font font = new Font(fontFamily, 10))
						{
							if (!IsMonospaced(font, g) || IsSymbolFont(font)) continue;
							cachedMonospacedFontNames.Add(fontFamily.Name);
						}
					}
				}
			}
		}

		return __cachedMonospacedFontNames;

		static bool HasAllStyles([NotNull] SysFontFamily thisValue, [NotNull] FontStyle[] styles)
		{
			return styles.Length == 0 || styles.All(thisValue.IsStyleAvailable);
		}
	}

	public static SizeF MeasureString(string value, CONSOLE_FONT_INFO_EX fontInfoEx)
	{
		if (string.IsNullOrEmpty(value)) return SizeF.Empty;
		if (fontInfoEx == null) return SizeF.Empty;

		using (Font font = new Font(fontInfoEx.lpszFaceName, fontInfoEx.nWidth, fontInfoEx.nWeight.IsBold() ? FontStyle.Bold : FontStyle.Regular))
		{
			using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					return g.MeasureString(value, font);
				}
			}
		}
	}
}
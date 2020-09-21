using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using asm.Collections;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Comparers;

namespace asm.Drawing.Helpers
{
	public static class FontHelper
	{
		private static readonly ISet<string> PANNED_MONOSPACE_FONT_NAMES = new HashSet<string>(StringFunctionalComparer.StartsWithOrdinalIgnoreCase)
		{
			"ESRI",
			"Oc_"
		};

		private static IReadOnlySet<string> _cachedMonospacedFontNames;

		public static bool IsMonospaced([NotNull] Font value, [NotNull] Graphics g) { return g.MeasureString("i", value).Width.IsEqual(g.MeasureString("W", value).Width); }

		public static bool IsSymbolFont([NotNull] Font font)
		{
			const byte SYMBOL_FONT = 2;

			Win32.LOGFONT logicalFont = new Win32.LOGFONT();
			font.ToLogFont(logicalFont);
			return logicalFont.lfCharSet == SYMBOL_FONT;
		}

		[NotNull]
		public static IReadOnlySet<string> GetMonospacedFontNames()
		{
			if (_cachedMonospacedFontNames == null)
			{
				FontStyle[] requiredStyles = 
				{
					FontStyle.Regular,
					FontStyle.Bold,
					FontStyle.Italic
				};
				ISet<string> cachedMonospacedFontNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				_cachedMonospacedFontNames = new ReadOnlySet<string>(cachedMonospacedFontNames);

				using (InstalledFontCollection ifc = new InstalledFontCollection())
				{
					using (Bitmap bmp = new Bitmap(1, 1))
					{
						using (Graphics g = Graphics.FromImage(bmp))
						{
							foreach (FontFamily fontFamily in ifc.Families)
							{
								if (PANNED_MONOSPACE_FONT_NAMES.Contains(fontFamily.Name)) continue;
								if (!fontFamily.HasStyles(requiredStyles)) continue;

								using (Font font = new Font(fontFamily, 10))
								{
									if (!IsMonospaced(font, g) || IsSymbolFont(font)) continue;
									cachedMonospacedFontNames.Add(fontFamily.Name);
								}
							}
						}
					}
				}
			}
			
			return _cachedMonospacedFontNames;
		}

		public static SizeF MeasureString(string value, Win32.CONSOLE_FONT_INFO_EX fontInfoEx)
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
}
using System.IO;
using JetBrains.Annotations;

namespace essentialMix.Logging.Internal
{
	internal static class TextWriterExtension
	{
		/*
		 * Write order:
		 * 1. background color
		 * 2. foreground color
		 * 3. message
		 * 4. reset foreground color
		 * 5. reset background color
		*/
		public static bool WriteColors([NotNull] this TextWriter thisValue, ConsoleColors colors)
		{
			// compose the string to avoid race conditions
			string colorsText = colors.Background?.GetBackgroundColorEscapeCode() + colors.Foreground?.GetForegroundColorEscapeCode();
			if (string.IsNullOrEmpty(colorsText)) return false;
			thisValue.Write(colorsText);
			return true;
		}

		public static void ResetColors([NotNull] this TextWriter thisValue)
		{
			thisValue.Write(ConsoleColorExtension.FOREGROUND_COLOR);
			thisValue.Write(ConsoleColorExtension.BACKGROUND_COLOR);
		}

		public static void ColorfulWrite([NotNull] this TextWriter thisValue, string text, ConsoleColors colors)
		{
			if (string.IsNullOrEmpty(text)) return;

			// compose the string to avoid race conditions
			string colorsText = colors.Background?.GetBackgroundColorEscapeCode() + colors.Foreground?.GetForegroundColorEscapeCode();
			string resetText = string.IsNullOrEmpty(colorsText)
									? null
									: ConsoleColorExtension.FOREGROUND_COLOR + ConsoleColorExtension.BACKGROUND_COLOR;
			thisValue.Write(colorsText + text + resetText);
		}
	}
}
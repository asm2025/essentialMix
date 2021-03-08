using System;
using System.IO;
using JetBrains.Annotations;

namespace asm.Logging.Internal
{
	internal static class TextWriterExtensions
	{
		/*
		 * Write order:
		 * 1. background color
		 * 2. foreground color
		 * 3. message
		 * 4. reset foreground color
		 * 5. reset background color
		*/
		public static bool WriteColor([NotNull] this TextWriter thisValue, ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			if (background.HasValue) thisValue.Write(background.Value);
			if (foreground.HasValue) thisValue.Write(foreground.Value);
			return background.HasValue || foreground.HasValue;
		}

		public static void ResetColor([NotNull] this TextWriter thisValue)
		{
			thisValue.Write(ConsoleColorHelper.ForegroundColor);
			thisValue.Write(ConsoleColorHelper.BackgroundColor);
		}
	}
}
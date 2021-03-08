using System;

namespace asm.Logging
{
	public readonly struct ConsoleColors
	{
		public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
		{
			Foreground = foreground;
			Background = background;
		}

		public ConsoleColor? Foreground { get; }

		public ConsoleColor? Background { get; }
	}
}
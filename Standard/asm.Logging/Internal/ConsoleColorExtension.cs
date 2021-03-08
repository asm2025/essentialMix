using System;
using JetBrains.Annotations;

namespace asm.Logging.Internal
{
	internal static class ConsoleColorExtension
	{
		[NotNull]
		public static string GetForegroundColorEscapeCode(this ConsoleColor thisValue)
		{
			return thisValue switch
			{
				ConsoleColor.Black =>       "\x1B[30m",
				ConsoleColor.DarkRed =>     "\x1B[31m",
				ConsoleColor.DarkGreen =>   "\x1B[32m",
				ConsoleColor.DarkYellow =>  "\x1B[33m",
				ConsoleColor.DarkBlue =>    "\x1B[34m",
				ConsoleColor.DarkMagenta => "\x1B[35m",
				ConsoleColor.DarkCyan =>    "\x1B[36m",
				ConsoleColor.Gray =>        "\x1B[37m",
				ConsoleColor.Red =>         "\x1B[1m\x1B[31m",
				ConsoleColor.Green =>       "\x1B[1m\x1B[32m",
				ConsoleColor.Yellow =>      "\x1B[1m\x1B[33m",
				ConsoleColor.Blue =>        "\x1B[1m\x1B[34m",
				ConsoleColor.Magenta =>     "\x1B[1m\x1B[35m",
				ConsoleColor.Cyan =>        "\x1B[1m\x1B[36m",
				ConsoleColor.White =>       "\x1B[1m\x1B[37m",
				_ => ConsoleColorHelper.ForegroundColor
			};
		}

		[NotNull]
		public static string GetBackgroundColorEscapeCode(this ConsoleColor thisValue)
		{
			return thisValue switch
			{
				ConsoleColor.Black =>       "\x1B[40m",
				ConsoleColor.DarkRed =>     "\x1B[41m",
				ConsoleColor.DarkGreen =>   "\x1B[42m",
				ConsoleColor.DarkYellow =>  "\x1B[43m",
				ConsoleColor.DarkBlue =>    "\x1B[44m",
				ConsoleColor.DarkMagenta => "\x1B[45m",
				ConsoleColor.DarkCyan =>    "\x1B[46m",
				ConsoleColor.Gray =>        "\x1B[47m",
				_ => ConsoleColorHelper.BackgroundColor
			};
		}
	}
}
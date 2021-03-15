using Microsoft.Extensions.Logging.Console;

namespace essentialMix.Logging
{
	public class ColoredConsoleFormatterOptions : SimpleConsoleFormatterOptions
	{
		public ConsoleColors TraceColors { get; set; }
		public ConsoleColors DebugColors { get; set; }
		public ConsoleColors InformationColors { get; set; }
		public ConsoleColors WarningColors { get; set; }
		public ConsoleColors ErrorColors { get; set; }
		public ConsoleColors CriticalColors { get; set; }
	}
}
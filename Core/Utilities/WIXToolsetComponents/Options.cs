using CommandLine;
using Microsoft.Extensions.Logging;

namespace WIXToolsetComponents
{
	public class Options
	{
		[Option('l', "log", Required = false, HelpText = "Set logging level.")]
		public LogLevel LogLevel { get; set; }
	}
}

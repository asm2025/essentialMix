using System.Collections.Generic;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace WiXComponents
{
	public class Options
	{
		[Option(HelpText = "Input files to be read. Supported file types are *.wixproj, *.csproj, *.vbproj, *.wxs, *.wsi, *.xml")]
		public ICollection<string> Files { get; set; }

		[Option('l', "log", HelpText = "Enables logging with the specified level.")]
		public LogLevel LogLevel { get; set; }
	}
}

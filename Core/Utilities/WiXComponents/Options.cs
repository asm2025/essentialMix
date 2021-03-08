using System.Collections.Generic;
using System.Reflection;
using asm.Extensions;
using CommandLine;
using CommandLine.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace WiXComponents
{
	public class Options
	{
		public Options()
		{
		}

		[Value(0, MetaName = "Input Files", HelpText = "Input files to be read. Supported file types are *.wixproj, *.csproj, *.vbproj, *.wxs, *.wsi, *.xml")]
		public ICollection<string> Files { get; set; }

		[Option('l', "log", Default = LogLevel.None, HelpText = "Enables logging with the specified level.")]
		public LogLevel LogLevel { get; set; }

		[NotNull]
		public static string GetUsage([NotNull] ParserResult<Options> result)
		{
			return HelpText.AutoBuild(result, helpText => HelpText.DefaultParsingErrorsHandler(result, SetHelpTextProps(helpText)));
		}

		[NotNull]
		public static HeadingInfo GetHeader(Assembly assembly = null)
		{
			Assembly asm = assembly ?? Assembly.GetExecutingAssembly();
			return new HeadingInfo(string.Concat(asm.GetCompany(), ' ', asm.GetTitle()), asm.GetFileVersion());
		}

		[NotNull]
		private static HelpText SetHelpTextProps([NotNull] HelpText helpText, Assembly assembly = null)
		{
			Assembly asm = assembly ?? Assembly.GetExecutingAssembly();
			helpText.Heading = GetHeader(asm);
			helpText.Copyright = asm.GetCopyright();
			helpText.AddDashesToOption = true;
			helpText.AddEnumValuesToHelpText = true;
			helpText.AdditionalNewLineAfterOption = true;
			return helpText;
		}
	}
}

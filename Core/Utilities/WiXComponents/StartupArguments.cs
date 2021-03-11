using System.Collections.Generic;
using System.Reflection;
using asm.Extensions;
using CommandLine;
using CommandLine.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace WiXComponents
{
	public class StartupArguments
	{
		private const string SEC_INPUT = "INPUT";

		public StartupArguments()
		{
		}

		[Option('f', "files", SetName = SEC_INPUT, HelpText = @"If a file or more are provided, a search inside these files is conducted to replace each WiX toolset 
component's GUID with a new random one. This can be useful when a previous installer's components have corrupted registry entries; typically on a developer computer.
Supported file types are wixproj, csproj, vbproj, wxs, wsi, xml.")]
		public ICollection<string> Files { get; set; }

		[Option('d', "directory", SetName = SEC_INPUT, HelpText = "Searches a path for wxs and wsi files to perform component's GUID replacement. See files argument for details.")]
		public string Directory { get; set; }

		[Option('l', "log", Default = LogLevel.None, HelpText = "Enables logging with the specified level.")]
		public LogLevel LogLevel { get; set; }

		[NotNull]
		public static string GetUsage([NotNull] ParserResult<StartupArguments> result)
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

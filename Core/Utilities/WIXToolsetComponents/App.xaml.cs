using System;
using System.IO;
using System.Text;
using System.Windows;
using asm.Extensions;
using asm.Helpers;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WIXToolsetComponents
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			Console.OutputEncoding = Encoding.UTF8;
			Directory.SetCurrentDirectory(AssemblyHelper.GetEntryAssembly().GetDirectoryPath());
			
			// Configuration
			IConfiguration configuration = IConfigurationBuilderHelper.CreateConfiguration()
																	.AddConfigurationFiles(EnvironmentHelper.GetEnvironmentName())
																	.AddEnvironmentVariables()
																	.AddUserSecrets()
																	.Build();

			// Command line
			string[] args = Environment.GetCommandLineArgs();
			Parser.Default.ParseArguments<Options>(args);

			// Logging
			LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
			if (configuration.GetValue<bool>("Logging:Enabled")) loggerConfiguration.ReadFrom.Configuration(configuration);
			Log.Logger = loggerConfiguration.CreateLogger();

			base.OnStartup(e);
		}

		/// <inheritdoc />
		protected override void OnExit(ExitEventArgs e)
		{
			Log.CloseAndFlush();
			base.OnExit(e);
		}


	}
}

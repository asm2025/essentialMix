using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using asm.Extensions;
using asm.Helpers;
using asm.Newtonsoft.Helpers;
using CommandLine;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using WiXComponents.Views;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WiXComponents
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private IConfiguration _configuration;
		public IServiceProvider ServiceProvider { get; private set; }

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			ConsoleHelper.AttachConsole(out bool consoleCreated);
			
			if (ConsoleHelper.HasConsole)
			{
				ConsoleHelper.Show();
			}
			else if (e.Args.Length > 0)
			{
				throw new Exception("Could not create console window.");
			}

			// Setup and command line processing
			JsonConvert.DefaultSettings = () => JsonHelper.CreateSettings();
			string basePath = Assembly.GetExecutingAssembly().GetDirectoryPath();

			// Configuration
			_configuration = IConfigurationBuilderHelper.CreateConfiguration(basePath)
														.AddConfigurationFiles(basePath, EnvironmentHelper.GetEnvironmentName())
														.AddEnvironmentVariables()
														.AddUserSecrets()
														.AddArguments(e.Args)
														.Build();

			// Logging
			LoggingLevelSwitch levelSwitch = new();
			LoggerConfiguration loggerConfiguration = new ();
			loggerConfiguration.MinimumLevel.ControlledBy(levelSwitch);
			
			Parser parser = null;

			try
			{
				// Command line
				parser = new Parser(settings =>
				{
					settings.CaseSensitive = false;
					settings.CaseInsensitiveEnumValues = true;
					settings.HelpWriter = Console.Error;
				});

				ParserResult<StartupArguments> result = parser.ParseArguments<StartupArguments>(e.Args);
				ObjectHelper.Dispose(ref parser);

				if (result.Tag == ParserResultType.NotParsed)
				{
					Shutdown();
					return;
				}

				StartupArguments args = ((Parsed<StartupArguments>)result).Value;

				// Logging:2
				if (_configuration.GetValue<bool>("Logging:Enabled") && args.LogLevel > LogLevel.None)
				{
					loggerConfiguration.ReadFrom.Configuration(_configuration);
					levelSwitch.MinimumLevel = args.LogLevel switch
					{
						LogLevel.Trace => LogEventLevel.Verbose,
						LogLevel.Debug => LogEventLevel.Debug,
						LogLevel.Information => LogEventLevel.Information,
						LogLevel.Warning => LogEventLevel.Warning,
						LogLevel.Error => LogEventLevel.Error,
						_ => LogEventLevel.Fatal
					};
					Log.Logger = loggerConfiguration.CreateLogger();
				}

				// Services
				ConfigureServices(args);

				// Command line
				if (args.Files.Count > 0 || !string.IsNullOrEmpty(args.Directory))
				{
					ProcessCommandLine(args);
					if (consoleCreated) ConsoleHelper.FreeConsole();
					Shutdown();
					return;
				}

				// Start the main window
				ConsoleHelper.Hide();
				if (consoleCreated) ConsoleHelper.FreeConsole();
				base.OnStartup(e);
				Start();
			}
			catch (Exception ex)
			{
				// This won't produce log until a logger is setup in the above try block
				Log.Error(ex.CollectMessages());
				Shutdown();
			}
			finally
			{
				// in case an error occurred and the parser was not disposed
				ObjectHelper.Dispose(ref parser);
			}
		}

		/// <inheritdoc />
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Log.CloseAndFlush();
		}

		private void ConfigureServices([NotNull] StartupArguments startupArguments)
		{
			ServiceCollection services = new();
			services.AddSingleton(startupArguments);
			services.AddLogging(builder => builder.AddSerilog(null, true)
												.AddColoredConsoleFormatter());
			services.AddTransient<MainView>();
			ServiceProvider = services.BuildServiceProvider();
		}

		private void Start()
		{
			MainView window = ServiceProvider.GetRequiredService<MainView>();
			window.Show();
		}

		private void ProcessCommandLine([NotNull] StartupArguments args)
		{
			// When we are here, args either has files or a directory to be processed.
			ILogger logger = ServiceProvider.GetService<ILogger<App>>();

			if (string.IsNullOrEmpty(args.Directory))
			{
				ProcessFiles(args.Files, logger);
				return;
			}

			if (!Directory.Exists(args.Directory))
			{
				logger.LogError("Path not found.");
				return;
			}

			ProcessFiles(DirectoryHelper.Enumerate(args.Directory, "*.wxs;*.wsi;*.xml", args.IncludeSubDirectory
																								? SearchOption.AllDirectories
																								: SearchOption.TopDirectoryOnly), logger);
		}

		private void ProcessFiles([NotNull] IEnumerable<string> files, ILogger logger)
		{
			foreach (string file in files)
			{
				logger.LogInformation(file);
			}

			//string fileName = PathHelper.Trim(args[0]);

			//if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
			//{
			//	Console.WriteLine("File not found.");
			//	return 1;
			//}

			//List<string> files = new List<string>();

			//try
			//{
			//	XmlDocument doc = XmlDocumentHelper.LoadFile(fileName);
			//	XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
			//	manager.AddNamespace("msbld", "http://schemas.microsoft.com/developer/msbuild/2003");

			//	// Finds all of the files included in the project.
			//	XmlNodeList nodes = doc.SelectNodes("/")
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.CollectMessages());
			//	return 1;
			//}

			//string path = Path.GetDirectoryName(fileName) ?? Directory.GetCurrentDirectory();
		}
	}
}

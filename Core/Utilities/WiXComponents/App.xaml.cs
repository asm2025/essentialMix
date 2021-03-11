using System;
using System.Reflection;
using System.Threading.Tasks;
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
			ConsoleHelper.AttachConsole();
			if (!ConsoleHelper.HasConsole) throw new Exception("Could not create console window.");
			ConsoleHelper.Show();

			// Setup and command line processing
			JsonConvert.DefaultSettings = () => JsonHelper.CreateSettings();
			string basePath = Assembly.GetExecutingAssembly().GetDirectoryPath();
			Parser parser = null;

			try
			{
				// Configuration
				_configuration = IConfigurationBuilderHelper.CreateConfiguration(basePath)
															.AddConfigurationFiles(basePath, EnvironmentHelper.GetEnvironmentName())
															.AddEnvironmentVariables()
															.AddUserSecrets()
															.AddArguments(e.Args)
															.Build();


				// Logging:1
				bool logConfigurationRead = false;
				LoggerConfiguration loggerConfiguration = new();

				if (_configuration.GetValue<bool>("Logging:Enabled"))
				{
					loggerConfiguration.ReadFrom.Configuration(_configuration);
					logConfigurationRead = true;
				}

				// Command line
				parser = new Parser(settings =>
				{
					settings.CaseSensitive = false;
					settings.CaseInsensitiveEnumValues = true;
					settings.HelpWriter = Console.Error;
				});

				ParserResult<StartupArguments> result = parser.ParseArguments<StartupArguments>(e.Args);

				if (result.Tag == ParserResultType.NotParsed)
				{
					Shutdown();
					return;
				}

				StartupArguments args = ((Parsed<StartupArguments>)result).Value;

				// Logging:2
				if (args.LogLevel > LogLevel.None)
				{
					if (!logConfigurationRead)
					{
						loggerConfiguration.ReadFrom.Configuration(_configuration);
						logConfigurationRead = true;
					}

					LoggingLevelSwitch levelSwitch = new()
					{
						MinimumLevel = args.LogLevel switch
						{
							LogLevel.Trace => LogEventLevel.Verbose,
							LogLevel.Debug => LogEventLevel.Debug,
							LogLevel.Information => LogEventLevel.Information,
							LogLevel.Warning => LogEventLevel.Warning,
							LogLevel.Error => LogEventLevel.Error,
							_ => LogEventLevel.Fatal
						}
					};
					loggerConfiguration.MinimumLevel.ControlledBy(levelSwitch);
				}

				if (logConfigurationRead) Log.Logger = loggerConfiguration.CreateLogger();

				// Services
				ConfigureServices(args);

				// Command line
				if (args.Files.Count > 0 || !string.IsNullOrEmpty(args.Directory))
				{
					Task.Run(ProcessCommandLine);
					return;
				}

				// Start the main window
				ConsoleHelper.Hide();
				base.OnStartup(e);
				Task.Run(Start);
			}
			catch (Exception ex)
			{
				// This won't produce until a logger is setup in the above try block
				Log.Error(ex.CollectMessages());
			}
			finally
			{
				ObjectHelper.Dispose(ref parser);
				ConsoleHelper.FreeConsole();
				Shutdown();
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
			services.AddLogging(builder => builder.AddSerilog(null, true));
			services.AddTransient<MainView>();
			ServiceProvider = services.BuildServiceProvider();
		}

		[NotNull]
		private Task Start()
		{
			MainView window = ServiceProvider.GetRequiredService<MainView>();
			window.Show();
			return Task.CompletedTask;
		}

		[NotNull]
		private Task ProcessCommandLine()
		{
			// When we are here, args either has files or a directory to be processed.
			StartupArguments args = ServiceProvider.GetRequiredService<StartupArguments>();

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
			return Task.CompletedTask;
		}
	}
}

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
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using WiXComponents.Views;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WiXComponents
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <inheritdoc />
		public App() 
		{
			Writer = new LoggerConfiguration()
					.MinimumLevel.Is(LogEventLevel.Verbose)
					.Enrich.FromLogContext()
					.WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}", theme: SystemConsoleTheme.Literate)
					.CreateLogger();
		}

		[NotNull]
		public Serilog.ILogger Writer { get; }

		public IServiceProvider ServiceProvider { get; private set; }

		public ILogger Logger { get; private set; }

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			// Setup
			JsonConvert.DefaultSettings = () => JsonHelper.CreateSettings();
			string basePath = Assembly.GetExecutingAssembly().GetDirectoryPath();

			ConsoleHelper.AttachConsole(out bool consoleCreated);
			
			if (ConsoleHelper.HasConsole)
			{
				ConsoleHelper.Show();
			}
			else if (e.Args.Length > 0)
			{
				Writer.Fatal("Could not create Writer window.");
				Shutdown();
				return;
			}

			// Configuration
			IConfiguration configuration = IConfigurationBuilderHelper.CreateConfiguration(basePath)
																	.AddConfigurationFiles(basePath, EnvironmentHelper.GetEnvironmentName())
																	.AddEnvironmentVariables()
																	.AddUserSecrets()
																	.AddArguments(e.Args)
																	.Build();

			Parser parser = null;
			StartupArguments args;

			try
			{
				// Command line
				parser = new Parser(settings =>
				{
					settings.CaseSensitive = false;
					settings.CaseInsensitiveEnumValues = true;
				});

				ParserResult<StartupArguments> result = parser.ParseArguments<StartupArguments>(e.Args);

				if (result.Tag == ParserResultType.NotParsed)
				{
					string usage = StartupArguments.GetUsage(result);
					Writer.Error(usage);
					Shutdown();
					return;
				}

				args = ((Parsed<StartupArguments>)result).Value;
			}
			catch (Exception ex)
			{
				Writer.Error(ex.CollectMessages());
				Shutdown();
				return;
			}
			finally
			{
				// in case an error occurred and the parser was not disposed
				ObjectHelper.Dispose(ref parser);
			}

			// Logging
			if (configuration.GetValue<bool>("Logging:Enabled") && args.LogLevel > LogLevel.None)
			{
				LoggerConfiguration loggerConfiguration = new ();
				loggerConfiguration.ReadFrom.Configuration(configuration);
				loggerConfiguration.MinimumLevel.Is(args.LogLevel switch
				{
					LogLevel.Trace => LogEventLevel.Verbose,
					LogLevel.Debug => LogEventLevel.Debug,
					LogLevel.Information => LogEventLevel.Information,
					LogLevel.Warning => LogEventLevel.Warning,
					LogLevel.Error => LogEventLevel.Error,
					_ => LogEventLevel.Fatal
				});
				Log.Logger = loggerConfiguration.CreateLogger();
			}

			// Services
			ConfigureServices(args, configuration);
			Logger = ServiceProvider.GetService<ILogger<App>>();

			if (args.Files.Count > 0 || !string.IsNullOrEmpty(args.Directory))
			{
				ProcessCommandLine(args);
				if (consoleCreated) ConsoleHelper.FreeConsole();
				Shutdown();
				return;
			}

			ConsoleHelper.Hide();
			if (consoleCreated) ConsoleHelper.FreeConsole();
			base.OnStartup(e);
			Start();
		}

		/// <inheritdoc />
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Log.CloseAndFlush();
		}

		private void ConfigureServices([NotNull] StartupArguments startupArguments, [NotNull] IConfiguration configuration)
		{
			ServiceCollection services = new();
			services.AddSingleton(startupArguments);
			services.AddSingleton(configuration);
			services.AddLogging(builder =>
			{
				// clear the default providers
				builder.ClearProviders();
				builder.AddConfiguration(configuration.GetSection("logging"));
				builder.AddDebug();
				builder.AddEventSourceLogger();
				builder.AddSerilog(null, true);
			});
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
			IEnumerable<string> files;

			if (string.IsNullOrEmpty(args.Directory))
			{
				files = args.Files;
			}
			else
			{
				if (!Directory.Exists(args.Directory))
				{
					Writer.Fatal("Path not found.");
					Logger?.LogError("Path not found.");
					return;
				}
				
				files = DirectoryHelper.EnumerateFiles(args.Directory, "*.wxs;*.wsi;*.xml", args.IncludeSubDirectory
																						? SearchOption.AllDirectories
																						: SearchOption.TopDirectoryOnly);
			}

			// wixproj, csproj, vbproj, wxs, wsi, xml
			foreach (string file in files)
			{
				Writer.Verbose(file);
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

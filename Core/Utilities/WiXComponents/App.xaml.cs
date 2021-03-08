using System;
using System.Reflection;
using System.Text;
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
		public IConfiguration Configuration { get; private set; }
		public IServiceProvider ServiceProvider { get; private set; }

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			JsonConvert.DefaultSettings = () => JsonHelper.CreateSettings();
			ConsoleHelper.AttachConsole();
			if (!ConsoleHelper.HasConsole) throw new Exception("Could not create console window.");
			Console.OutputEncoding = Encoding.UTF8;
			ConsoleHelper.Show();

			string basePath = Assembly.GetExecutingAssembly().GetDirectoryPath();
			Parser parser = null;

			try
			{
				// Configuration
				Configuration = IConfigurationBuilderHelper.CreateConfiguration(basePath)
															.AddConfigurationFiles(basePath, EnvironmentHelper.GetEnvironmentName())
															.AddEnvironmentVariables()
															.AddUserSecrets()
															.AddArguments(e.Args)
															.Build();


				// Logging:1
				bool logConfigurationRead = false;
				LoggerConfiguration loggerConfiguration = new();

				if (Configuration.GetValue<bool>("Logging:Enabled"))
				{
					loggerConfiguration.ReadFrom.Configuration(Configuration);
					logConfigurationRead = true;
				}

				// Command line
				parser = new Parser(settings =>
				{
					settings.CaseSensitive = false;
					settings.CaseInsensitiveEnumValues = true;
					settings.HelpWriter = Console.Error;
				});

				ParserResult<Options> result = parser.ParseArguments<Options>(e.Args);

				if (result.Tag == ParserResultType.NotParsed)
				{
					Shutdown();
					return;
				}

				Options options = ((Parsed<Options>)result).Value;

				// Logging:2
				if (options.LogLevel > LogLevel.None)
				{
					if (!logConfigurationRead)
					{
						loggerConfiguration.ReadFrom.Configuration(Configuration);
						logConfigurationRead = true;
					}

					LoggingLevelSwitch levelSwitch = new()
					{
						MinimumLevel = options.LogLevel switch
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
				ServiceCollection services = new();
				ConfigureServices(services, options);
				ServiceProvider = services.BuildServiceProvider();

				if (options.Files.Count > 0)
				{
					// Command line mode
					RunGuidReplacer();
					return;
				}

				ConsoleHelper.Hide();
				base.OnStartup(e);
				RunGui();
			}
			catch (Exception ex)
			{
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

		private void RunGui()
		{
			MainView window = ServiceProvider.GetService<MainView>() ?? throw new InvalidOperationException("Invalid configuration.");
			window.Show();
		}

		private void RunGuidReplacer()
		{
			Console.WriteLine("Hello world!");
		}

		private void ConfigureServices([NotNull] ServiceCollection services, [NotNull] Options options)
		{
			services.AddSingleton(options);
			services.AddLogging(builder => builder.AddSerilog(dispose: true));
			services.AddTransient<MainView>();
		}
	}
}

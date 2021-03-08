using System;
using System.Diagnostics;
using System.IO;
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
using WiXComponents.Properties;
using WiXComponents.Views;

namespace WiXComponents
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			JsonConvert.DefaultSettings = () => JsonHelper.CreateSettings();
			Directory.SetCurrentDirectory(Assembly.GetExecutingAssembly().GetDirectoryPath());
			
			// Command line args
			Parser parser = null;
			Options options;
			
			try
			{
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

				options = ((Parsed<Options>)result).Value;
			}
			finally
			{
				ObjectHelper.Dispose(ref parser);
			}

			// Logging
			if (Settings.Default.Logging)
			{
				LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
				loggerConfiguration.ReadFrom.Configuration(configuration);

				if (options.LogLevel > LogLevel.None)
				{
					LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch
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

				Log.Logger = loggerConfiguration.CreateLogger();
			}

			// Services
			ServiceCollection services = new ServiceCollection();
			ConfigureServices(services, options);
			ServiceProvider = services.BuildServiceProvider();

			if (options.Files.Count > 0)
			{
				// Command line mode
				ConsoleHelper.AttachConsole(out bool consoleCreated);
				if (!ConsoleHelper.HasConsole) throw new Exception("Could not create console window.");
				ConsoleHelper.Show();
				Console.OutputEncoding = Encoding.UTF8;

				try
				{
					RunGuidReplacer();
				}
				catch (Exception ex)
				{
					Log.Logger.Error(ex.CollectMessages());
				}
				finally
				{
					if (consoleCreated) ConsoleHelper.FreeConsole();
				}

				Shutdown();
				return;
			}

			base.OnStartup(e);
			RunGui();
		}

		public IServiceProvider ServiceProvider { get; private set; }

		private void RunGui()
		{
			Views.Main window = ServiceProvider.GetService<Main>();
			Debug.Assert(window != null, nameof(window) + " != null");
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
			services.AddSingleton<Main>();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml;
using asm.Data.Helpers;
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
using WiXComponents.Properties;
using WiXComponents.Views;

namespace WiXComponents
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly Lazy<IReadOnlySet<string>> __supportedProjects = new Lazy<IReadOnlySet<string>>(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".WiXProj",
			".CSProj",
			".VBProj",
		}, LazyThreadSafetyMode.PublicationOnly);

		private static readonly Lazy<IReadOnlySet<string>> __supportedFiles = new Lazy<IReadOnlySet<string>>(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".wxs",
			".wsi",
			".xml",
		}, LazyThreadSafetyMode.PublicationOnly);

		/// <inheritdoc />
		public App()
		{
			Serilog.ILogger serilogLogger = new LoggerConfiguration()
											.MinimumLevel.Is(LogEventLevel.Verbose)
											.Enrich.FromLogContext()
											.WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}", theme: SystemConsoleTheme.Literate, applyThemeToRedirectedOutput: true)
											.CreateLogger();
			ILoggerFactory factory = LoggerFactory.Create(builder =>
			{
				builder.ClearProviders();
				builder.AddSerilog(serilogLogger, true);
			});
			Logger = new Logger<App>(factory.CreateLogger(nameof(App)));
		}


		public IServiceProvider ServiceProvider { get; private set; }

		[NotNull]
		public Logger<App> Logger { get; }

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
				Logger.LogError("Could not create Writer window.");
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
					Logger.LogError(StartupArguments.GetUsage(result));
					Shutdown();
					return;
				}

				args = ((Parsed<StartupArguments>)result).Value;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
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
				LoggerConfiguration loggerConfiguration = new();
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
			Logger._logger = ServiceProvider.GetService<ILogger<App>>();

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
			if (!string.IsNullOrEmpty(args.Directory))
			{
				if (!Directory.Exists(args.Directory))
				{
					Logger.LogError(Errors.DirectoryNotFound, args.Directory);
					return;
				}

				IEnumerable<string> files = DirectoryHelper.EnumerateFiles(args.Directory, "*.wxs;*.wsi;*.xml", args.IncludeSubDirectory
																													? SearchOption.AllDirectories
																													: SearchOption.TopDirectoryOnly);
				foreach (string fileName in files)
				{
					ProcessWiXFile(fileName);
				}

				return;
			}

			foreach (string fileName in args.Files)
			{
				string ext = PathHelper.Extension(fileName);
				if (ext == null) continue;
				if (__supportedProjects.Value.Contains(ext)) ProcessProjectFile(fileName);
				if (__supportedFiles.Value.Contains(ext)) ProcessWiXFile(fileName);
			}
		}

		private void ProcessProjectFile([NotNull] string fileName)
		{
			// wixproj, csproj, vbproj
			if (!File.Exists(fileName))
			{
				Logger.LogError(Errors.FileNotFound, fileName);
				return;
			}

			string path = Path.GetDirectoryName(fileName) ?? Directory.GetCurrentDirectory();

			try
			{
				XmlDocument doc = XmlDocumentHelper.LoadFile(fileName);
				XmlNode root = doc.DocumentElement;
				if (root == null) return;

				XmlNamespaceManager manager = doc.GetNamespaceManager();
				string prefix = manager.GetDefaultPrefix();

				// Finds all of the files included in the project.
				XmlNodeList nodes = doc.SelectNodes($"//{prefix}Compile", manager);
				
				if (nodes != null && nodes.Count > 0)
				{
					foreach (XmlNode node in nodes)
					{
						if (node.HasAttributeOrChild("Link")) continue;
						string itemName = node.Attributes?["Include"]?.Value;
						if (string.IsNullOrEmpty(itemName)) continue;

						string ext = PathHelper.Extension(itemName);
						if (ext == null || !__supportedFiles.Value.Contains(ext)) continue;
						ProcessWiXFile(Path.Combine(path, itemName));
					}
				}

				nodes = doc.SelectNodes($"//{prefix}Content", manager);
				if (nodes == null || nodes.Count == 0) return;
				
				foreach (XmlNode node in nodes)
				{
					if (node.HasAttributeOrChild("Link")) continue;
					string itemName = node.Attributes?["Include"]?.Value;
					if (string.IsNullOrEmpty(itemName)) continue;

					string ext = PathHelper.Extension(itemName);
					if (ext == null || !__supportedFiles.Value.Contains(ext)) continue;
					ProcessWiXFile(Path.Combine(path, itemName));
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
			}
		}

		private void ProcessWiXFile([NotNull] string fileName)
		{
			// wxs, wsi, xml
			if (!File.Exists(fileName))
			{
				Logger.LogError(Errors.FileNotFound, fileName);
				return;
			}

			try
			{
				// This will only update files that aren't readonly
				if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) return;
				
				bool modified = false;
				XmlDocument doc = new()
				{
					PreserveWhitespace = true
				};
				doc.Load(fileName);
		
				foreach (XmlElement element in doc.GetElementsByTagName("Component").Cast<XmlElement>())
				{
					element.SetAttribute("Guid", Guid.NewGuid().ToString("B").ToUpperInvariant());
					modified = true;
				}
		
				if (!modified) return;
				doc.Save(fileName);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
			}
		}
	}
}

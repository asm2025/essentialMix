using System;
using asm.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class IHostBuilderExtension
	{
		[NotNull]
		public static IHostBuilder Setup([NotNull] this IHostBuilder thisValue) { return Setup(thisValue, null, null); }
		[NotNull]
		public static IHostBuilder Setup([NotNull] this IHostBuilder thisValue, Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate) { return Setup(thisValue, null, configureDelegate); }
		[NotNull]
		public static IHostBuilder Setup([NotNull] this IHostBuilder thisValue, Action<IWebHostBuilder> configureHost) { return Setup(thisValue, configureHost, null); }
		[NotNull]
		public static IHostBuilder Setup([NotNull] this IHostBuilder thisValue, Action<IWebHostBuilder> configureHost, Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			return thisValue.ConfigureWebHostDefaults(builder =>
			{
				builder.UseContentRoot(AssemblyHelper.GetEntryAssembly().GetDirectoryPath())
						.ConfigureAppConfiguration((context, configurationBuilder) =>
						{
							configurationBuilder.Setup(context.HostingEnvironment)
												.AddConfigurationFiles((IHostEnvironment)context.HostingEnvironment)
												.AddEnvironmentVariables()
												.AddUserSecrets();
							configureDelegate?.Invoke(context, configurationBuilder);
						});
				configureHost?.Invoke(builder);
			});
		}
	}
}
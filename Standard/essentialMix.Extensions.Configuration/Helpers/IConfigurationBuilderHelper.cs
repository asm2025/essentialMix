using System;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace asm.Helpers;

public static class IConfigurationBuilderHelper
{
	[NotNull]
	public static IConfigurationBuilder CreateConfiguration(string baseDirectory = null)
	{
		baseDirectory = PathHelper.Trim(baseDirectory) ?? AppDomain.CurrentDomain.BaseDirectory;
		IHostEnvironment env = new HostingEnvironment
		{
			EnvironmentName = EnvironmentHelper.GetEnvironmentName(),
			ApplicationName = AppDomain.CurrentDomain.FriendlyName,
			ContentRootPath = baseDirectory,
			ContentRootFileProvider = new PhysicalFileProvider(baseDirectory)
		};
		return new ConfigurationBuilder().Setup(env);
	}
}
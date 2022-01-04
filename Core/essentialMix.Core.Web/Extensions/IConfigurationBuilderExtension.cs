using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class IConfigurationBuilderExtension
{
	/// <summary>
	/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostingenvironment?view=aspnetcore-2.2
	/// https://stackoverflow.com/questions/55601958/ihostingenvironment-is-obsolete
	/// </summary>
	[NotNull]
	public static IConfigurationBuilder Setup([NotNull] this IConfigurationBuilder thisValue, [NotNull] IWebHostEnvironment environment)
	{
		return thisValue.Setup(environment.ContentRootPath);
	}

	[NotNull]
	public static IConfigurationBuilder AddConfigurationFiles([NotNull] this IConfigurationBuilder thisValue, [NotNull] IWebHostEnvironment environment)
	{
		return thisValue.AddConfigurationFiles(environment.ContentRootPath, environment.EnvironmentName);
	}

	[NotNull]
	public static IConfigurationBuilder AddConfigurationFile([NotNull] this IConfigurationBuilder thisValue, [NotNull] string fileName, bool optional, [NotNull] IWebHostEnvironment environment)
	{
		return thisValue.AddConfigurationFile(environment.ContentRootPath, fileName, optional, environment.EnvironmentName);
	}
}
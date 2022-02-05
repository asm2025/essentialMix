using System;
using essentialMix.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class LogFactoryExtension
{
	[NotNull]
	public static ILoggingBuilder AddColoredConsoleFormatter([NotNull] this ILoggingBuilder thisValue)
	{
		return thisValue.AddConsoleFormatter<ColoredConsoleFormatter, ColoredConsoleFormatterOptions>();
	}

	[NotNull]
	public static ILoggingBuilder AddColoredConsoleFormatter([NotNull] this ILoggingBuilder thisValue, [NotNull] Action<ColoredConsoleFormatterOptions> configure)
	{
		return thisValue.AddConsoleFormatter<ColoredConsoleFormatter, ColoredConsoleFormatterOptions>(configure);
	}
}
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class LogLevelExtension
{
	[NotNull]
	public static string String(this LogLevel thisValue)
	{
		return thisValue switch
		{
			LogLevel.Trace => "trce",
			LogLevel.Debug => "dbug",
			LogLevel.Information or LogLevel.None => "info",
			LogLevel.Warning => "warn",
			LogLevel.Error => "fail",
			LogLevel.Critical => "crit",
			_ => throw new ArgumentOutOfRangeException(nameof(thisValue))
		};
	}
}
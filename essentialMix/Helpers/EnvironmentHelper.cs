using System;
using System.Text.RegularExpressions;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class EnvironmentHelper
{
	private static readonly Regex __rgxIsEnvVar = new Regex("^%\\w+%$", RegexHelper.OPTIONS_I);
	private static readonly Regex __rgxHasEnvVar = new Regex("%\\w+%", RegexHelper.OPTIONS_I);

	public static bool IsVar(string value) { return !string.IsNullOrEmpty(value) && __rgxIsEnvVar.IsMatch(value); }

	public static bool HasVar(string value) { return !string.IsNullOrEmpty(value) && __rgxHasEnvVar.IsMatch(value); }
	[NotNull]
	public static string GetEnvironmentName()
	{
		return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToNullIfEmpty() ??
				Environment.GetEnvironmentVariable("APPNETCORE_ENVIRONMENT").ToNullIfEmpty() ??
				Environment.GetEnvironmentVariable("ENVIRONMENT").ToNullIfEmpty() ??
				"Development";
	}
}
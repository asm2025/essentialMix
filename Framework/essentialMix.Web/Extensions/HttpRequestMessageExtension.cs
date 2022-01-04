using System;
using System.Net.Http;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class HttpRequestMessageExtension
{
	public static bool IsLocalUrl([NotNull] this HttpRequestMessage thisValue)
	{
		object isLocal = thisValue.Properties["MS_IsLocal"];
		return isLocal as bool? == true || isLocal is Lazy<bool> { Value: true };
	}
}
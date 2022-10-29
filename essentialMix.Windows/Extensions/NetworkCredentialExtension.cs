using System.Net;
using essentialMix.Windows;
using essentialMix.Windows.Helpers;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class NetworkCredentialExtension
{
	[NotNull]
	public static SafeAccessTokenHandle Impersonate([NotNull] this NetworkCredential thisValue, LogonType logonType)
	{
		return ImpersonationHelper.Impersonate(thisValue, logonType);
	}
}
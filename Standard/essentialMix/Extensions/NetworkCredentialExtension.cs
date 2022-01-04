using System.Net;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace essentialMix.Extensions;

public static class NetworkCredentialExtension
{
	[NotNull]
	public static SafeAccessTokenHandle Impersonate([NotNull] this NetworkCredential thisValue, LogonType logonType)
	{
		return ImpersonationHelper.Impersonate(thisValue, logonType);
	}
}
using System.Net;
using asm.Helpers;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace asm.Extensions
{
	public static class NetworkCredentialExtension
	{
		[NotNull]
		public static SafeAccessTokenHandle Impersonate([NotNull] this NetworkCredential thisValue, LogonType logonType)
		{
			return ImpersonationHelper.Impersonate(thisValue, logonType);
		}
	}
}
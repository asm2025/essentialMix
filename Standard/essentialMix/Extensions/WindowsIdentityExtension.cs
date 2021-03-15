using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class WindowsIdentityExtension
	{
		public static bool HasElevatedPrivileges([NotNull] this WindowsIdentity thisValue)
		{
			WindowsPrincipal principal = new WindowsPrincipal(thisValue);

			/*
			 * Check if this user has the Administrator role. If they do, return immediately.
			 * If UAC is on, and the process is not elevated, then this will actually return false.
			 */
			if (principal.IsInRole(WindowsBuiltInRole.Administrator)) return true;

			// If we're not running in Vista onwards, we don't have to worry about checking for UAC.
			if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6)
			{
				// Operating system does not support UAC; skipping elevation check.
				return false;
			}

			int tokenInfLength = Marshal.SizeOf(typeof(int));
			IntPtr tokenInformation = Marshal.AllocHGlobal(tokenInfLength);

			try
			{
				if (!Win32.GetTokenInformation(thisValue.Token, TokenInformationClass.TokenElevationType, tokenInformation, tokenInfLength, out tokenInfLength))
					throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());

				TokenElevationType elevationType = (TokenElevationType)Marshal.ReadInt32(tokenInformation);
				return elevationType == TokenElevationType.TokenElevationTypeFull;
			}
			finally
			{
				if (tokenInformation != IntPtr.Zero) Marshal.FreeHGlobal(tokenInformation);
			}
		}
	}
}
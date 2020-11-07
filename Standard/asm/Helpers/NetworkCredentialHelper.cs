using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using asm.Exceptions.Security;
using asm.Security;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace asm.Helpers
{
	// https://github.com/mj1856/SimpleImpersonation/blob/master/src/Impersonation.cs
	public static class NetworkCredentialHelper
	{
		[NotNull]
		public static SafeAccessTokenHandle Impersonate([NotNull] NetworkCredential credential, LogonType logonType)
		{
			return credential.SecurePassword == null
						? Impersonate(credential.UserName, credential.Password, credential.Domain, logonType)
						: Impersonate(credential.UserName, credential.SecurePassword, credential.Domain, logonType);
		}

		[NotNull]
		public static SafeAccessTokenHandle Impersonate([NotNull] string userName, [NotNull] string password, string domain, LogonType logonType)
		{
			if (Win32.LogonUser(userName, domain, password, (int)logonType, 0, out IntPtr tokenHandle)) return new SafeAccessTokenHandle(tokenHandle);
			if (tokenHandle != IntPtr.Zero) Win32.CloseHandle(tokenHandle);
			throw new ImpersonationException(new Win32Exception(Marshal.GetLastWin32Error()));
		}

		[NotNull]
		public static SafeAccessTokenHandle Impersonate([NotNull] string userName, [NotNull] SecureString password, string domain, LogonType logonType)
		{
			IntPtr passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(password);

			try
			{
				if (Win32.LogonUser(userName, domain, passwordPtr, (int)logonType, 0, out IntPtr tokenHandle)) return new SafeAccessTokenHandle(tokenHandle);
				if (tokenHandle != IntPtr.Zero) Win32.CloseHandle(tokenHandle);
				throw new ImpersonationException(new Win32Exception(Marshal.GetLastWin32Error()));
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
			}
		}

		public static void RunAsUser([NotNull] NetworkCredential credential, LogonType logonType, [NotNull] Action action)
		{
			SafeAccessTokenHandle tokenHandle = null;

			try
			{
				tokenHandle = Impersonate(credential, logonType);
				RunImpersonated(tokenHandle, _ => action());
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenHandle);
			}
		}

		public static void RunAsUser([NotNull] NetworkCredential credential, LogonType logonType, [NotNull] Action<SafeAccessTokenHandle> action)
		{
			SafeAccessTokenHandle tokenHandle = null;

			try
			{
				tokenHandle = Impersonate(credential, logonType);
				RunImpersonated(tokenHandle, action);
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenHandle);
			}
		}

		public static T RunAsUser<T>([NotNull] NetworkCredential credential, LogonType logonType, [NotNull] Func<T> action)
		{
			SafeAccessTokenHandle tokenHandle = null;

			try
			{
				tokenHandle = Impersonate(credential, logonType);
				return RunImpersonated(tokenHandle, _ => action());
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenHandle);
			}
		}

		public static T RunAsUser<T>([NotNull] NetworkCredential credential, LogonType logonType, [NotNull] Func<SafeAccessTokenHandle, T> action)
		{
			SafeAccessTokenHandle tokenHandle = null;

			try
			{
				tokenHandle = Impersonate(credential, logonType);
				return RunImpersonated(tokenHandle, action);
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenHandle);
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static void RunImpersonated(SafeAccessTokenHandle tokenHandle, [NotNull] Action<SafeAccessTokenHandle> action)
		{
			WindowsIdentity.RunImpersonated(tokenHandle, () => action(tokenHandle));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static T RunImpersonated<T>(SafeAccessTokenHandle tokenHandle, [NotNull] Func<SafeAccessTokenHandle, T> function)
		{
			return WindowsIdentity.RunImpersonated(tokenHandle, () => function(tokenHandle));
		}
	}
}
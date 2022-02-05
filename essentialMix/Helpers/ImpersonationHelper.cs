using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace essentialMix.Helpers;

// https://github.com/mj1856/SimpleImpersonation/blob/master/src/Impersonation.cs
public static class ImpersonationHelper
{
	[NotNull]
	public static SafeAccessTokenHandle Impersonate([NotNull] NetworkCredential credential, LogonType logonType)
	{
		return credential.SecurePassword == null
					? Impersonate(credential.UserName, credential.Password, credential.Domain, logonType)
					: Impersonate(credential.UserName, credential.SecurePassword, credential.Domain, logonType);
	}

	[NotNull]
	public static SafeAccessTokenHandle Impersonate([NotNull] string userName, [NotNull] string password, LogonType logonType) { return Impersonate(userName, password, null, logonType); }

	[NotNull]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static SafeAccessTokenHandle Impersonate([NotNull] string userName, [NotNull] string password, string domain, LogonType logonType)
	{
		if (Win32.LogonUser(userName, domain, password, logonType, 0, out IntPtr tokenHandle)) return new SafeAccessTokenHandle(tokenHandle);
		if (tokenHandle != IntPtr.Zero) Win32.CloseHandle(tokenHandle);
		throw new Win32Exception(Marshal.GetLastWin32Error());
	}

	[NotNull]
	public static SafeAccessTokenHandle Impersonate([NotNull] string userName, [NotNull] SecureString password, LogonType logonType) { return Impersonate(userName, password, null, logonType); }

	[NotNull]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static SafeAccessTokenHandle Impersonate([NotNull] string userName, [NotNull] SecureString password, string domain, LogonType logonType)
	{
		IntPtr passwordPtr = Marshal.SecureStringToGlobalAllocUnicode(password);
			
		try
		{
			if (Win32.LogonUser(userName, domain, passwordPtr, logonType, 0, out IntPtr tokenHandle)) return new SafeAccessTokenHandle(tokenHandle);
			if (tokenHandle != IntPtr.Zero) Win32.CloseHandle(tokenHandle);
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
		}
	}

	public static void RunWithCredential([NotNull] NetworkCredential credential, LogonType logonType, [NotNull] Action action)
	{
		if (credential.SecurePassword == null)
			RunWithCredential(credential.UserName, credential.Password, credential.Domain, logonType, action);
		else
			RunWithCredential(credential.UserName, credential.SecurePassword, credential.Domain, logonType, action);
	}

	public static void RunWithCredential([NotNull] string userName, [NotNull] string password, LogonType logonType, [NotNull] Action action) { RunWithCredential(userName, password, null, logonType, action); }
		
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static void RunWithCredential([NotNull] string userName, [NotNull] string password, string domain, LogonType logonType, [NotNull] Action action)
	{
		SafeAccessTokenHandle tokenHandle = Impersonate(userName, password, domain, logonType);
		WindowsIdentity.RunImpersonated(tokenHandle, () =>
		{
			try
			{
				action();
			}
			finally
			{
				Win32.RevertToSelf();
				ObjectHelper.Dispose(ref tokenHandle);
			}
		});
	}

	public static void RunWithCredential([NotNull] string userName, [NotNull] SecureString password, LogonType logonType, [NotNull] Action action) { RunWithCredential(userName, password, null, logonType, action); }
	
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static void RunWithCredential([NotNull] string userName, [NotNull] SecureString password, string domain, LogonType logonType, [NotNull] Action action)
	{
		SafeAccessTokenHandle tokenHandle = Impersonate(userName, password, domain, logonType);
		WindowsIdentity.RunImpersonated(tokenHandle, () =>
		{
			try
			{
				action();
			}
			finally
			{
				Win32.RevertToSelf();
				ObjectHelper.Dispose(ref tokenHandle);
			}
		});
	}

	public static T RunWithCredential<T>([NotNull] NetworkCredential credential, LogonType logonType, [NotNull] Func<T> action)
	{
		return credential.SecurePassword == null
					? RunWithCredential(credential.UserName, credential.Password, credential.Domain, logonType, action)
					: RunWithCredential(credential.UserName, credential.SecurePassword, credential.Domain, logonType, action);
	}

	public static T RunWithCredential<T>([NotNull] string userName, [NotNull] string password, LogonType logonType, [NotNull] Func<T> action) { return RunWithCredential(userName, password, null, logonType, action); }
		
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static T RunWithCredential<T>([NotNull] string userName, [NotNull] string password, string domain, LogonType logonType, [NotNull] Func<T> action)
	{
		SafeAccessTokenHandle tokenHandle = Impersonate(userName, password, domain, logonType);
		return WindowsIdentity.RunImpersonated(tokenHandle, () =>
		{
			try
			{
				return action();
			}
			finally
			{
				Win32.RevertToSelf();
				ObjectHelper.Dispose(ref tokenHandle);
			}
		});
	}

	public static T RunWithCredential<T>([NotNull] string userName, [NotNull] SecureString password, LogonType logonType, [NotNull] Func<T> action) { return RunWithCredential(userName, password, null, logonType, action); }
		
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static T RunWithCredential<T>([NotNull] string userName, [NotNull] SecureString password, string domain, LogonType logonType, [NotNull] Func<T> action)
	{
		SafeAccessTokenHandle tokenHandle = Impersonate(userName, password, domain, logonType);
		return WindowsIdentity.RunImpersonated(tokenHandle, () =>
		{
			try
			{
				return action();
			}
			finally
			{
				Win32.RevertToSelf();
				ObjectHelper.Dispose(ref tokenHandle);
			}
		});
	}

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static void RunWithElevatedPrivilege([NotNull] Action action)
	{
		WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.AllAccess);
		WindowsIdentity.RunImpersonated(identity.AccessToken, () =>
		{
			try { action(); }
			finally { ObjectHelper.Dispose(ref identity); }
		});
	}

	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public static T RunWithElevatedPrivilege<T>([NotNull] Func<T> action)
	{
		WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.AllAccess);
		return WindowsIdentity.RunImpersonated(identity.AccessToken, () =>
		{
			try { return action(); }
			finally { ObjectHelper.Dispose(ref identity); }
		});
	}
}
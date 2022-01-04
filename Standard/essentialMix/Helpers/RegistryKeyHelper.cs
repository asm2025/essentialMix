using System;
using System.Security;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace essentialMix.Helpers;

public static class RegistryKeyHelper
{
	[NotNull]
	[SecuritySafeCritical]
	public static RegistryKey OpenBaseKey(RegistryHive root)
	{
		return RegistryKey.OpenBaseKey(root, Environment.Is64BitOperatingSystem
												? RegistryView.Registry64
												: RegistryView.Registry32);
	}
}
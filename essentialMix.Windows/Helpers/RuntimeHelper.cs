using System;
using System.Linq;
using essentialMix.Extensions;
using essentialMix.Helpers;
using Microsoft.Win32;

namespace essentialMix.Windows.Helpers;

public static class RuntimeHelper
{
	public static System.Version GetInstalledFrameworkVersion()
	{
		// Check first for 4.5 or higher
		RegistryKey rootKey = null;
		RegistryKey ndpKey = null;

		try
		{
			rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
			ndpKey = rootKey.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\");

			if (ndpKey != null)
			{
				int release = ndpKey.GetValue("Release").To(0);

				if (release > 0)
				{
					if (release >= 527934) return new System.Version(4, 8, 0);
					if (release >= 461808) return new System.Version(4, 7, 2);
					if (release >= 461308) return new System.Version(4, 7, 1);
					if (release >= 460798) return new System.Version(4, 7, 0);
					if (release >= 394802) return new System.Version(4, 6, 2);
					if (release >= 394254) return new System.Version(4, 6, 1);
					if (release >= 393295) return new System.Version(4, 6, 0);
					if (release >= 379893) return new System.Version(4, 5, 2);
					if (release >= 378675) return new System.Version(4, 5, 1);
					if (release >= 378389) return new System.Version(4, 5, 0);
				}
			}
		}
		finally
		{
			ObjectHelper.Dispose(ref ndpKey);
			ObjectHelper.Dispose(ref rootKey);
		}

		try
		{
			rootKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, string.Empty);
			ndpKey = rootKey.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\");
			if (ndpKey == null) return null;

			foreach (string subKeyName in ndpKey.GetSubKeyNames()
												.Where(e => e.StartsWith("v", StringComparison.InvariantCultureIgnoreCase))
												.Reverse())
			{
				RegistryKey key = null;

				try
				{
					key = ndpKey.OpenSubKey(subKeyName);
					if (key == null || string.Equals((string)key.GetValue(null), "deprecated")) continue;

					string install = (string)key.GetValue("Install", null);
					if (string.IsNullOrEmpty(install) || install != "1") continue;

					string version = (string)key.GetValue("Version", null);
					if (string.IsNullOrEmpty(version)) continue;

					string sp = (string)key.GetValue("SP", null);
					if (!string.IsNullOrEmpty(sp)) version += sp;
					if (System.Version.TryParse(version, out System.Version result)) return result;
				}
				finally
				{
					ObjectHelper.Dispose(ref key);
				}
			}
		}
		finally
		{
			ObjectHelper.Dispose(ref ndpKey);
			ObjectHelper.Dispose(ref rootKey);
		}

		return null;
	}
}
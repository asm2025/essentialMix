using System;
using System.Diagnostics;
using System.IO;
using essentialMix.Helpers;
using Microsoft.Win32;

namespace essentialMix.Windows.Helpers;

public static class IEHelper
{
    private const string IE_KEY = @"Software\Microsoft\Internet Explorer";
    private const string IE_FEATURE_KEY = IE_KEY + @"\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

    public static int GetVersion()
    {
        int result = 0;
        RegistryKey root = null;
        RegistryKey key = null;

        try
        {
            root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            key = root.OpenSubKey(IE_KEY);
            if (key == null) return -1;

            // 1. svcVersion
            int value = GetRegValue(key, "svcVersion");
            if (value > 0) result = value;

            // 2. svcUpdateVersion
            value = GetRegValue(key, "svcUpdateVersion");
            if (value > 0 && value > result) result = value;

            // 3. Version
            value = GetRegValue(key, "Version");
            if (value > 0 && value > result) result = value;

            // 4. W2kVersion
            value = GetRegValue(key, "W2kVersion");
            if (value > 0 && value > result) result = value;
        }
        finally
        {
            ObjectHelper.Dispose(ref key);
            ObjectHelper.Dispose(ref root);
        }

        return result;

        static int GetRegValue(RegistryKey key, string name)
        {
            string value = (string)key.GetValue(name, null);
            if (string.IsNullOrEmpty(value)) return -1;

            int dot = value.IndexOf('.');
            if (dot > 0) value = value.Substring(0, dot);
            return !int.TryParse(value, out int ver)
                        ? -1
                        : ver;
        }
    }

    public static int GetEmbeddedVersion()
    {
        int ver = GetVersion();
        return ver switch
        {
            > 9 => ver * 1000 + 1,
            > 7 => ver * 1111,
            _ => 7000
        };
    }

    public static void UseLatestVersion()
    {
        string path = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(path)) return;
        UseVersion(path);
    }

    public static void UseVersion(string appName, int version = 0)
    {
        appName = appName?.Trim();
        if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException(nameof(appName));

        if (version <= 0)
        {
            version = GetEmbeddedVersion();
            if (version < 7000) return;
        }
        else if (version is < 7000 or > 11001)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        if (appName.IndexOf(Path.DirectorySeparatorChar) > -1) appName = Path.GetFileName(appName);
        SetKey(RegistryHive.LocalMachine, appName, version);
        SetKey(RegistryHive.CurrentUser, appName, version);

        static void SetKey(RegistryHive hive, string appName, int version)
        {
            RegistryKey root = null;
            RegistryKey key = null;

            try
            {
                root = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                key = root.OpenSubKey(IE_FEATURE_KEY, true);
                key?.SetValue(appName, version, RegistryValueKind.DWord);
            }
            finally
            {
                ObjectHelper.Dispose(ref key);
                ObjectHelper.Dispose(ref root);
            }
        }
    }
}
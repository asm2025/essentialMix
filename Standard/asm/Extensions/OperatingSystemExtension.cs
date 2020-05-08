using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class OperatingSystemExtension
	{
		private static readonly HashSet<PlatformID> WINDOWS_PLATFORMS = new HashSet<PlatformID>
		{
			PlatformID.Win32Windows,
			PlatformID.Win32NT,
			PlatformID.WinCE,
			PlatformID.Win32S
		};

		public static bool IsUnix([NotNull] this OperatingSystem thisValue) { return thisValue.Platform == PlatformID.Unix; }

		public static bool IsMacOSX([NotNull] this OperatingSystem thisValue) { return thisValue.Platform == PlatformID.MacOSX; }

		public static bool IsXbox([NotNull] this OperatingSystem thisValue) { return thisValue.Platform == PlatformID.Xbox; }

		public static bool IsWindows([NotNull] this OperatingSystem thisValue) { return WINDOWS_PLATFORMS.Contains(thisValue.Platform); }

		public static bool IsWindows32([NotNull] this OperatingSystem thisValue) { return thisValue.Platform == PlatformID.Win32Windows; }

		public static bool IsWindowsNT([NotNull] this OperatingSystem thisValue) { return thisValue.Platform == PlatformID.Win32NT; }

		public static bool IsWindowsCE([NotNull] this OperatingSystem thisValue) { return thisValue.Platform == PlatformID.WinCE; }

		public static bool IsWindows95OrHigher([NotNull] this OperatingSystem thisValue) { return thisValue.IsWindows32() && thisValue.Version.Major >= 4; }

		public static bool IsWindows98OrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindows32() && (thisValue.Version.Major > 4 || thisValue.Version.Major == 4 && thisValue.Version.Minor >= 10);
		}

		public static bool IsWindowsMEOrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindows32() && (thisValue.Version.Major > 4 || thisValue.Version.Major == 4 && thisValue.Version.Minor >= 90);
		}

		public static bool IsWindowsNTOrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindowsNT() && (thisValue.Version.Major > 4 || thisValue.Version.Major == 4 && thisValue.Version.Minor == 0);
		}

		public static bool IsWindows2KOrHigher([NotNull] this OperatingSystem thisValue) { return thisValue.IsWindowsNT() && thisValue.Version.Major >= 5; }

		public static bool IsWindowsXPOrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindowsNT() && (thisValue.Version.Major > 5 || thisValue.Version.Major == 5 && thisValue.Version.Minor >= 1);
		}

		public static bool IsWindowsVistaOrHigher([NotNull] this OperatingSystem thisValue) { return thisValue.IsWindowsNT() && thisValue.Version.Major >= 6; }

		public static bool IsWindows7OrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindowsNT() && (thisValue.Version.Major > 6 || thisValue.Version.Major == 6 && thisValue.Version.Minor >= 1);
		}

		public static bool IsWindows8OrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindowsNT() && (thisValue.Version.Major > 6 || thisValue.Version.Major == 6 && thisValue.Version.Minor >= 2);
		}

		public static bool IsWindows81OrHigher([NotNull] this OperatingSystem thisValue)
		{
			return thisValue.IsWindowsNT() && (thisValue.Version.Major > 6 || thisValue.Version.Major == 6 && thisValue.Version.Minor >= 3);
		}

		public static bool IsWindows10OrHigher([NotNull] this OperatingSystem thisValue) { return thisValue.IsWindowsNT() && thisValue.Version.Major >= 10; }
	}
}
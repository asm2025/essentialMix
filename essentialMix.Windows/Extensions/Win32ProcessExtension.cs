using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Permissions;
using essentialMix.Helpers;
using essentialMix.Windows;
using essentialMix.Windows.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class Win32ProcessExtension
{
	public static bool IsRunning(this Process thisValue)
	{
		if (!thisValue.IsAwaitable()) return false;

		SystemInfoRequest request = new SystemInfoRequest(SystemInfoType.Win32_Process)
		{
			SelectExpression = "ProcessId",
			Filter = mo => Convert.ToInt32(mo["ProcessId"]) == thisValue.Id
		};

		return SystemInfo.Get(request).Any();
	}

	[NotNull]
	public static IEnumerable<Process> GetChildren(this Process thisValue)
	{
		if (!thisValue.IsAwaitable()) return [];

		Func<ManagementObject, Process> converter = mo => ProcessHelper.TryGetProcessById(Convert.ToInt32(mo["ProcessID"]), out Process p) ? p : null;
		SystemInfoRequest<Process> request = new SystemInfoRequest<Process>(SystemInfoType.Win32_Process, converter)
		{
			WhereExpression = $"ParentProcessID={thisValue.Id}"
		};

		return SystemInfo.Get(request).Where(p => p != null);
	}

	[NotNull]
	public static IEnumerable<Process> GetChildren([NotNull] this Process thisValue, [NotNull] Predicate<Process> predicate)
	{
		if (!thisValue.IsAwaitable()) return [];

		Func<ManagementObject, Process> converter = mo => ProcessHelper.TryGetProcessById(Convert.ToInt32(mo["ProcessID"]), out Process p) ? p : null;
		SystemInfoRequest<Process> request = new SystemInfoRequest<Process>(SystemInfoType.Win32_Process, converter)
		{
			WhereExpression = $"ParentProcessID={thisValue.Id}"
		};

		return SystemInfo.Get(request).Where(p => p != null && predicate(p));
	}

	public static Process GetChild([NotNull] this Process thisValue, [NotNull] Predicate<Process> predicate)
	{
		if (!thisValue.IsAwaitable()) return null;

		Func<ManagementObject, Process> converter = mo => ProcessHelper.TryGetProcessById(Convert.ToInt32(mo["ProcessID"]), out Process p) ? p : null;
		SystemInfoRequest<Process> request = new SystemInfoRequest<Process>(SystemInfoType.Win32_Process, converter)
		{
			WhereExpression = $"ParentProcessID={thisValue.Id}"
		};

		return SystemInfo.Get(request).FirstOrDefault(p => p != null && predicate(p));
	}

	public static bool KillChildren([NotNull] this Process thisValue)
	{
		foreach (Process p in GetChildren(thisValue))
		{
			if (!p.Die()) return false;
		}

		return true;
	}

	public static bool KillChildren([NotNull] this Process thisValue, [NotNull] Predicate<Process> predicate)
	{
		foreach (Process p in GetChildren(thisValue, predicate))
		{
			if (!p.Die()) return false;
		}

		return true;
	}
	public static bool Die(this Process thisValue, int delay = 0)
	{
		if (!thisValue.IsAwaitable()) return true;

		if (delay > 0)
		{
			TimeSpanHelper.WasteTime(delay);
			if (!thisValue.IsAwaitable()) return true;
		}

		try
		{
			if (!KillChildren(thisValue)) return false;
			if (!thisValue.IsAwaitable()) return true;
			thisValue.Kill();
			return true;
		}
		catch
		{
			return false;
		}
	}
	public static void Die([NotNull] this Process[] thisValue)
	{
		foreach (Process process in thisValue)
			Die(process);
	}

	public static string GetFileName(this Process thisValue) { return GetCommandLine(thisValue).ExecutablePath; }

	public static (string ExecutablePath, string CommandLine) GetCommandLine(this Process thisValue)
	{
		if (!thisValue.IsAwaitable()) return (null, null);

		SystemInfoRequest request = new SystemInfoRequest(SystemInfoType.Win32_Process)
		{
			SelectExpression = "ProcessId, ExecutablePath, CommandLine",
			Filter = e => Convert.ToInt32(e["ProcessId"]) == thisValue.Id
		};

		ManagementObject mo = SystemInfo.FirstOrDefault(request);
		return mo == null
					? (null, null)
					: ((string)mo["ExecutablePath"], (string)mo["CommandLine"]);
	}
}
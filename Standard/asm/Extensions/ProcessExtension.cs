using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using asm.Helpers;

namespace asm.Extensions
{
	public static class ProcessExtension
	{
		private static readonly PropertyInfo ASSOCIATED_PROPERTY;

		static ProcessExtension()
		{
			ASSOCIATED_PROPERTY = typeof(Process).GetProperty("Associated", Constants.BF_NON_PUBLIC_INSTANCE, typeof(bool));
		}

		public static bool IsAssociated(this Process thisValue)
		{
			if (thisValue.IsDisposed()) return false;
			thisValue.Refresh();
			return (bool)ASSOCIATED_PROPERTY.GetValue(thisValue);
		}

		public static bool IsAwaitable(this Process thisValue) { return IsAssociated(thisValue) && !thisValue.HasExited; }

		public static bool IsRunning(this Process thisValue)
		{
			if (!IsAwaitable(thisValue)) return false;

			SystemInfoRequest request = new SystemInfoRequest(SystemInfoType.Win32_Process)
			{
				SelectExpression = "ProcessId",
				Filter = mo => Convert.ToInt32(mo["ProcessId"]) == thisValue.Id
			};

			return SystemInfo.Get(request).Any();
		}

		[NotNull]
		public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, CancellationToken token = default(CancellationToken))
		{
			return WaitForExitAsync(thisValue, TimeSpanHelper.INFINITE, false, token);
		}

		[NotNull]
		public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitForExitAsync(thisValue, timeout.TotalIntMilliseconds(), false, token);
		}

		[NotNull]
		public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, TimeSpan timeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			return WaitForExitAsync(thisValue, timeout.TotalIntMilliseconds(), exitContext, token);
		}

		[NotNull]
		public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			return WaitForExitAsync(thisValue, millisecondsTimeout, false, token);
		}

		[NotNull]
		public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, int millisecondsTimeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			if (!IsAwaitable(thisValue)) return Task.FromResult(true);
			token.ThrowIfCancellationRequested();
			return Task.Run(() => WaitForExit(thisValue, millisecondsTimeout, exitContext, token), token).ConfigureAwait();
		}

		public static bool WaitForExit([NotNull] this Process thisValue, CancellationToken token = default(CancellationToken)) { return WaitForExit(thisValue, TimeSpanHelper.INFINITE, false, token); }

		public static bool WaitForExit([NotNull] this Process thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitForExit(thisValue, timeout.TotalIntMilliseconds(), false, token);
		}

		public static bool WaitForExit([NotNull] this Process thisValue, TimeSpan timeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			return WaitForExit(thisValue, timeout.TotalIntMilliseconds(), exitContext, token);
		}

		public static bool WaitForExit([NotNull] this Process thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			return WaitForExit(thisValue, millisecondsTimeout, false, token);
		}

		public static bool WaitForExit([NotNull] this Process thisValue, int millisecondsTimeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			token.ThrowIfCancellationRequested();
			if (!IsAwaitable(thisValue)) return true;

			using (SafeWaitHandle waitHandle = new SafeWaitHandle(thisValue.Handle, false))
			{
				if (!waitHandle.IsAwaitable()) return false;
				
				using (ManualResetEvent processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle })
				{
					return processFinishedEvent.WaitOne(millisecondsTimeout, true, exitContext, token);
				}
			}
		}

		public static bool SpinWaitForExit([NotNull] this Process thisValue, Func<bool> evalFunc) { return SpinWaitForExit(thisValue, TimeSpanHelper.INFINITE, evalFunc); }

		public static bool SpinWaitForExit([NotNull] this Process thisValue, TimeSpan timeout, Func<bool> evalFunc)
		{
			return SpinWaitForExit(thisValue, timeout.TotalIntMilliseconds(), evalFunc);
		}

		public static bool SpinWaitForExit([NotNull] this Process thisValue, int millisecondsTimeout, Func<bool> evalFunc)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsAwaitable(thisValue)) return true;

			Func<bool> predicate;

			if (evalFunc == null)
				predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForExit(TimeSpanHelper.MINIMUM);
			else
				predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForExit(TimeSpanHelper.MINIMUM) || evalFunc();

			return SpinWait.SpinUntil(predicate, millisecondsTimeout);
		}

		public static bool SpinWaitForInputIdle([NotNull] this Process thisValue, Func<bool> evalFunc)
		{
			return SpinWaitForInputIdle(thisValue, TimeSpanHelper.INFINITE, evalFunc);
		}

		public static bool SpinWaitForInputIdle([NotNull] this Process thisValue, TimeSpan timeout, Func<bool> evalFunc)
		{
			return SpinWaitForInputIdle(thisValue, timeout.TotalIntMilliseconds(), evalFunc);
		}

		public static bool SpinWaitForInputIdle([NotNull] this Process thisValue, int millisecondsTimeout, Func<bool> evalFunc)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsAwaitable(thisValue)) return false;

			Func<bool> predicate;

			if (evalFunc == null)
				predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForInputIdle(TimeSpanHelper.MINIMUM_SCHEDULE);
			else
				predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForInputIdle(TimeSpanHelper.MINIMUM_SCHEDULE) || evalFunc();

			return SpinWait.SpinUntil(predicate, millisecondsTimeout);
		}

		public static bool IsResponding([NotNull] this Process thisValue)
		{
			try
			{
				return IsAwaitable(thisValue) && thisValue.Responding;
			}
			catch
			{
				return false;
			}
		}

		[NotNull]
		public static IEnumerable<Process> GetChildren(this Process thisValue)
		{
			if (!IsAwaitable(thisValue)) return Enumerable.Empty<Process>();

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
			if (!IsAwaitable(thisValue)) return Enumerable.Empty<Process>();

			Func<ManagementObject, Process> converter = mo => ProcessHelper.TryGetProcessById(Convert.ToInt32(mo["ProcessID"]), out Process p) ? p : null;
			SystemInfoRequest<Process> request = new SystemInfoRequest<Process>(SystemInfoType.Win32_Process, converter)
			{
				WhereExpression = $"ParentProcessID={thisValue.Id}"
			};

			return SystemInfo.Get(request).Where(p => p != null && predicate(p));
		}

		public static Process GetChild([NotNull] this Process thisValue, [NotNull] Predicate<Process> predicate)
		{
			if (!IsAwaitable(thisValue)) return null;

			Func<ManagementObject, Process> converter = mo => ProcessHelper.TryGetProcessById(Convert.ToInt32(mo["ProcessID"]), out Process p) ? p : null;
			SystemInfoRequest<Process> request = new SystemInfoRequest<Process>(SystemInfoType.Win32_Process, converter)
			{
				WhereExpression = $"ParentProcessID={thisValue.Id}"
			};

			return SystemInfo.Get(request).FirstOrDefault(p => p != null && predicate(p));
		}

		public static bool KillChildren([NotNull] this Process thisValue) { return GetChildren(thisValue).All(p => p.Die()); }

		public static bool KillChildren([NotNull] this Process thisValue, [NotNull] Predicate<Process> predicate)
		{
			return GetChildren(thisValue, predicate).All(p => p.Die());
		}

		[SecurityPermission(SecurityAction.LinkDemand)]
		public static bool Die(this Process thisValue, int delay = 0)
		{
			if (!IsAwaitable(thisValue)) return true;

			if (delay > 0)
			{
				Thread.Sleep(delay);
				if (!IsAwaitable(thisValue)) return true;
			}

			try
			{
				if (!KillChildren(thisValue)) return false;
				if (!IsAwaitable(thisValue)) return true;
				thisValue.Kill();
				return true;
			}
			catch
			{
				return false;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand)]
		public static void Die([NotNull] this Process[] thisValue)
		{
			foreach (Process process in thisValue)
				Die(process);
		}

		public static FileVersionInfo Version([NotNull] this Process thisValue)
		{
			return IsAwaitable(thisValue)
						? thisValue.MainModule?.FileVersionInfo
						: null;
		}

		public static string GetFileName(this Process thisValue) { return GetCommandLine(thisValue).ExecutablePath; }

		public static (string ExecutablePath, string CommandLine) GetCommandLine(this Process thisValue)
		{
			if (!IsAwaitable(thisValue)) return (null, null);

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
}
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class ProcessExtension
{
	private static readonly Lazy<PropertyInfo> __associatedProperty = new Lazy<PropertyInfo>(() => typeof(Process).GetProperty("Associated", Constants.BF_NON_PUBLIC_INSTANCE, typeof(bool)), LazyThreadSafetyMode.PublicationOnly);

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsAssociated(this Process thisValue)
	{
		if (thisValue.IsDisposed()) return false;
		thisValue.Refresh();
		return (bool)__associatedProperty.Value.GetValue(thisValue);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsAwaitable(this Process thisValue) { return IsAssociated(thisValue) && !thisValue.HasExited; }

	[NotNull]
	public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, CancellationToken token = default(CancellationToken))
	{
		return WaitForExitAsync(thisValue, TimeSpanHelper.INFINITE, token);
	}

	[NotNull]
	public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
	{
		return WaitForExitAsync(thisValue, timeout.TotalIntMilliseconds(), token);
	}

	[NotNull]
	public static Task<bool> WaitForExitAsync([NotNull] this Process thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
	{
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		token.ThrowIfCancellationRequested();
		if (!IsAwaitable(thisValue)) return Task.FromResult(true);

		SafeWaitHandle waitHandle = new SafeWaitHandle(thisValue.Handle, false);
		if (!waitHandle.IsAwaitable()) return Task.FromResult(false);
		ManualResetEvent processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
		return TaskHelper.FromWaitHandle(processFinishedEvent, millisecondsTimeout, token)
						.ConfigureAwait()
						.ContinueWith(t =>
						{
							ObjectHelper.Dispose(ref processFinishedEvent);
							ObjectHelper.Dispose(ref waitHandle);
							return t.IsCompleted && t.Result;
						}, token);
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

		SafeWaitHandle waitHandle = null;
		ManualResetEvent processFinishedEvent = null;

		try
		{
			waitHandle = new SafeWaitHandle(thisValue.Handle, false);
			if (!waitHandle.IsAwaitable()) return false;
			processFinishedEvent = new ManualResetEvent(false) { SafeWaitHandle = waitHandle };
			return processFinishedEvent.WaitOne(millisecondsTimeout, true, exitContext, token);
		}
		finally
		{
			ObjectHelper.Dispose(ref processFinishedEvent);
			ObjectHelper.Dispose(ref waitHandle);
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
			predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForExit(TimeSpanHelper.ZERO);
		else
			predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForExit(TimeSpanHelper.ZERO) || evalFunc();

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
			predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForInputIdle(TimeSpanHelper.HALF);
		else
			predicate = () => !IsAwaitable(thisValue) || thisValue.WaitForInputIdle(TimeSpanHelper.HALF) || evalFunc();

		return SpinWait.SpinUntil(predicate, millisecondsTimeout);
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
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

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static FileVersionInfo Version([NotNull] this Process thisValue)
	{
		return IsAwaitable(thisValue)
					? thisValue.MainModule?.FileVersionInfo
					: null;
	}
}
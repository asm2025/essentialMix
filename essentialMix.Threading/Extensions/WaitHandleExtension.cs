using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class WaitHandleExtension
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsAwaitable(this WaitHandle thisValue) { return thisValue != null && thisValue.SafeWaitHandle.IsAwaitable(); }

	[NotNull]
	public static WaitHandle[] Merge(this WaitHandle thisValue, [NotNull] params WaitHandle[] waitHandles) { return waitHandles.Prepend(thisValue); }

	public static bool WaitOne([NotNull] this WaitHandle thisValue, CancellationToken token = default(CancellationToken)) { return WaitOne(thisValue, TimeSpanHelper.INFINITE, false, false, token); }

	public static bool WaitOne([NotNull] this WaitHandle thisValue, bool setEvent, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, TimeSpanHelper.INFINITE, setEvent, false, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, bool setEvent, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, TimeSpanHelper.INFINITE, setEvent, exitContext, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, timeout.TotalIntMilliseconds(), false, false, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, bool setEvent, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, timeout.TotalIntMilliseconds(), setEvent, false, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, bool setEvent, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, timeout.TotalIntMilliseconds(), setEvent, exitContext, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, millisecondsTimeout, false, false, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, bool setEvent, CancellationToken token = default(CancellationToken))
	{
		return WaitOne(thisValue, millisecondsTimeout, setEvent, false, token);
	}

	public static bool WaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, bool setEvent, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

		bool result;

		if (!token.CanBeCanceled)
		{
			result = thisValue.WaitOne(millisecondsTimeout, exitContext);
		}
		else if (token.IsCancellationRequested)
		{
			result = false;
		}
		else
		{
			int n = WaitHandle.WaitAny([thisValue, token.WaitHandle], millisecondsTimeout, exitContext);
			result = n == 0;
		}

		if (result && setEvent && thisValue is EventWaitHandle evt) evt.Set();
		return result;
	}

	[NotNull]
	public static Task<bool> WaitOneAsync([NotNull] this WaitHandle thisValue, CancellationToken token = default(CancellationToken)) { return WaitOneAsync(thisValue, TimeSpanHelper.INFINITE, false, token); }
	[NotNull]
	public static Task<bool> WaitOneAsync([NotNull] this WaitHandle thisValue, bool setEvent, CancellationToken token = default(CancellationToken)) { return WaitOneAsync(thisValue, TimeSpanHelper.INFINITE, setEvent, token); }
	[NotNull]
	public static Task<bool> WaitOneAsync([NotNull] this WaitHandle thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return WaitOneAsync(thisValue, timeout.TotalIntMilliseconds(), false, token); }
	[NotNull]
	public static Task<bool> WaitOneAsync([NotNull] this WaitHandle thisValue, TimeSpan timeout, bool setEvent, CancellationToken token = default(CancellationToken)) { return WaitOneAsync(thisValue, timeout.TotalIntMilliseconds(), setEvent, token); }
	[NotNull]
	public static Task<bool> WaitOneAsync([NotNull] this WaitHandle thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken)) { return WaitOneAsync(thisValue, millisecondsTimeout, false, token); }
	[NotNull]
	public static Task<bool> WaitOneAsync([NotNull] this WaitHandle thisValue, int millisecondsTimeout, bool setEvent, CancellationToken token = default(CancellationToken))
	{
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		if (token.IsCancellationRequested) return Task.FromCanceled<bool>(token);

		if (!token.CanBeCanceled)
		{
			bool result = thisValue.WaitOne(millisecondsTimeout);
			if (result && setEvent && thisValue is EventWaitHandle evt) evt.Set();
			return Task.FromResult(result);
		}

		return TaskHelper.FromWaitHandle(thisValue, millisecondsTimeout, token)
						.ContinueWith(t =>
						{
							if (t.Result && setEvent && thisValue is EventWaitHandle evt) evt.Set();
							return t.Result;
						}, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
	}

	public static bool WaitAll([NotNull] this WaitHandle[] thisValue, CancellationToken token = default(CancellationToken)) { return WaitAll(thisValue, TimeSpanHelper.INFINITE, false, token); }

	public static bool WaitAll([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
	{
		return WaitAll(thisValue, timeout.TotalIntMilliseconds(), false, token);
	}

	public static bool WaitAll([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		return WaitAll(thisValue, timeout.TotalIntMilliseconds(), exitContext, token);
	}

	public static bool WaitAll([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
	{
		return WaitAll(thisValue, millisecondsTimeout, false, token);
	}

	public static bool WaitAll([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

		if (!token.CanBeCanceled) return WaitHandle.WaitAll(thisValue, millisecondsTimeout, exitContext);
		if (token.IsCancellationRequested) return false;

		return WaitHandle.WaitAll(thisValue, millisecondsTimeout, exitContext) && !token.IsCancellationRequested;
	}

	public static int WaitAny([NotNull] this WaitHandle[] thisValue, CancellationToken token = default(CancellationToken)) { return WaitAny(thisValue, TimeSpanHelper.INFINITE, false, token); }

	public static int WaitAny([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
	{
		return WaitAny(thisValue, timeout.TotalIntMilliseconds(), false, token);
	}

	public static int WaitAny([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		return WaitAny(thisValue, timeout.TotalIntMilliseconds(), exitContext, token);
	}

	public static int WaitAny([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
	{
		return WaitAny(thisValue, millisecondsTimeout, false, token);
	}

	public static int WaitAny([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, bool exitContext, CancellationToken token = default(CancellationToken))
	{
		if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		if (!token.CanBeCanceled) return WaitHandle.WaitAny(thisValue, millisecondsTimeout, exitContext);

		WaitHandle[] handles = thisValue.Append(token.WaitHandle);
		return WaitHandle.WaitAny(handles, millisecondsTimeout, exitContext);
	}

	public static bool SignalAndWait([NotNull] this WaitHandle thisValue, [NotNull] WaitHandle toWaitOn, TimeSpan timeout, bool exitContext = false)
	{
		return SignalAndWait(thisValue, toWaitOn, timeout.TotalIntMilliseconds(), exitContext);
	}

	public static bool SignalAndWait([NotNull] this WaitHandle thisValue, [NotNull] WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext = false)
	{
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		return WaitHandle.SignalAndWait(thisValue, toWaitOn, millisecondsTimeout, exitContext);
	}

	public static bool SpinWaitOne([NotNull] this WaitHandle thisValue, Func<bool> evalFunc, bool setEvent = false, bool exitContext = false)
	{
		return SpinWaitOne(thisValue, TimeSpanHelper.INFINITE, evalFunc, setEvent, exitContext);
	}

	public static bool SpinWaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, Func<bool> evalFunc = null, bool setEvent = false, bool exitContext = false)
	{
		return SpinWaitOne(thisValue, timeout.TotalIntMilliseconds(), evalFunc, setEvent, exitContext);
	}

	public static bool SpinWaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, Func<bool> evalFunc, bool setEvent = false, bool exitContext = false)
	{
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		if (!IsAwaitable(thisValue)) return true;

		Func<bool> predicate = evalFunc == null
									? () => !IsAwaitable(thisValue) || thisValue.WaitOne(TimeSpanHelper.ZERO, exitContext)
									: () => !IsAwaitable(thisValue) || thisValue.WaitOne(TimeSpanHelper.ZERO, exitContext) || evalFunc();
		bool result = SpinWait.SpinUntil(predicate, millisecondsTimeout);
		if (result && setEvent && thisValue is EventWaitHandle evt) evt.Set();
		return result;
	}

	public static bool SpinWaitAll([NotNull] this WaitHandle[] thisValue, Func<bool> evalFunc, bool exitContext = false)
	{
		return SpinWaitAll(thisValue, TimeSpanHelper.INFINITE, evalFunc, exitContext);
	}

	public static bool SpinWaitAll([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, Func<bool> evalFunc = null, bool exitContext = false)
	{
		return SpinWaitAll(thisValue, timeout.TotalIntMilliseconds(), evalFunc, exitContext);
	}

	public static bool SpinWaitAll([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, Func<bool> evalFunc, bool exitContext = false)
	{
		if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

		Func<bool> predicate = evalFunc == null
									? () => WaitHandle.WaitAll(thisValue, TimeSpanHelper.ZERO, exitContext)
									: () => WaitHandle.WaitAll(thisValue, TimeSpanHelper.ZERO, exitContext) || evalFunc();
		return SpinWait.SpinUntil(predicate, millisecondsTimeout);
	}

	public static int SpinWaitAny([NotNull] this WaitHandle[] thisValue, Func<bool> evalFunc = null) { return SpinWaitAny(thisValue, TimeSpanHelper.INFINITE, evalFunc); }

	public static int SpinWaitAny([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, Func<bool> evalFunc = null, bool exitContext = false)
	{
		return SpinWaitAny(thisValue, timeout.TotalIntMilliseconds(), evalFunc, exitContext);
	}

	public static int SpinWaitAny([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, Func<bool> evalFunc = null, bool exitContext = false)
	{
		if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

		int index = -1;
		Func<bool> predicate = evalFunc == null
									? () =>
									{
										index = WaitHandle.WaitAny(thisValue, TimeSpanHelper.ZERO, exitContext);
										return index == 0;
									}
		: () =>
		{
			index = WaitHandle.WaitAny(thisValue, TimeSpanHelper.ZERO, exitContext);
			return index == 0 || evalFunc();
		};

		SpinWait.SpinUntil(predicate, millisecondsTimeout);
		return index;
	}

	[NotNull]
	public static Task<bool> ToTask([NotNull] this WaitHandle thisValue) { return ToTask(thisValue, TimeSpanHelper.INFINITE, CancellationToken.None); }
	[NotNull]
	public static Task<bool> ToTask([NotNull] this WaitHandle thisValue, CancellationToken token) { return ToTask(thisValue, TimeSpanHelper.INFINITE, token); }
	[NotNull]
	public static Task<bool> ToTask([NotNull] this WaitHandle thisValue, TimeSpan timeout) { return ToTask(thisValue, timeout.TotalIntMilliseconds(), CancellationToken.None); }
	[NotNull]
	public static Task<bool> ToTask([NotNull] this WaitHandle thisValue, TimeSpan timeout, CancellationToken token) { return ToTask(thisValue, timeout.TotalIntMilliseconds(), token); }
	[NotNull]
	public static Task<bool> ToTask([NotNull] this WaitHandle thisValue, int millisecondsTimeout) { return ToTask(thisValue, millisecondsTimeout, CancellationToken.None); }
	[NotNull]
	public static Task<bool> ToTask([NotNull] this WaitHandle thisValue, int millisecondsTimeout, CancellationToken token)
	{
		return TaskHelper.FromWaitHandle(thisValue, millisecondsTimeout, token);
	}
}
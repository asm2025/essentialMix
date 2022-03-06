using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using Timer = System.Timers.Timer;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class CancellationTokenSourceExtension
{
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static bool IsCancelledOrDisposed(this CancellationTokenSource thisValue)
	{
		try { return thisValue is not { IsCancellationRequested: not true }; }
		catch (ObjectDisposedException) { return true; }
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static void CancelIfNotDisposed(this CancellationTokenSource thisValue, bool throwOnFirstException = false)
	{
		if (IsCancelledOrDisposed(thisValue)) return;
		try { thisValue.Cancel(throwOnFirstException); }
		catch (ObjectDisposedException) { }
	}

	[NotNull]
	public static CancellationTokenSource Merge([NotNull] this CancellationTokenSource thisValue, [NotNull] params CancellationToken[] tokens)
	{
		return tokens.Length == 0
					? thisValue
					: thisValue.Token.Merge(tokens);
	}

	[NotNull]
	public static CancellationTokenSource Merge([NotNull] this CancellationTokenSource thisValue, [NotNull] params CancellationTokenSource[] sources)
	{
		if (sources.Length == 0) return thisValue;
		return thisValue.Token.Merge(sources.Where(e => e != null)
											.Select(e => e.Token)
											.ToArray());
	}

	[NotNull]
	public static CancellationTokenSource Merge([NotNull] this CancellationTokenSource thisValue, CancellationToken token) { return thisValue.Token.Merge(token); }

	[NotNull]
	public static CancellationTokenSource Merge([NotNull] this CancellationTokenSource thisValue, [NotNull] CancellationTokenSource source)
	{
		return thisValue.Token.Merge(source.Token);
	}

	public static CancellationTokenSource CancelAfter(this CancellationTokenSource thisValue, TimeSpan timeout)
	{
		return CancelAfter(thisValue, timeout.TotalIntMilliseconds());
	}

	public static CancellationTokenSource CancelAfter(this CancellationTokenSource thisValue, int timeoutMillisecond)
	{
		if (timeoutMillisecond < Timeout.Infinite) throw new ArgumentOutOfRangeException(nameof(timeoutMillisecond));

		Timer timer = new Timer(timeoutMillisecond) { AutoReset = false };
		timer.Elapsed += (_, _) => thisValue?.Cancel();
		timer.Start();
		return thisValue;
	}
}
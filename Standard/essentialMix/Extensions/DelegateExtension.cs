using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class DelegateExtension
	{
		[NotNull]
		public static Task AsTask([NotNull] Action action, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			Task.Yield();
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

			try
			{
				action();
				tcs.TrySetResult(Type.Missing);
			}
			catch (OperationCanceledException)
			{
				tcs.TrySetCanceled(token);
			}
			catch (Exception e)
			{
				tcs.SetException(e);
			}

			return tcs.Task;
		}

		[NotNull]
		public static Task<TResult> AsTask<TResult>([NotNull] Func<TResult> func, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			Task.Yield();
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

			try
			{
				TResult result = func();
				tcs.TrySetResult(result);
			}
			catch (OperationCanceledException)
			{
				tcs.TrySetCanceled(token);
			}
			catch (Exception e)
			{
				tcs.SetException(e);
			}
	
			return tcs.Task;
		}

		public static void WithRetry([NotNull] Action action, int retries = 1, TimeSpan? timeBetweenExecutions = null, CancellationToken token = default(CancellationToken))
		{
			WithRetry(action, retries, timeBetweenExecutions?.TotalIntMilliseconds() ?? 0, token);
		}

		public static void WithRetry([NotNull] Action action, int retries = 1, int milliSecondTimeBetweenExecutions = 0, CancellationToken token = default(CancellationToken))
		{
			if (retries < 1) throw new ArgumentOutOfRangeException(nameof(retries));

			int remaining = retries + 1;

			while (!token.IsCancellationRequested)
			{
				try
				{
					action();
				}
				catch
				{
					remaining--;
					if (remaining == 0) throw;
					if (milliSecondTimeBetweenExecutions > 0) TimeSpanHelper.WasteTime(milliSecondTimeBetweenExecutions, token);
				}
			}
		}

		public static TResult WithRetry<TResult>([NotNull] Func<TResult> func, int retries = 1, TimeSpan? timeBetweenExecutions = null,
			CancellationToken token = default(CancellationToken))
		{
			return WithRetry(func, retries, timeBetweenExecutions?.TotalIntMilliseconds() ?? 0, token);
		}

		public static TResult WithRetry<TResult>([NotNull] Func<TResult> func, int retries = 1, int milliSecondTimeBetweenExecutions = 0,
			CancellationToken token = default(CancellationToken))
		{
			if (retries < 1) throw new ArgumentOutOfRangeException(nameof(retries));

			int remaining = retries + 1;
			TResult result = default(TResult);

			while (!token.IsCancellationRequested)
			{
				try
				{
					result = func();
				}
				catch
				{
					remaining--;
					if (remaining == 0) throw;
					if (milliSecondTimeBetweenExecutions > 0) TimeSpanHelper.WasteTime(milliSecondTimeBetweenExecutions, token);
				}
			}

			return result;
		}
	}
}
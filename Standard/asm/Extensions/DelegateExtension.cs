using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class DelegateExtension
	{
		public static void WithRetry([NotNull] Action action, int retries = 1, TimeSpan? timeBetweenExecutions = null, CancellationToken token = default(CancellationToken)) { WithRetry(action, retries, timeBetweenExecutions?.TotalIntMilliseconds() ?? 0, token); }
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
					if (milliSecondTimeBetweenExecutions > 0) Thread.Sleep(milliSecondTimeBetweenExecutions);
				}
			}
		}

		public static TResult WithRetry<TResult>([NotNull] Func<TResult> func, int retries = 1, TimeSpan? timeBetweenExecutions = null, CancellationToken token = default(CancellationToken))
		{
			return WithRetry(func, retries, timeBetweenExecutions?.TotalIntMilliseconds() ?? 0, token);
		}

		public static TResult WithRetry<TResult>([NotNull] Func<TResult> func, int retries = 1, int milliSecondTimeBetweenExecutions = 0, CancellationToken token = default(CancellationToken))
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
					if (milliSecondTimeBetweenExecutions > 0) Thread.Sleep(milliSecondTimeBetweenExecutions);
				}
			}

			return result;
		}
	}
}
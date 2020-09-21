using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Threading.Helpers
{
	public static class TaskHelper
	{
		public static int QueueMinimum { get; } = 1;
		public static int QueueMaximum { get; } = SystemInfo.IsHyperThreadingEnabled 
			? Environment.ProcessorCount * 2
			: Environment.ProcessorCount;
		public static int QueueDefault { get; } = QueueMaximum;

		public static int ProcessMinimum { get; } = 1;
		public static int ProcessMaximum { get; } = Environment.ProcessorCount;
		public static int ProcessDefault { get; } = ProcessMaximum;

		[NotNull]
		public static Task AsyncPattern([NotNull] Action action, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			
			try
			{
				action();
				return Task.CompletedTask;
			}
			catch (OperationCanceledException)
			{
				return Task.FromCanceled(token);
			}
		}

		[NotNull]
		public static Task<TResult> AsyncPattern<TResult>([NotNull] Func<TResult> func, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			
			try
			{
				return Task.FromResult(func());
			}
			catch (OperationCanceledException)
			{
				return Task.FromCanceled<TResult>(token);
			}
		}

		[NotNull]
		public static Task RunAsync([NotNull] Action action, CancellationToken token = default(CancellationToken)) { return AsyncPattern(action, token); }

		[NotNull]
		public static Task<TResult> RunAsync<TResult>([NotNull] Func<TResult> func, CancellationToken token = default(CancellationToken)) { return AsyncPattern(func, token); }

		[NotNull]
		public static Task RunAsync([NotNull] Action<CancellationTokenSource> action, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return RunAsync(action, timeout.TotalIntMilliseconds(), token);
		}

		[NotNull]
		public static Task RunAsync([NotNull] Action<CancellationTokenSource> action, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			CancellationTokenSource cts = new CancellationTokenSource(millisecondsTimeout);
			CancellationTokenSource mergedCts = !token.CanBeCanceled
				? cts
				: cts.Merge(token);
			Task task = Task.Run(() => action(mergedCts), mergedCts.Token)
				.TimeoutAfter(millisecondsTimeout, mergedCts.Token)
				.ContinueWith(t =>
				{
					ObjectHelper.Dispose(ref mergedCts);
					ObjectHelper.Dispose(ref cts);
				}, mergedCts.Token);
			return task.ConfigureAwait();
		}

		[NotNull]
		public static Task<TResult> RunAsync<TResult>([NotNull] Func<CancellationTokenSource, TResult> func, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return RunAsync(func, timeout.TotalIntMilliseconds(), token);
		}

		[NotNull]
		public static Task<TResult> RunAsync<TResult>([NotNull] Func<CancellationTokenSource, TResult> func, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			CancellationTokenSource cts = new CancellationTokenSource(millisecondsTimeout);
			CancellationTokenSource mergedCts = !token.CanBeCanceled
				? cts
				: cts.Merge(token);
			Task<TResult> task = Task.Run(() => func(mergedCts), mergedCts.Token)
				.TimeoutAfter(millisecondsTimeout, mergedCts.Token)
				.ContinueWith(t =>
				{
					ObjectHelper.Dispose(ref mergedCts);
					ObjectHelper.Dispose(ref cts);
					return t.Result;
				}, mergedCts.Token);
			return task.ConfigureAwait();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void Run([NotNull] Func<Task> func, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			func().Wait(token);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TResult Run<TResult>([NotNull] Func<Task<TResult>> func, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return func().GetAwaiter().GetResult();
		}

		public static void Sequence([NotNull] Action[] actions, bool continueOnError, CancellationToken token = default(CancellationToken))
		{
			if (actions.Length == 0) return;
			if (actions.Any(e => e == null)) throw new NullReferenceException($"{nameof(actions)} contains a null reference.");
			IEnumerable<Action> en = actions;
			if (token.CanBeCanceled) en = en.TakeWhile(e => !token.IsCancellationRequested);
			
			Task result = Task.CompletedTask;

			foreach (Action action in en)
			{
				result = result.ContinueWith(t => token.IsCancellationRequested || t.IsCanceled
													? Task.FromCanceled(token)
													: t.IsFaulted
														? continueOnError
															? Task.Run(action, token).ConfigureAwait()
															: Task.FromException(t.Exception ?? new Exception("Unknown error."))
														: Task.Run(action, token).ConfigureAwait(), TaskContinuationOptions.NotOnCanceled)
								.Unwrap();
			}
		}

		public static Task<TResult> Sequence<TResult>([NotNull] Func<TResult, Task<TResult>>[] functions, bool continueOnError, CancellationToken token = default(CancellationToken))
		{
			return Sequence(functions, continueOnError, default(TResult), null, token);
		}

		public static Task<TResult> Sequence<TResult>([NotNull] Func<TResult, Task<TResult>>[] functions, TResult defaultValue, [NotNull] Func<TResult, bool> evaluator, CancellationToken token = default(CancellationToken))
		{
			return Sequence(functions, false, defaultValue, evaluator, token);
		}

		public static Task<TResult> Sequence<TResult>([NotNull] Func<TResult, Task<TResult>>[] functions, bool continueOnError, [NotNull] Func<TResult, bool> evaluator, CancellationToken token = default(CancellationToken))
		{
			return Sequence(functions, continueOnError, default(TResult), evaluator, token);
		}

		public static Task<TResult> Sequence<TResult>([NotNull] Func<TResult, Task<TResult>>[] functions, bool continueOnError, TResult defaultValue, Func<TResult, bool> evaluator, CancellationToken token = default(CancellationToken))
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			tcs.SetResult(defaultValue);
			if (functions.Length == 0) return tcs.Task;

			IEnumerable<Func<TResult, Task<TResult>>> fs = functions;
			if (token.CanBeCanceled) fs = fs.TakeWhile(f => !token.IsCancellationRequested);
			Task<TResult> result = tcs.Task;

			foreach (Func<TResult, Task<TResult>> func in fs)
			{
				result = result.ContinueWith(t => token.IsCancellationRequested || t.IsCanceled
													? Task.FromCanceled<TResult>(token)
													: t.IsFaulted
														? continueOnError
															? func(defaultValue).ConfigureAwait()
															: Task.FromException<TResult>(t.Exception ?? new Exception("Unknown error."))
														: evaluator?.Invoke(t.Result) == true
															? Task.FromResult(t.Result)
															: func(t.Result).ConfigureAwait(), TaskContinuationOptions.NotOnCanceled)
								.Unwrap();
			}

			return result;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public static class TaskHelper
	{
		// based on Nito.AsyncEx.Interop
		private sealed class WaitHandleRegistration : IDisposable
		{
			private readonly RegisteredWaitHandle _registeredWaitHandle;

			public WaitHandleRegistration([NotNull] WaitHandle handle, int millisecondsTimeout, TaskCompletionSource<bool> completionSource)
			{
				_registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle, 
																(state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), 
																completionSource, millisecondsTimeout, true);
			}

			/// <inheritdoc />
			void IDisposable.Dispose() { _registeredWaitHandle.Unregister(null); }
		}

		public static int QueueMinimum => 1;

		public static int QueueMaximum { get; } = SystemInfo.IsHyperThreadingEnabled 
													? Environment.ProcessorCount * 2
													: Environment.ProcessorCount;
		public static int QueueDefault => QueueMaximum;

		public static int ProcessMinimum => 1;
		public static int ProcessMaximum { get; } = Environment.ProcessorCount;
		public static int ProcessDefault => ProcessMaximum;

		[NotNull]
		public static Task Run([NotNull] Action action, TaskCreationOptions options) { return Run(action, options, CancellationToken.None); }
		/// <summary>
		/// See these articles first to understand why this method exists, Then you can decide if you want to use it or not.
		/// <para><see href="https://devblogs.microsoft.com/pfxteam/task-run-vs-task-factory-startnew/" /></para>
		/// <para><see href="https://blog.stephencleary.com/2013/08/startnew-is-dangerous.html" /></para>
		/// <para><see href="https://blog.i3arnon.com/2015/07/02/task-run-long-running/" /></para>
		/// <para><see href="https://github.com/dotnet/runtime/issues/14978" /></para>
		/// <para><see href="https://devblogs.microsoft.com/pfxteam/taskcreationoptions-preferfairness/" /></para>
		/// <para>The only meaningful options here are <see cref="TaskCreationOptions.PreferFairness"/> or <see cref="TaskCreationOptions.LongRunning"/></para>
		/// </summary>
		[NotNull]
		public static Task Run([NotNull] Action action, TaskCreationOptions options, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			TaskCreationOptions opt = TaskCreationOptions.None;
			if ((options & TaskCreationOptions.LongRunning) == TaskCreationOptions.LongRunning) opt |= TaskCreationOptions.LongRunning;
			if ((options & TaskCreationOptions.PreferFairness) == TaskCreationOptions.PreferFairness) opt |= TaskCreationOptions.PreferFairness;
			if ((options & TaskCreationOptions.HideScheduler) == TaskCreationOptions.HideScheduler) opt |= TaskCreationOptions.HideScheduler;
			if ((options & TaskCreationOptions.RunContinuationsAsynchronously) == TaskCreationOptions.RunContinuationsAsynchronously) opt |= TaskCreationOptions.RunContinuationsAsynchronously;
			if (opt == TaskCreationOptions.None) return Task.Run(action, token);
			opt |= TaskCreationOptions.DenyChildAttach;
			return Task.Factory.StartNew(action, token, opt, Task.Factory.Scheduler ?? TaskScheduler.Default);
		}

		[NotNull]
		public static Task<T> Run<T>([NotNull] Func<T> func, TaskCreationOptions options) { return Run(func, options, CancellationToken.None); }
		/// <summary>
		/// See these articles first to understand why this method exists, Then you can decide if you want to use it or not.
		/// <para><see href="https://devblogs.microsoft.com/pfxteam/task-run-vs-task-factory-startnew/" /></para>
		/// <para><see href="https://blog.stephencleary.com/2013/08/startnew-is-dangerous.html" /></para>
		/// <para><see href="https://blog.i3arnon.com/2015/07/02/task-run-long-running/" /></para>
		/// <para><see href="https://github.com/dotnet/runtime/issues/14978" /></para>
		/// <para><see href="https://devblogs.microsoft.com/pfxteam/taskcreationoptions-preferfairness/" /></para>
		/// <para>The only meaningful options here are <see cref="TaskCreationOptions.PreferFairness"/> or <see cref="TaskCreationOptions.LongRunning"/></para>
		/// </summary>
		[NotNull]
		public static Task<T> Run<T>([NotNull] Func<T> func, TaskCreationOptions options, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			TaskCreationOptions opt = TaskCreationOptions.None;
			if ((options & TaskCreationOptions.LongRunning) == TaskCreationOptions.LongRunning) opt |= TaskCreationOptions.LongRunning;
			if ((options & TaskCreationOptions.PreferFairness) == TaskCreationOptions.PreferFairness) opt |= TaskCreationOptions.PreferFairness;
			if ((options & TaskCreationOptions.HideScheduler) == TaskCreationOptions.HideScheduler) opt |= TaskCreationOptions.HideScheduler;
			if ((options & TaskCreationOptions.RunContinuationsAsynchronously) == TaskCreationOptions.RunContinuationsAsynchronously) opt |= TaskCreationOptions.RunContinuationsAsynchronously;
			if (opt == TaskCreationOptions.None) return Task.Run(func, token);
			opt |= TaskCreationOptions.DenyChildAttach;
			return Task.Factory.StartNew(func, token, opt, Task.Factory.Scheduler ?? TaskScheduler.Default);
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
			return func().Execute();
		}

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
			// copy to local variable
			CancellationToken tkn = mergedCts.Token;
			return Task.Run(() => action(mergedCts), tkn)
					.TimeoutAfter(millisecondsTimeout, tkn)
					.ContinueWith(_ =>
					{
						ObjectHelper.Dispose(ref mergedCts);
						ObjectHelper.Dispose(ref cts);
					}, tkn);
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
			// copy to local variable
			CancellationToken tkn = mergedCts.Token;
			return Task.Run(() => func(mergedCts), tkn)
					.TimeoutAfter(millisecondsTimeout, tkn)
					.ContinueWith(t =>
					{
						ObjectHelper.Dispose(ref mergedCts);
						ObjectHelper.Dispose(ref cts);
						return t.Result;
					}, tkn);
		}

		public static void Sequence([NotNull] params Action[] actions)
		{
			if (actions.Length == 0) return;

			foreach (Action action in actions)
			{
				if (action != null) continue;
				throw new NullReferenceException($"{nameof(actions)} contains a null reference.");
			}

			foreach (Action action in actions) 
				action();
		}
		
		[NotNull]
		public static Task SequenceAsync([NotNull] params Action[] actions) { return SequenceAsync(CancellationToken.None, actions); }
		[NotNull]
		public static Task SequenceAsync(CancellationToken token, [NotNull] params Action[] actions)
		{
			if (actions.Length == 0) return Task.CompletedTask;

			foreach (Action action in actions)
			{
				if (action != null) continue;
				throw new NullReferenceException($"{nameof(actions)} contains a null reference.");
			}

			Task firstTask = Task.CompletedTask;
			Task task = firstTask;

			foreach (Action action in actions) 
				task = task.ContinueWith(_ => action(), token, TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default).ConfigureAwait();

			return firstTask;
		}

		public static TResult Sequence<TResult>([NotNull] params Func<TResult, TResult>[] functions) { return Sequence(default(TResult), null, functions); }
		public static TResult Sequence<TResult>([NotNull] Func<TResult, bool> evaluator, [NotNull] params Func<TResult, TResult>[] functions) { return Sequence(default(TResult), evaluator, functions); }
		public static TResult Sequence<TResult>(TResult defaultValue, Func<TResult, bool> evaluator, [NotNull] params Func<TResult, TResult>[] functions)
		{
			if (functions.Length == 0) return defaultValue;

			foreach (Func<TResult, TResult> func in functions)
			{
				if (func != null) continue;
				throw new NullReferenceException($"{nameof(functions)} contains a null reference.");
			}

			TResult result = defaultValue;

			if (evaluator != null)
			{
				foreach (Func<TResult, TResult> func in functions)
				{
					result = func(result);
					if (evaluator(result)) break;
				}
			}
			else
			{
				foreach (Func<TResult, TResult> func in functions) 
					result = func(result);
			}

			return result;
		}

		[NotNull]
		public static Task<TResult> SequenceAsync<TResult>([NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, default(TResult), null, functions); }
		[NotNull]
		public static Task<TResult> SequenceAsync<TResult>([NotNull] Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, default(TResult), evaluator, functions); }
		[NotNull]
		public static Task<TResult> SequenceAsync<TResult>(TResult defaultValue, Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, defaultValue, evaluator, functions); }
		[NotNull]
		public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(token, default(TResult), null, functions); }
		[NotNull]
		public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(token, default(TResult), evaluator, functions); }
		public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, TResult defaultValue, Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions)
		{
			if (functions.Length == 0) return Task.FromResult(defaultValue);

			Queue<Func<TResult, Task<TResult>>> queue = new Queue<Func<TResult, Task<TResult>>>(functions.Length);

			foreach (Func<TResult, Task<TResult>> func in functions)
			{
				if (func == null) throw new NullReferenceException($"{nameof(functions)} contains a null reference.");
				queue.Enqueue(func);
			}

			return SequenceAsyncLocal(queue, evaluator, defaultValue, token);

			static async Task<TResult> SequenceAsyncLocal(Queue<Func<TResult, Task<TResult>>> queue, Func<TResult, bool> evaluator, TResult defaultValue, CancellationToken token)
			{
				TResult result = defaultValue;
				CancellationTokenSource cts = null;
				IDisposable tokenRegistration = null;

				try
				{
					cts = new CancellationTokenSource();
					if (token.CanBeCanceled) tokenRegistration = token.Register(state => ((CancellationTokenSource)state).CancelIfNotDisposed(), cts, false);

					while (!token.IsCancellationRequested && queue.Count > 0)
					{
						Func<TResult, Task<TResult>> func = queue.Dequeue();
						result = await func(result).ConfigureAwait();
						if (token.IsCancellationRequested || evaluator != null && !evaluator(result)) continue;
						return result;
					}

					return result;
				}
				finally
				{
					ObjectHelper.Dispose(ref tokenRegistration);
					ObjectHelper.Dispose(ref cts);
				}
			}
		}

		[NotNull]
		public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle) { return FromWaitHandle(handle, TimeSpanHelper.INFINITE, CancellationToken.None); }
		[NotNull]
		public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle, CancellationToken token) { return FromWaitHandle(handle, TimeSpanHelper.INFINITE, token); }
		[NotNull]
		public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle, TimeSpan timeout) { return FromWaitHandle(handle, timeout.TotalIntMilliseconds(), CancellationToken.None); }
		[NotNull]
		public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle, TimeSpan timeout, CancellationToken token) { return FromWaitHandle(handle, timeout.TotalIntMilliseconds(), token); }
		[NotNull]
		public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle, int millisecondsTimeout) { return FromWaitHandle(handle, millisecondsTimeout, CancellationToken.None); }
		public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle, int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			
			bool isSet = handle.WaitOne(0);
			if (isSet) return Task.FromResult(true);
			if (millisecondsTimeout == 0 || token.IsCancellationRequested) return Task.FromResult(false);
			return FromWaitHandleLocal(handle, millisecondsTimeout, token);

			static async Task<bool> FromWaitHandleLocal(WaitHandle handle, int millisecondsTimeout, CancellationToken token)
			{
				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				WaitHandleRegistration registration = null;
				IDisposable tokenRegistration = null;

				try
				{
					registration = new WaitHandleRegistration(handle, millisecondsTimeout, tcs);
					if (token.CanBeCanceled) tokenRegistration = token.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs, false);
					return await tcs.Task.ConfigureAwait();
				}
				finally
				{
					ObjectHelper.Dispose(ref registration);
					ObjectHelper.Dispose(ref tokenRegistration);
				}
			}
		}
	}
}
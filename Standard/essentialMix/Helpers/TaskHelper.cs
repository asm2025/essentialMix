using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public static class TaskHelper
	{
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

		public static void Sequence([NotNull] params Action[] actions) { SequenceAsync(CancellationToken.None, actions).Execute(); }
		
		[NotNull]
		public static Task SequenceAsync([NotNull] params Action[] actions) { return SequenceAsync(CancellationToken.None, actions); }
		public static async Task SequenceAsync(CancellationToken token, [NotNull] params Action[] actions)
		{
			if (actions.Length == 0) return;

			Queue<Action> queue = new Queue<Action>(actions.Length);

			foreach (Action action in actions)
			{
				if (action == null) throw new NullReferenceException($"{nameof(actions)} contains a null reference.");
				queue.Enqueue(action);
			}

			IDisposable link = null;

			try
			{
				BufferBlock<Action> buffer = new BufferBlock<Action>(new DataflowBlockOptions
				{
					CancellationToken = token
				});
				ActionBlock<Action> processor = new ActionBlock<Action>(ac => ac(), new ExecutionDataflowBlockOptions
				{
					MaxDegreeOfParallelism = 1,
					CancellationToken = token
				});
				link = buffer.LinkTo(processor, new DataflowLinkOptions
				{
					PropagateCompletion = false
				});

				while (!token.IsCancellationRequested && queue.Count > 0)
				{
					Action action = queue.Dequeue();
					await buffer.SendAsync(action, token).ConfigureAwait();
				}

				token.ThrowIfCancellationRequested();
				buffer.Complete();
				await Task.WhenAll(buffer.Completion, processor.Completion);
			}
			finally
			{
				ObjectHelper.Dispose(ref link);
			}
		}

		public static TResult Sequence<TResult>([NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, default(TResult), null, functions).Execute(); }
		public static TResult Sequence<TResult>([NotNull] Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, default(TResult), evaluator, functions).Execute(); }
		public static TResult Sequence<TResult>(TResult defaultValue, [NotNull] Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, defaultValue, evaluator, functions).Execute(); }

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
		public static async Task<TResult> SequenceAsync<TResult>(CancellationToken token, TResult defaultValue, Func<TResult, bool> evaluator, [NotNull] params Func<TResult, Task<TResult>>[] functions)
		{
			if (functions.Length == 0) return defaultValue;

			Queue<Func<TResult, Task<TResult>>> queue = new Queue<Func<TResult, Task<TResult>>>(functions.Length);

			foreach (Func<TResult, Task<TResult>> func in functions)
			{
				if (func == null) throw new NullReferenceException($"{nameof(functions)} contains a null reference.");
				queue.Enqueue(func);
			}

			TResult result = defaultValue;
			CancellationTokenSource cts = null;
			IDisposable link = null;

			try
			{
				cts = new CancellationTokenSource();

				if (token.CanBeCanceled)
				{
					CancellationTokenSource ctsReg = cts;
					token.Register(() => ctsReg.Cancel());
				}

				BufferBlock<Func<TResult, Task<TResult>>> buffer = new BufferBlock<Func<TResult, Task<TResult>>>(new DataflowBlockOptions
				{
					CancellationToken = token
				});

				CancellationTokenSource ctsRef = cts;
				ActionBlock<Func<TResult, Task<TResult>>> processor = new ActionBlock<Func<TResult, Task<TResult>>>(func =>
				{
					result = func(result).Execute();
					if (!token.IsCancellationRequested && (evaluator == null || evaluator(result))) return;
					result = defaultValue;
					ctsRef.Cancel();
				}, new ExecutionDataflowBlockOptions
				{
					MaxDegreeOfParallelism = 1,
					CancellationToken = token
				});
				link = buffer.LinkTo(processor, new DataflowLinkOptions
				{
					PropagateCompletion = false
				});

				while (!token.IsCancellationRequested && queue.Count > 0)
				{
					Func<TResult, Task<TResult>> func = queue.Dequeue();
					await buffer.SendAsync(func, token).ConfigureAwait();
				}

				token.ThrowIfCancellationRequested();
				buffer.Complete();
				await Task.WhenAll(buffer.Completion, processor.Completion);
				return result;
			}
			finally
			{
				ObjectHelper.Dispose(ref link);
				ObjectHelper.Dispose(ref cts);
			}
		}
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class TaskHelper
{
	// based on Nito.AsyncEx.Interop.CancellationTokenRegistration
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

	public static void Run([NotNull] Action action, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { Run(action, timeout.TotalIntMilliseconds(), TaskCreationOptions.None, null, token); }
	public static void Run([NotNull] Action action, TimeSpan timeout, Action onTimeout, CancellationToken token = default(CancellationToken)) { Run(action, timeout.TotalIntMilliseconds(), TaskCreationOptions.None, onTimeout, token); }
	public static void Run([NotNull] Action action, TimeSpan timeout, TaskCreationOptions options, CancellationToken token = default(CancellationToken)) { Run(action, timeout.TotalIntMilliseconds(), options, null, token); }
	public static void Run([NotNull] Action action, TimeSpan timeout, TaskCreationOptions options, Action onTimeout, CancellationToken token = default(CancellationToken)) { Run(action, timeout.TotalIntMilliseconds(), options, onTimeout, token); }
	public static void Run([NotNull] Action action, int millisecondTimeout, CancellationToken token = default(CancellationToken)) { Run(action, millisecondTimeout, TaskCreationOptions.None, null, token); }
	public static void Run([NotNull] Action action, int millisecondTimeout, Action onTimeout, CancellationToken token = default(CancellationToken)) { Run(action, millisecondTimeout, TaskCreationOptions.None, onTimeout, token); }
	public static void Run([NotNull] Action action, int millisecondTimeout, TaskCreationOptions options, CancellationToken token = default(CancellationToken)) { Run(action, millisecondTimeout, options, null, token); }
	public static void Run([NotNull] Action action, int millisecondTimeout, TaskCreationOptions options, Action onTimeout, CancellationToken token = default(CancellationToken))
	{
		if (millisecondTimeout < 0) throw new ArgumentOutOfRangeException(nameof(millisecondTimeout));
		token.ThrowIfCancellationRequested();
		Task task = Run(action, options, token).ConfigureAwait();
		if (task.Wait(millisecondTimeout)) return;
		token.ThrowIfCancellationRequested();
		if (onTimeout == null) throw new TimeoutException();
		onTimeout();
	}

	public static T Run<T>([NotNull] Func<T> func, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return Run(func, default(T), timeout.TotalIntMilliseconds(), TaskCreationOptions.None, null, token); }
	public static T Run<T>([NotNull] Func<T> func, TimeSpan timeout, Action onTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, default(T), timeout.TotalIntMilliseconds(), TaskCreationOptions.None, onTimeout, token); }
	public static T Run<T>([NotNull] Func<T> func, TimeSpan timeout, TaskCreationOptions options, CancellationToken token = default(CancellationToken)) { return Run(func, default(T), timeout.TotalIntMilliseconds(), options, null, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, timeout.TotalIntMilliseconds(), TaskCreationOptions.None, null, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, TimeSpan timeout, Action onTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, timeout.TotalIntMilliseconds(), TaskCreationOptions.None, onTimeout, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, TimeSpan timeout, TaskCreationOptions options, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, timeout.TotalIntMilliseconds(), options, null, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, TimeSpan timeout, TaskCreationOptions options, Action onTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, timeout.TotalIntMilliseconds(), options, onTimeout, token); }
	public static T Run<T>([NotNull] Func<T> func, int millisecondTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, default(T), millisecondTimeout, TaskCreationOptions.None, null, token); }
	public static T Run<T>([NotNull] Func<T> func, int millisecondTimeout, Action onTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, default(T), millisecondTimeout, TaskCreationOptions.None, onTimeout, token); }
	public static T Run<T>([NotNull] Func<T> func, int millisecondTimeout, TaskCreationOptions options, CancellationToken token = default(CancellationToken)) { return Run(func, default(T), millisecondTimeout, options, null, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, int millisecondTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, millisecondTimeout, TaskCreationOptions.None, null, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, int millisecondTimeout, Action onTimeout, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, millisecondTimeout, TaskCreationOptions.None, onTimeout, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, int millisecondTimeout, TaskCreationOptions options, CancellationToken token = default(CancellationToken)) { return Run(func, defaultValue, millisecondTimeout, options, null, token); }
	public static T Run<T>([NotNull] Func<T> func, T defaultValue, int millisecondTimeout, TaskCreationOptions options, Action onTimeout, CancellationToken token = default(CancellationToken))
	{
		if (millisecondTimeout < 0) throw new ArgumentOutOfRangeException(nameof(millisecondTimeout));
		token.ThrowIfCancellationRequested();
		Task<T> task = Run(func, options, token).ConfigureAwait();
		if (task.Wait(millisecondTimeout)) return task.Result;
		token.ThrowIfCancellationRequested();
		if (onTimeout == null) throw new TimeoutException();
		onTimeout();
		return defaultValue;
	}

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
	public static Task Run([NotNull] Action action, TaskCreationOptions options, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		TaskCreationOptions opt = TaskCreationOptions.None;

		if (options != TaskCreationOptions.None)
		{
			if ((options & TaskCreationOptions.LongRunning) == TaskCreationOptions.LongRunning) opt |= TaskCreationOptions.LongRunning;
			if ((options & TaskCreationOptions.PreferFairness) == TaskCreationOptions.PreferFairness) opt |= TaskCreationOptions.PreferFairness;
			if ((options & TaskCreationOptions.HideScheduler) == TaskCreationOptions.HideScheduler) opt |= TaskCreationOptions.HideScheduler;
			if ((options & TaskCreationOptions.RunContinuationsAsynchronously) == TaskCreationOptions.RunContinuationsAsynchronously) opt |= TaskCreationOptions.RunContinuationsAsynchronously;
			if (opt == TaskCreationOptions.None) return Task.Run(action, token);
		}

		opt |= TaskCreationOptions.DenyChildAttach;
		return Task.Factory.StartNew(action, token, opt, Task.Factory.Scheduler ?? TaskScheduler.Default);
	}

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
	public static Task<T> Run<T>([NotNull] Func<T> func, TaskCreationOptions options, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		TaskCreationOptions opt = TaskCreationOptions.None;

		if (options != TaskCreationOptions.None)
		{
			if ((options & TaskCreationOptions.LongRunning) == TaskCreationOptions.LongRunning) opt |= TaskCreationOptions.LongRunning;
			if ((options & TaskCreationOptions.PreferFairness) == TaskCreationOptions.PreferFairness) opt |= TaskCreationOptions.PreferFairness;
			if ((options & TaskCreationOptions.HideScheduler) == TaskCreationOptions.HideScheduler) opt |= TaskCreationOptions.HideScheduler;
			if ((options & TaskCreationOptions.RunContinuationsAsynchronously) == TaskCreationOptions.RunContinuationsAsynchronously) opt |= TaskCreationOptions.RunContinuationsAsynchronously;
			if (opt == TaskCreationOptions.None) return Task.Run(func, token);
		}

		opt |= TaskCreationOptions.DenyChildAttach;
		return Task.Factory.StartNew(func, token, opt, Task.Factory.Scheduler ?? TaskScheduler.Default);
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

	[NotNull]
	public static Task SequenceAsync([NotNull] Task action1, [NotNull] Task actions2) { return SequenceAsync(CancellationToken.None, action1, actions2); }
	public static async Task SequenceAsync(CancellationToken token, [NotNull] Task action1, [NotNull] Task action2)
	{
		if (!token.CanBeCanceled)
		{
			await action1;
			await action2;
			return;
		}

		token.ThrowIfCancellationRequested();
		await action1;
		token.ThrowIfCancellationRequested();
		await action2;
	}

	[NotNull]
	public static Task SequenceAsync([NotNull] Task action1, [NotNull] Task actions2, [NotNull] Task actions3) { return SequenceAsync(CancellationToken.None, action1, actions2, actions3); }
	public static async Task SequenceAsync(CancellationToken token, [NotNull] Task action1, [NotNull] Task action2, [NotNull] Task action3)
	{
		if (!token.CanBeCanceled)
		{
			await action1;
			await action2;
			await action3;
			return;
		}

		token.ThrowIfCancellationRequested();
		await action1;
		token.ThrowIfCancellationRequested();
		await action2;
		token.ThrowIfCancellationRequested();
		await action3;
	}

	[NotNull]
	public static Task SequenceAsync([NotNull] Task action1, [NotNull] Task actions2, [NotNull] Task actions3, [NotNull] Task actions4) { return SequenceAsync(CancellationToken.None, action1, actions2, actions3, actions4); }
	public static async Task SequenceAsync(CancellationToken token, [NotNull] Task action1, [NotNull] Task action2, [NotNull] Task action3, [NotNull] Task action4)
	{
		if (!token.CanBeCanceled)
		{
			await action1;
			await action2;
			await action3;
			await action4;
			return;
		}

		token.ThrowIfCancellationRequested();
		await action1;
		token.ThrowIfCancellationRequested();
		await action2;
		token.ThrowIfCancellationRequested();
		await action3;
		token.ThrowIfCancellationRequested();
		await action4;
	}

	[NotNull]
	public static Task SequenceAsync([NotNull] Task action1, [NotNull] Task actions2, [NotNull] Task actions3, [NotNull] Task actions4, [NotNull] Task actions5) { return SequenceAsync(CancellationToken.None, action1, actions2, actions3, actions4, actions5); }
	public static async Task SequenceAsync(CancellationToken token, [NotNull] Task action1, [NotNull] Task action2, [NotNull] Task action3, [NotNull] Task action4, [NotNull] Task action5)
	{
		if (!token.CanBeCanceled)
		{
			await action1;
			await action2;
			await action3;
			await action4;
			await action5;
			return;
		}

		token.ThrowIfCancellationRequested();
		await action1;
		token.ThrowIfCancellationRequested();
		await action2;
		token.ThrowIfCancellationRequested();
		await action3;
		token.ThrowIfCancellationRequested();
		await action4;
		token.ThrowIfCancellationRequested();
		await action5;
	}

	[NotNull]
	public static Task SequenceAsync([NotNull] Task action, [NotNull] params Task[] actions) { return SequenceAsync(CancellationToken.None, action, actions); }
	public static async Task SequenceAsync(CancellationToken token, [NotNull] Task action, [NotNull] params Task[] actions)
	{
		token.ThrowIfCancellationRequested();

		foreach (Task act in actions)
		{
			if (act != null) continue;
			throw new NullReferenceException($"{nameof(actions)} contains a null reference.");
		}

		await action;

		foreach (Task act in actions)
		{
			token.ThrowIfCancellationRequested();
			await act;
		}
	}

	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>([NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2) { return SequenceAsync(CancellationToken.None, null, function1, function2); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2) { return SequenceAsync(CancellationToken.None, predicate, function1, function2); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2) { return SequenceAsync(token, null, function1, function2); }
	public static async Task<TResult> SequenceAsync<TResult>(CancellationToken token, Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2)
	{
		TResult result;

		if (!token.CanBeCanceled)
		{
			result = await function1;
			return predicate == null
						? await function2(result)
						: !predicate(result)
							? result
							: await function2(result);
		}

		token.ThrowIfCancellationRequested();
		result = await function1;
		token.ThrowIfCancellationRequested();
		if (predicate == null) return await function2(result);
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		return await function2(result);
	}

	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>([NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3) { return SequenceAsync(CancellationToken.None, null, function1, function2, function3); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3) { return SequenceAsync(CancellationToken.None, predicate, function1, function2, function3); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3) { return SequenceAsync(token, null, function1, function2, function3); }
	public static async Task<TResult> SequenceAsync<TResult>(CancellationToken token, Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3)
	{
		TResult result;

		if (!token.CanBeCanceled)
		{
			result = await function1;

			if (predicate == null)
			{
				result = await function2(result);
				return await function3(result);
			}

			if (!predicate(result)) return result;
			result = await function2(result);
			return !predicate(result)
						? result
						: await function3(result);
		}

		token.ThrowIfCancellationRequested();
		result = await function1;
		token.ThrowIfCancellationRequested();

		if (predicate == null)
		{
			result = await function2(result);
			token.ThrowIfCancellationRequested();
			return await function3(result);
		}

		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		result = await function2(result);
		token.ThrowIfCancellationRequested();
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		return await function3(result);
	}

	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>([NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4) { return SequenceAsync(CancellationToken.None, null, function1, function2, function3, function4); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4) { return SequenceAsync(CancellationToken.None, predicate, function1, function2, function3, function4); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4) { return SequenceAsync(token, null, function1, function2, function3, function4); }
	public static async Task<TResult> SequenceAsync<TResult>(CancellationToken token, Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4)
	{
		TResult result;

		if (!token.CanBeCanceled)
		{
			result = await function1;

			if (predicate == null)
			{
				result = await function2(result);
				result = await function3(result);
				return await function4(result);
			}

			if (!predicate(result)) return result;
			result = await function2(result);
			if (!predicate(result)) return result;
			result = await function3(result);
			return !predicate(result)
						? result
						: await function4(result);
		}

		token.ThrowIfCancellationRequested();
		result = await function1;
		token.ThrowIfCancellationRequested();

		if (predicate == null)
		{
			result = await function2(result);
			token.ThrowIfCancellationRequested();
			result = await function3(result);
			token.ThrowIfCancellationRequested();
			return await function4(result);
		}

		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		result = await function2(result);
		token.ThrowIfCancellationRequested();
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		result = await function3(result);
		token.ThrowIfCancellationRequested();
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		return await function4(result);
	}

	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>([NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4, [NotNull] Func<TResult, Task<TResult>> function5) { return SequenceAsync(CancellationToken.None, null, function1, function2, function3, function4, function5); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4, [NotNull] Func<TResult, Task<TResult>> function5) { return SequenceAsync(CancellationToken.None, predicate, function1, function2, function3, function4, function5); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4, [NotNull] Func<TResult, Task<TResult>> function5) { return SequenceAsync(token, null, function1, function2, function3, function4, function5); }
	public static async Task<TResult> SequenceAsync<TResult>(CancellationToken token, Predicate<TResult> predicate, [NotNull] Task<TResult> function1, [NotNull] Func<TResult, Task<TResult>> function2, [NotNull] Func<TResult, Task<TResult>> function3, [NotNull] Func<TResult, Task<TResult>> function4, [NotNull] Func<TResult, Task<TResult>> function5)
	{
		TResult result;

		if (!token.CanBeCanceled)
		{
			result = await function1;

			if (predicate == null)
			{
				result = await function2(result);
				result = await function3(result);
				result = await function4(result);
				return await function5(result);
			}

			if (!predicate(result)) return result;
			result = await function2(result);
			if (!predicate(result)) return result;
			result = await function3(result);
			if (!predicate(result)) return result;
			result = await function4(result);
			return !predicate(result)
						? result
						: await function5(result);
		}

		token.ThrowIfCancellationRequested();
		result = await function1;
		token.ThrowIfCancellationRequested();

		if (predicate == null)
		{
			result = await function2(result);
			token.ThrowIfCancellationRequested();
			result = await function3(result);
			token.ThrowIfCancellationRequested();
			result = await function4(result);
			token.ThrowIfCancellationRequested();
			return await function5(result);
		}

		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		result = await function2(result);
		token.ThrowIfCancellationRequested();
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		result = await function3(result);
		token.ThrowIfCancellationRequested();
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		result = await function4(result);
		token.ThrowIfCancellationRequested();
		if (!predicate(result)) return result;
		token.ThrowIfCancellationRequested();
		return await function5(result);
	}

	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>([NotNull] Task<TResult> function, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, null, function, functions); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(Predicate<TResult> predicate, [NotNull] Task<TResult> function, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(CancellationToken.None, predicate, function, functions); }
	[NotNull]
	public static Task<TResult> SequenceAsync<TResult>(CancellationToken token, [NotNull] Task<TResult> function, [NotNull] params Func<TResult, Task<TResult>>[] functions) { return SequenceAsync(token, null, function, functions); }
	public static async Task<TResult> SequenceAsync<TResult>(CancellationToken token, Predicate<TResult> predicate, [NotNull] Task<TResult> function, [NotNull] params Func<TResult, Task<TResult>>[] functions)
	{
		token.ThrowIfCancellationRequested();
			
		foreach (Func<TResult, Task<TResult>> func in functions)
		{
			if (func != null) continue;
			throw new NullReferenceException($"{nameof(functions)} contains a null reference.");
		}

		TResult result = await function;

		if (predicate != null)
		{
			if (predicate(result)) return result;

			foreach (Func<TResult, Task<TResult>> func in functions)
			{
				token.ThrowIfCancellationRequested();
				result = await func(result);
				if (predicate(result)) break;
			}
		}
		else
		{
			foreach (Func<TResult, Task<TResult>> func in functions)
			{
				token.ThrowIfCancellationRequested();
				result = await func(result);
			}
		}

		return result;
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
	/// <summary>
	/// Registers a wait task in the thread pool.
	/// <para>Based on <see href="https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Interop.WaitHandles/Interop/WaitHandleAsyncFactory.cs">Stephen Cleary's Nito.AsyncEx.Interop.WaitHandles/Interop.WaitHandleAsyncFactory</see></para>
	/// </summary>
	public static Task<bool> FromWaitHandle([NotNull] WaitHandle handle, int millisecondsTimeout, CancellationToken token)
	{
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			
		bool isSet = handle.WaitOne(0);
		if (isSet) return Task.FromResult(true);
		if (millisecondsTimeout == 0 || token.IsCancellationRequested) return Task.FromResult(false);
		return FromWaitHandleLocal(handle, millisecondsTimeout, token);

		static async Task<bool> FromWaitHandleLocal(WaitHandle handle, int millisecondsTimeout, CancellationToken token)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Other.MarcGravell.Nullable;
using CancellationToken = System.Threading.CancellationToken;

namespace essentialMix.Extensions
{
	public static class TaskExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsReady(this Task thisValue) { return thisValue != null && thisValue.Status.IsReady(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsRunning(this Task thisValue) { return thisValue != null && thisValue.Status.IsRunning(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsStarted(this Task thisValue) { return thisValue != null && thisValue.Status.IsStarted(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsFinished(this Task thisValue) { return thisValue != null && thisValue.Status.IsFinished(); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task ConfigureAwait([NotNull] this Task thisValue)
		{
			thisValue.ConfigureAwait(false);
			return thisValue;
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task<TResult> ConfigureAwait<TResult>([NotNull] this Task<TResult> thisValue)
		{
			thisValue.ConfigureAwait(false);
			return thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void Execute([NotNull] this Task thisValue)
		{
			thisValue.GetAwaiter().GetResult();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TResult Execute<TResult>([NotNull] this Task<TResult> thisValue)
		{
			return thisValue.GetAwaiter().GetResult();
		}

		public static Task Then(this Task thisValue, [NotNull] Func<Task> next, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return thisValue == null || thisValue.IsCanceled || thisValue.IsFaulted
						? thisValue
						: thisValue
						.ContinueWith(_ => next(), token, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default)
						.Unwrap();
		}

		public static Task<TResult> Then<TResult>(this Task<TResult> thisValue, [NotNull] Func<Task<TResult>, Task<TResult>> next, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return thisValue == null || thisValue.IsCanceled || thisValue.IsFaulted
						? thisValue
						: thisValue
						.ContinueWith(next, token, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default)
						.Unwrap();
		}

		public static Task WithCancellation(this Task thisValue, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			if (thisValue == null || thisValue.IsFinished())
			{
				// Either the task has already completed or timeout will never occur.
				return thisValue;
			}

			return WithCancellationLocal(thisValue, token);

			static async Task WithCancellationLocal(Task task, CancellationToken token)
			{
				TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
				IDisposable tokenRegistration = null;

				try
				{
					if (token.CanBeCanceled) tokenRegistration = token.Register(state => ((TaskCompletionSource<object>)state).TrySetCanceled(), tcs, false);
					Task finishedTask = await Task.WhenAny(task, tcs.Task);
					if (finishedTask != tcs.Task) return;
					throw new OperationCanceledException(token);
				}
				finally
				{
					ObjectHelper.Dispose(ref tokenRegistration);
				}
			}
		}

		public static Task<TResult> WithCancellation<TResult>(this Task<TResult> thisValue, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			if (thisValue == null || thisValue.IsCanceled || thisValue.IsFaulted || thisValue.IsCompleted)
			{
				// Either the task has already completed or timeout will never occur.
				// No proxy necessary.
				return thisValue;
			}

			return WithCancellationLocal(thisValue, token);

			static async Task<TResult> WithCancellationLocal(Task<TResult> task, CancellationToken token)
			{
				TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
				IDisposable tokenRegistration = null;

				try
				{
					if (token.CanBeCanceled) tokenRegistration = token.Register(state => ((TaskCompletionSource<TResult>)state).TrySetCanceled(), tcs, false);
					Task<TResult> finishedTask = await Task.WhenAny(task, tcs.Task);
					if (finishedTask != tcs.Task) return finishedTask.Result;
					throw new OperationCanceledException(token);
				}
				finally
				{
					ObjectHelper.Dispose(ref tokenRegistration);
				}
			}
		}

		public static Task<T> WithResult<T>(this Task thisValue, T value, CancellationToken token = default(CancellationToken))
		{
			if (thisValue == null) return null;

			if (thisValue.IsFaulted)
			{
				TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
				
				if (thisValue.Exception != null)
					tcs.SetException(thisValue.Exception.InnerExceptions);
				else
					tcs.SetException(new Exception("Unknown error."));

				return tcs.Task;
			}

			if (thisValue.IsCanceled) return Task.FromCanceled<T>(token);
			return thisValue.IsCompleted
						? Task.FromResult(value)
						: thisValue.ContinueWith(_ => value, token);
		}

		public static Task<T> WithResult<T>(this Task thisValue, [NotNull] Func<T> getValue, CancellationToken token = default(CancellationToken))
		{
			if (thisValue == null) return null;
			if (thisValue.IsCanceled) return Task.FromCanceled<T>(token);
			if (thisValue.IsFaulted) return Task.FromException<T>(thisValue.Exception ?? new Exception("Unknown error."));
			if (thisValue.IsCompleted) return Task.FromResult(getValue());
			return thisValue.ContinueWith(_ => getValue(), token);
		}

		public static Task TimeoutAfter(this Task thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return TimeoutAfter(thisValue, timeout.TotalIntMilliseconds(), token); }
		/// <summary>
		/// https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/
		/// </summary>
		public static Task TimeoutAfter(this Task thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			token.ThrowIfCancellationRequested();
			if (thisValue == null) return null;

			// Short-circuit: infinite timeout or task already completed/faulted
			if (thisValue.IsFinished() || millisecondsTimeout < 0)
			{
				// Either the task has already completed/faulted or timeout will never occur.
				return thisValue;
			}

			// tcs.Task will be returned as a proxy to the caller
			TaskCompletionSource<VoidTypeStruct> tcs = new TaskCompletionSource<VoidTypeStruct>(TaskCreationOptions.RunContinuationsAsynchronously);

			// Short-circuit #2: zero timeout
			if (millisecondsTimeout == 0)
			{
				// We've already timed out.
				tcs.SetException(new TimeoutException());
				return tcs.Task;
			}

			// Set up a timer to complete after the specified timeout period
			Timer timer = new Timer(state =>
			{
				// Recover your state information
				TaskCompletionSource<VoidTypeStruct> tcsTimer = (TaskCompletionSource<VoidTypeStruct>)state;

				// Fault our proxy with a TimeoutException
				tcsTimer.TrySetException(new TimeoutException());
			}, tcs, millisecondsTimeout, TimeSpanHelper.INFINITE);

			// Wire up the logic for what happens when source task completes
			thisValue.ContinueWith((antecedent, state) =>
			{
				if (token.IsCancellationRequested) return;
				// Recover our state data
				(Timer Timer, TaskCompletionSource<VoidTypeStruct> CompletionSource) tuple = (ValueTuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

				// Cancel the Timer
				ObjectHelper.Dispose(tuple.Timer);
				// Marshal results to proxy
				MarshalTaskResults(antecedent, tuple.CompletionSource);
			}, (timer, tcs), token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

			return tcs.Task;
		}

		public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return TimeoutAfter(thisValue, timeout.TotalIntMilliseconds(), token); }
		public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			token.ThrowIfCancellationRequested();
			if (thisValue == null) return null;

			// Short-circuit #1: infinite timeout or task already completed/faulted
			if (thisValue.IsFinished() || millisecondsTimeout < 0)
			{
				// Either the task has already completed/faulted or timeout will never occur.
				// No proxy necessary.
				return thisValue;
			}

			// tcs.Task will be returned as a proxy to the caller
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

			// Short-circuit #2: zero timeout
			if (millisecondsTimeout == 0)
			{
				// We've already timed out.
				tcs.SetException(new TimeoutException());
				return tcs.Task;
			}

			// Set up a timer to complete after the specified timeout period
			Timer timer = new Timer(state =>
			{
				// Recover your state information
				TaskCompletionSource<TResult> tcsTimer = (TaskCompletionSource<TResult>)state;

				// Fault our proxy with a TimeoutException
				tcsTimer.TrySetException(new TimeoutException());
			}, tcs, millisecondsTimeout, TimeSpanHelper.INFINITE);

			// Wire up the logic for what happens when source task completes
			thisValue.ContinueWith((antecedent, state) =>
			{
				// Recover our state data
				(Timer Timer, TaskCompletionSource<TResult> CompletionSource) tuple = (ValueTuple<Timer, TaskCompletionSource<TResult>>)state;

				// Cancel the Timer
				ObjectHelper.Dispose(tuple.Timer);
				// Marshal results to proxy
				MarshalTaskResults(antecedent, tuple.CompletionSource);
			}, (timer, tcs), token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

			return tcs.Task;
		}

		public static bool WaitSilently(this Task thisValue, CancellationToken token = default(CancellationToken)) { return WaitSilently(thisValue, TimeSpanHelper.INFINITE, token); }
		public static bool WaitSilently(this Task thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return WaitSilently(thisValue, timeout.TotalIntMilliseconds(), token); }
		public static bool WaitSilently(this Task thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested) return false;

			// Short-circuit #2: task already completed/faulted
			if (thisValue == null || thisValue.IsCanceled || thisValue.IsFaulted) return false;
			if (thisValue.IsCompleted) return true;

			try
			{
				bool result;

				if (millisecondsTimeout == TimeSpanHelper.INFINITE)
				{
					thisValue.Wait(token);
					result = true;
				}
				else
				{
					result = thisValue.Wait(millisecondsTimeout, token);
				}

				return !token.IsCancellationRequested && result && thisValue.IsCompleted;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		public static bool WaitAllSilently([NotNull] this IList<Task> thisValue, CancellationToken token = default(CancellationToken)) { return WaitAllSilently(thisValue, TimeSpanHelper.INFINITE, token); }
		public static bool WaitAllSilently([NotNull] this IList<Task> thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return WaitAllSilently(thisValue, timeout.TotalIntMilliseconds(), token); }
		public static bool WaitAllSilently([NotNull] this IList<Task> thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested) return false;

			// Short-circuit #2: task already completed/faulted
			int i = 0, count = thisValue.Count;
			bool hasNull = false;

			foreach (Task task in thisValue)
			{
				if (task == null)
				{
					hasNull = true;
					count--;
					continue;
				}

				if (task.IsCanceled || task.IsFaulted) return false;
				if (task.IsCompleted) i++;
			}

			if (i == count) return true;

			Task[] tasks = hasNull || !thisValue.GetType().IsArray
								? thisValue.Where(e => e != null).ToArray()
								: (Task[])thisValue;
			try
			{
				bool result;

				if (millisecondsTimeout == TimeSpanHelper.INFINITE)
				{
					Task.WaitAll(tasks);
					result = true;
				}
				else
				{
					result = Task.WaitAll(tasks, millisecondsTimeout, token);
				}

				return !token.IsCancellationRequested && result;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		public static int WaitAnySilently([NotNull] this IList<Task> thisValue, CancellationToken token = default(CancellationToken)) { return WaitAnySilently(thisValue, TimeSpanHelper.INFINITE, token); }
		public static int WaitAnySilently([NotNull] this IList<Task> thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return WaitAnySilently(thisValue, timeout.TotalIntMilliseconds(), token); }
		public static int WaitAnySilently([NotNull] this IList<Task> thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested) return -1;

			// Short-circuit #2: task already completed/faulted
			for (int i = 0; i < thisValue.Count; i++)
			{
				Task task = thisValue[i];
				if (task == null || task.IsCompleted || task.IsCanceled || task.IsFaulted) return i;
			}

			Task[] tasks = thisValue.GetType().IsArray
								? (Task[])thisValue
								: thisValue.ToArray();

			try
			{
				return millisecondsTimeout == TimeSpanHelper.INFINITE
							? Task.WaitAny(tasks, token)
							: Task.WaitAny(tasks, millisecondsTimeout, token);
			}
			catch (OperationCanceledException)
			{
				return -1;
			}
			catch (TimeoutException)
			{
				return -1;
			}
		}

		public static bool WaitAll([NotNull] this IList<Task> thisValue, CancellationToken token = default(CancellationToken)) { return WaitAll(thisValue, TimeSpanHelper.INFINITE, token); }
		public static bool WaitAll([NotNull] this IList<Task> thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return WaitAll(thisValue, timeout.TotalIntMilliseconds(), token); }
		public static bool WaitAll([NotNull] this IList<Task> thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested) return false;

			// Short-circuit #2: task already completed/faulted
			int i = 0, count = thisValue.Count;
			bool hasNull = false;

			foreach (Task task in thisValue)
			{
				if (task == null)
				{
					hasNull = true;
					count--;
					continue;
				}

				if (task.IsCanceled || task.IsFaulted) return false;
				if (task.IsCompleted) i++;
			}

			if (i == count) return true;

			Task[] tasks = hasNull || !thisValue.GetType().IsArray
								? thisValue.Where(e => e != null).ToArray()
								: (Task[])thisValue;
			bool result;

			if (millisecondsTimeout == TimeSpanHelper.INFINITE)
			{
				Task.WaitAll(tasks, token);
				result = true;
			}
			else
			{
				result = Task.WaitAll(tasks, millisecondsTimeout, token);
			}

			return result;
		}

		[NotNull]
		public static Task WhenAll([NotNull] this Task[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled(token)
						: Task.WhenAll(thisValue);
		}

		[NotNull]
		public static Task<TResult[]> WhenAll<TResult>([NotNull] this Task<TResult>[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<TResult[]>(token)
						: Task.WhenAll(thisValue);
		}

		[NotNull]
		public static Task<Task> WhenAny([NotNull] this Task[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<Task>(token)
						: Task.WhenAny(thisValue);
		}

		[NotNull]
		public static Task<Task<TResult>> WhenAny<TResult>([NotNull] this Task<TResult>[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<Task<TResult>>(token)
						: Task.WhenAny(thisValue);
		}

		public static object GetResult(this Task thisValue)
		{
			if (thisValue == null) return null;

			Type type = thisValue.GetType();
			if (!type.IsGenericType) return null;

			Type[] types = type.GetGenericArguments();
			if (types.Length == 0 || types[0].Name == "VoidTaskResult") return null;
			return ((dynamic)thisValue).Result;
		}

		public static Task<TU> As<T, TU>(this Task<T> thisValue, CancellationToken token = default(CancellationToken))
			where T : TU
		{
			if (thisValue == null) return null;
			TaskCompletionSource<TU> tcs = new TaskCompletionSource<TU>(TaskCreationOptions.RunContinuationsAsynchronously);
			thisValue.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					if (t.Exception != null)
						tcs.SetException(t.Exception.InnerExceptions);
					else
						tcs.SetException(new Exception("Unknown error."));
				}
				else if (t.IsCanceled)
				{
					tcs.SetCanceled();
				}
				else
				{
					tcs.SetResult(t.Result);
				}
			}, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
			return tcs.Task;
		}

		private static void MarshalTaskResults<TResult>([NotNull] Task source, TaskCompletionSource<TResult> proxy)
		{
			switch (source.Status)
			{
				case TaskStatus.Faulted:
					if (source.Exception != null) proxy.TrySetException(source.Exception.InnerException ?? source.Exception);
					break;
				case TaskStatus.Canceled:
					proxy.TrySetCanceled();
					break;
				case TaskStatus.RanToCompletion:
					proxy.TrySetResult(source is not Task<TResult> task
											? default(TResult) // source is a Task
											: task.Result); // source is a Task<TResult>
					break;
			}
		}
	}
}
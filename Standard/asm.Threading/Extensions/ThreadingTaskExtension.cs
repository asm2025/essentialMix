using System;
using System.Threading;
using System.Threading.Tasks;
using asm.Helpers;
using Other.MarcGravell.Nullable;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ThreadingTaskExtension
	{
		public static bool IsReady([NotNull] this Task thisValue) { return thisValue.Status.IsReady(); }

		public static bool IsRunning([NotNull] this Task thisValue) { return thisValue.Status.IsRunning(); }

		public static bool IsStarted([NotNull] this Task thisValue) { return thisValue.Status.IsStarted(); }

		public static bool IsFinished([NotNull] this Task thisValue) { return thisValue.Status.IsFinished(); }

		public static Task Then([NotNull] this Task thisValue, [NotNull] Func<Task> next, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return thisValue
					.ContinueWith(_ => next(), token, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default)
					.Unwrap();
		}

		public static Task<TResult> Then<TResult>([NotNull] this Task<TResult> thisValue, [NotNull] Func<Task<TResult>, Task<TResult>> next, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return thisValue
					.ContinueWith(next, token, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default)
					.Unwrap();
		}

		[NotNull]
		public static Task WithCancellation([NotNull] this Task thisValue, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			if (thisValue.IsCompleted || thisValue.IsCanceled)
			{
				// Either the task has already completed or timeout will never occur.
				// No proxy necessary.
				return thisValue;
			}

			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			if (token.CanBeCanceled) token.Register(() => tcs.TrySetResult(null));
			if (Task.WhenAny(thisValue, tcs.Task).ConfigureAwait().Result == tcs.Task) throw new OperationCanceledException(token);
			return thisValue;
		}

		[NotNull]
		public static Task<TResult> WithCancellation<TResult>([NotNull] this Task<TResult> thisValue, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			if (thisValue.IsCompleted || thisValue.IsCanceled)
			{
				// Either the task has already completed or timeout will never occur.
				// No proxy necessary.
				return thisValue;
			}

			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			if (token.CanBeCanceled) token.Register(() => tcs.TrySetResult(default));
			if (Task.WhenAny(thisValue, tcs.Task).ConfigureAwait().Result == tcs.Task) throw new OperationCanceledException(token);
			return thisValue;
		}

		[NotNull]
		public static Task<T> WithResult<T>([NotNull] this Task thisValue, T value, CancellationToken token = default(CancellationToken)) { return thisValue.ContinueWith(t => value, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default); }

		public static Task TimeoutAfter([NotNull] this Task thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return TimeoutAfter(thisValue, timeout.TotalIntMilliseconds(), token);
		}

		/// <summary>
		/// https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/
		/// </summary>
		public static Task TimeoutAfter([NotNull] this Task thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE)
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			token.ThrowIfCancellationRequested();

			// Short-circuit: infinite timeout or task already completed/faulted
			if (thisValue.IsFinished() || millisecondsTimeout < 0)
			{
				// Either the task has already completed/faulted or timeout will never occur.
				// No proxy necessary.
				return thisValue;
			}

			// tcs.Task will be returned as a proxy to the caller
			TaskCompletionSource<VoidTypeStruct> tcs = new TaskCompletionSource<VoidTypeStruct>();

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
				if (token.IsCancellationRequested)
					return;
				// Recover our state data
				(Timer Timer, TaskCompletionSource<VoidTypeStruct> CompletionSource) tuple = (ValueTuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

				// Cancel the Timer
				ObjectHelper.Dispose(tuple.Timer);
				// Marshal results to proxy
				MarshalTaskResults(antecedent, tuple.CompletionSource);
			}, (timer, tcs), token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

			return tcs.Task;
		}

		public static Task<TResult> TimeoutAfter<TResult>([NotNull] this Task<TResult> thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return TimeoutAfter(thisValue, timeout.TotalIntMilliseconds(), token);
		}

		public static Task<TResult> TimeoutAfter<TResult>([NotNull] this Task<TResult> thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE)
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			token.ThrowIfCancellationRequested();

			// Short-circuit #1: infinite timeout or task already completed/faulted
			if (thisValue.IsFinished() || millisecondsTimeout < 0)
			{
				// Either the task has already completed/faulted or timeout will never occur.
				// No proxy necessary.
				return thisValue;
			}

			// tcs.Task will be returned as a proxy to the caller
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();

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
			}, (timer, tcs), token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

			return tcs.Task;
		}

		public static bool WaitSilently([NotNull] this Task thisValue, CancellationToken token = default(CancellationToken)) { return WaitSilently(thisValue, TimeSpanHelper.INFINITE, token); }

		public static bool WaitSilently([NotNull] this Task thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitSilently(thisValue, timeout.TotalIntMilliseconds(), token);
		}

		public static bool WaitSilently([NotNull] this Task thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE)
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested)
				return false;

			// Short-circuit #2: task already completed/faulted
			if (thisValue.IsCanceled || thisValue.IsFaulted)
				return false;
			if (thisValue.IsCompleted)
				return true;

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
			catch
			{
				return false;
			}
		}

		public static bool WaitSilently([NotNull] this Task[] thisValue, CancellationToken token = default(CancellationToken)) { return WaitSilently(thisValue, TimeSpanHelper.INFINITE, token); }

		public static bool WaitSilently([NotNull] this Task[] thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitSilently(thisValue, timeout.TotalIntMilliseconds(), token);
		}

		public static bool WaitSilently([NotNull] this Task[] thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE)
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested) return false;

			// Short-circuit #2: task already completed/faulted
			int i = 0;

			foreach (Task task in thisValue)
			{
				if (task.IsCanceled || task.IsFaulted) return false;
				if (task.IsCompleted) i++;
			}

			if (i == thisValue.Length) return true;

			try
			{
				bool result;

				if (millisecondsTimeout == TimeSpanHelper.INFINITE)
				{
					Task.WaitAll(thisValue, token);
					result = true;
				}
				else
				{
					result = Task.WaitAll(thisValue, millisecondsTimeout, token);
				}

				return !token.IsCancellationRequested && result;
			}
			catch
			{
				return false;
			}
		}

		public static bool WaitAll([NotNull] this Task[] thisValue, CancellationToken token = default(CancellationToken)) { return WaitAll(thisValue, TimeSpanHelper.INFINITE, token); }

		public static bool WaitAll([NotNull] this Task[] thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitAll(thisValue, timeout.TotalIntMilliseconds(), token);
		}

		public static bool WaitAll([NotNull] this Task[] thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE)
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			// Short-circuit #1: already cancelled
			if (token.IsCancellationRequested)
				return false;

			// Short-circuit #2: task already completed/faulted
			int i = 0;

			foreach (Task task in thisValue)
			{
				if (task.IsCanceled || task.IsFaulted)
					return false;
				if (task.IsCompleted)
					i++;
			}

			if (i == thisValue.Length)
				return true;

			bool result;

			if (millisecondsTimeout == TimeSpanHelper.INFINITE)
			{
				Task.WaitAll(thisValue, token);
				result = true;
			}
			else
			{
				result = Task.WaitAll(thisValue, millisecondsTimeout, token);
			}

			return result;
		}

		[NotNull]
		public static Task WhenAll([NotNull] this Task[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled(token)
						: Task.WhenAll(thisValue).ConfigureAwait();
		}

		[NotNull]
		public static Task<TResult[]> WhenAll<TResult>([NotNull] this Task<TResult>[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<TResult[]>(token)
						: Task.WhenAll(thisValue).ConfigureAwait();
		}

		[NotNull]
		public static Task<Task> WhenAny([NotNull] this Task[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<Task>(token)
						: Task.WhenAny(thisValue).ConfigureAwait();
		}

		[NotNull]
		public static Task<Task<TResult>> WhenAny<TResult>([NotNull] this Task<TResult>[] thisValue, CancellationToken token = default(CancellationToken))
		{
			return token.IsCancellationRequested
						? Task.FromCanceled<Task<TResult>>(token)
						: Task.WhenAny(thisValue).ConfigureAwait();
		}

		public static object GetResult([NotNull] this Task thisValue)
		{
			Type type = thisValue.GetType();
			if (!type.IsGenericType)
				return null;

			Type[] types = type.GetGenericArguments();
			if (types.Length == 0 || types[0].Name == "VoidTaskResult") return null;
			return ((dynamic)thisValue).Result;
		}

		public static Task<TU> As<T, TU>([NotNull] this Task<T> thisValue)
			where T : TU
		{
			TaskCompletionSource<TU> tcs = new TaskCompletionSource<TU>();
			thisValue.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					if (t.Exception != null)
					{
						tcs.SetException(t.Exception.InnerExceptions);
					}
					else
					{
						tcs.SetException(new Exception("Unknown error."));
					}
				}
				else if (t.IsCanceled)
				{
					tcs.SetCanceled();
				}
				else
				{
					tcs.SetResult(t.Result);
				}
			}, TaskContinuationOptions.ExecuteSynchronously);
			return tcs.Task;
		}

		internal static void MarshalTaskResults<TResult>([NotNull] Task source, TaskCompletionSource<TResult> proxy)
		{
			switch (source.Status)
			{
				case TaskStatus.Faulted:
					proxy.TrySetException(source.Exception?.InnerException ?? source.Exception ?? new Exception("Unknown error."));
					break;
				case TaskStatus.Canceled:
					proxy.TrySetCanceled();
					break;
				case TaskStatus.RanToCompletion:
					proxy.TrySetResult(!(source is Task<TResult> task)
						? default(TResult) // source is a Task
						: task.Result); // source is a Task<TResult>
					break;
			}
		}
	}
}
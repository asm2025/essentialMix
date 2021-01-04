using System;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Object;
using asm.Threading.Collections.ProducerConsumer.Queue;
using JetBrains.Annotations;

namespace asm.Threading.Collections.ProducerConsumer
{
	/*
	 * This is based on the insightful book of Joseph Albahari, C# 6 in a Nutshell
	 * http://www.albahari.com/threading/
	 */
	public abstract class ProducerConsumerQueue<T> : Disposable, IProducerConsumer<T>
	{
		private CancellationTokenSource _cts;

		private volatile int _completeMarked;

		protected ProducerConsumerQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		{
			Threads = options.Threads;
			ExecuteCallback = options.ExecuteCallback;
			ResultCallback = options.ResultCallback;
			FinalizeCallback = options.FinalizeCallback;
			_cts = new CancellationTokenSource();
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			Token = _cts.Token;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CompleteInternal();
				StopInternal(!WaitOnDispose);
				ObjectHelper.Dispose(ref _cts);
			}
			base.Dispose(disposing);
		}

		public event EventHandler WorkStarted;
		public event EventHandler WorkCompleted;

		public abstract int Count { get; }

		public bool WaitOnDispose { get; set; } = true;

		public int Threads { get; }

		public CancellationToken Token { get; }

		public abstract bool IsBusy { get; }

		public bool CompleteMarked
		{
			get => _completeMarked != 0;
			protected set =>
				Interlocked.CompareExchange(ref _completeMarked, value
																	? 1
																	: 0, _completeMarked);
		}

		public int SleepAfterEnqueue { get; set; } = TimeSpanHelper.INFINITE;

		[NotNull]
		protected Func<T, TaskResult> ExecuteCallback { get; }

		protected Func<T, TaskResult, Exception, bool> ResultCallback { get; }

		protected Action<T> FinalizeCallback { get; }

		public void Stop()
		{
			ThrowIfDisposed();
			StopInternal(!WaitOnDispose);
		}

		public void Stop(bool enforce)
		{
			ThrowIfDisposed();
			StopInternal(enforce);
		}

		public void Enqueue(T item)
		{
			ThrowIfDisposed();
			if (CompleteMarked) throw new InvalidOperationException("Completion marked.");
			EnqueueInternal(item);
			if (SleepAfterEnqueue > 0) TimeSpanHelper.WasteTime(SleepAfterEnqueue, Token);
		}

		public void Complete()
		{
			ThrowIfDisposed();
			if (CompleteMarked) return;
			CompleteInternal();
		}

		public bool Wait()
		{
			ThrowIfDisposed();
			return WaitInternal(TimeSpanHelper.INFINITE);
		}

		public bool Wait(TimeSpan timeout)
		{
			ThrowIfDisposed();
			return WaitInternal(timeout.TotalIntMilliseconds());
		}

		public bool Wait(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			return WaitInternal(millisecondsTimeout);
		}

		[NotNull] 
		public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }

		[NotNull] 
		public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }

		[NotNull]
		public Task<bool> WaitAsync(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			return Task.Run(() => WaitInternal(millisecondsTimeout), Token);
		}

		public void Clear()
		{
			ThrowIfDisposed();
			ClearInternal();
		}

		protected abstract void EnqueueInternal([NotNull] T item);

		protected abstract void CompleteInternal();

		protected abstract void ClearInternal();

		protected abstract bool WaitInternal(int millisecondsTimeout);

		protected abstract void StopInternal(bool enforce);

		protected virtual void OnWorkStarted(EventArgs args)
		{
			WorkStarted?.Invoke(this, args);
		}

		protected virtual void OnWorkCompleted(EventArgs args)
		{
			WorkCompleted?.Invoke(this, args);
		}

		protected virtual void Run([NotNull] T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			try
			{
				TaskResult result = ExecuteCallback(item);
				ResultCallback?.Invoke(item, result, null);
			}
			catch (TimeoutException)
			{
				ResultCallback?.Invoke(item, TaskResult.Timeout, null);
			}
			catch (AggregateException aggTimeout) when(aggTimeout.InnerException is TimeoutException)
			{
				ResultCallback?.Invoke(item, TaskResult.Timeout, null);
			}
			catch (OperationCanceledException)
			{
				ResultCallback?.Invoke(item, TaskResult.Canceled, null);
			}
			catch (AggregateException aggCanceled) when(aggCanceled.InnerException is OperationCanceledException)
			{
				ResultCallback?.Invoke(item, TaskResult.Canceled, null);
			}
			catch (Exception e)
			{
				ResultCallback?.Invoke(item, TaskResult.Error, e);
			}
			finally
			{
				FinalizeCallback?.Invoke(item);
			}
		}

		protected void Cancel()
		{
			_cts.CancelIfNotDisposed();
		}
	}

	public static class ProducerConsumerQueue
	{
		[NotNull]
		public static IProducerConsumer<T> Create<T>(ThreadQueueMode mode, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			return mode switch
			{
				ThreadQueueMode.Task => new TaskQueue<T>(options, token),
				ThreadQueueMode.DataFlow => new DataFlowQueue<T>(options, token),
				ThreadQueueMode.WaitAndPulse => new WaitAndPulseQueue<T>(options, token),
				ThreadQueueMode.Event => new EventQueue<T>(options, token),
				ThreadQueueMode.BlockingCollection => new BlockingCollectionQueue<T>(options, token),
				ThreadQueueMode.TaskGroup => new TaskGroupQueue<T>(options, token),
				ThreadQueueMode.SemaphoreSlim => new SemaphoreSlimQueue<T>(options, token),
				ThreadQueueMode.Semaphore => new SemaphoreQueue<T>(options, token),
				ThreadQueueMode.Mutex => new MutexQueue<T>(options, token),
				ThreadQueueMode.ThresholdTaskGroup => new ThresholdTaskGroupQueue<T>(options, token),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
			};
		}
	}
}
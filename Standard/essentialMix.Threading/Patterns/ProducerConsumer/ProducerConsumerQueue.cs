using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using essentialMix.Threading.Helpers;
using essentialMix.Threading.Patterns.ProducerConsumer.Queue;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	/*
	 * This is based on the insightful book of Joseph Albahari, C# 6 in a Nutshell
	 * http://www.albahari.com/threading/
	 */
	[DebuggerDisplay("Count = {Count}")]
	public abstract class ProducerConsumerQueue<T> : Disposable, IProducerConsumer<T>
	{
		private CancellationTokenSource _cts;
		private volatile int _paused;
		private volatile int _completeMarked;

		protected ProducerConsumerQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		{
			Threads = options.Threads;
			WaitOnDispose = options.WaitOnDispose;
			ExecuteCallback = options.ExecuteCallback;
			ResultCallback = options.ResultCallback;
			ScheduledCallback = options.ScheduledCallback;
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
				if (WaitOnDispose) WaitInternal(TimeSpanHelper.INFINITE);
				else StopInternal(true);
				ObjectHelper.Dispose(ref _cts);
			}
			base.Dispose(disposing);
		}

		public event EventHandler WorkStarted;
		public event EventHandler WorkCompleted;

		public abstract int Count { get; }

		public bool WaitOnDispose { get; set; }

		public int Threads { get; }

		public CancellationToken Token { get; }

		public abstract bool IsBusy { get; }

		public bool IsPaused
		{
			get
			{
				// ensure we have the latest value
				Thread.MemoryBarrier();
				return _paused != 0;
			}
			protected set => Interlocked.CompareExchange(ref _paused, value
																		? 1
																		: 0, _paused);
		}

		public bool CompleteMarked
		{
			get
			{
				// ensure we have the latest value
				Thread.MemoryBarrier();
				return _completeMarked != 0;
			}
			protected set => Interlocked.CompareExchange(ref _completeMarked, value
																				? 1
																				: 0, _completeMarked);
		}

		public int SleepAfterEnqueue { get; set; } = TimeSpanHelper.INFINITE;

		[NotNull]
		protected Func<T, TaskResult> ExecuteCallback { get; }
		protected Func<T, TaskResult, Exception, bool> ResultCallback { get; }
		protected Action<T> ScheduledCallback { get; }
		protected Action<T> FinalizeCallback { get; }

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

		public void Pause()
		{
			ThrowIfDisposed();
			IsPaused = true;
		}

		public void Resume()
		{
			ThrowIfDisposed();
			IsPaused = false;
		}

		public void Stop()
		{
			ThrowIfDisposed();
			IsPaused = false;
			StopInternal(!WaitOnDispose);
		}

		public void Stop(bool enforce)
		{
			ThrowIfDisposed();
			IsPaused = false;
			StopInternal(enforce);
		}

		[NotNull]
		public Task StopAsync() { return StopAsync(!WaitOnDispose); }

		[NotNull]
		public Task StopAsync(bool enforce)
		{
			ThrowIfDisposed();
			IsPaused = false;
			return TaskHelper.Run(() => StopInternal(enforce), TaskCreationOptions.LongRunning, Token).ConfigureAwait();
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
			return TaskHelper.Run(() => WaitInternal(millisecondsTimeout), TaskCreationOptions.LongRunning, Token).ConfigureAwait();
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
			catch (OperationCanceledException)
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
				ThreadQueueMode.DataFlow => new DataFlowQueue<T>(options, token),
				ThreadQueueMode.BlockingCollection => new BlockingCollectionQueue<T>(options, token),
				_ => Create(mode, new Queue<T>(), options, token)
			};
		}
		
		[NotNull]
		public static IProducerConsumer<T> Create<TQueue, T>(ThreadQueueMode mode, [NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			where TQueue : ICollection, IReadOnlyCollection<T>
		{
			token.ThrowIfCancellationRequested();
			return mode switch
			{
				ThreadQueueMode.Task => new TaskQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.WaitAndPulse => new WaitAndPulseQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.Event => new EventQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.TaskGroup => new TaskGroupQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.SemaphoreSlim => new SemaphoreSlimQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.Semaphore => new SemaphoreQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.Mutex => new MutexQueue<TQueue, T>(queue, options, token),
				ThreadQueueMode.ThresholdTaskGroup => new ThresholdTaskGroupQueue<TQueue, T>(queue, options, token),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
			};
		}
	}
}
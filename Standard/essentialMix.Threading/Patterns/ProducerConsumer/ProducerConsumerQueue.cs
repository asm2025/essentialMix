using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
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
		private readonly SynchronizationContext _context;
		private readonly CallbackDelegates<T> _workStartedCallback;
		private readonly CallbackDelegates<T> _workCompletedCallback;

		private CancellationTokenSource _cts;
		private ManualResetEventSlim _workerStart;
		private CountdownEvent _workersCountdown;
		private AutoResetEvent _taskStart;
		private AutoResetEvent _taskComplete;
		private CountdownEvent _tasksCountdown;

		private volatile int _paused;
		private volatile int _completeMarked;
		private volatile int _tasksCompleted;
		private volatile int _workCompleted;
		
		protected ProducerConsumerQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		{
			InitializeToken(token);
			Threads = options.Threads;
			WaitOnDispose = options.WaitOnDispose;
			ExecuteCallback = options.ExecuteCallback;
			ResultCallback = options.ResultCallback;
			ScheduledCallback = options.ScheduledCallback;
			FinalizeCallback = options.FinalizeCallback;

			_workStartedCallback = options.WorkStartedCallback;
			_workCompletedCallback = options.WorkCompletedCallback;

			_context = options.SynchronizeContext
							? SynchronizationContext.Current
							: null;
			
			if (_context != null)
			{
				_context.OperationStarted();
				WorkStartedCallback = que => _context.Post(WorkStartedSendOrPostCallback, que);
				WorkCompletedCallback = que => _context.Post(WorkCompletedSendOrPostCallback, que);
			}
			else
			{
				WorkStartedCallback = _workStartedCallback;
				WorkCompletedCallback = _workCompletedCallback;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CompleteInternal();

				if (WaitOnDispose) Wait(TimeSpanHelper.INFINITE);
				else Stop(true);
				
				ReleaseWorkStart();
				ReleaseTaskStart();
				ReleaseTaskComplete();
				ReleaseTasksCountDown();
				ReleaseWorkersCountDown();
				ReleaseToken();
				_context?.OperationCompleted();
			}
			base.Dispose(disposing);
		}

		/// <inheritdoc />
		public abstract int Count { get; }
		/// <inheritdoc />
		public int Running => (_tasksCountdown?.CurrentCount ?? 1) - 1;
		/// <inheritdoc />
		public abstract bool IsEmpty { get; }

		/// <inheritdoc />
		public bool IsBusy => Count > 0;

		/// <inheritdoc />
		public bool WaitOnDispose { get; set; }

		/// <inheritdoc />
		public int Threads { get; }

		/// <inheritdoc />
		public CancellationToken Token { get; private set; }

		/// <inheritdoc />
		public abstract bool CanPause { get; }

		/// <inheritdoc />
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

		/// <inheritdoc />
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

		/// <inheritdoc />
		public int SleepAfterEnqueue { get; set; } = TimeSpanHelper.INFINITE;

		[NotNull]
		protected ExecuteCallbackDelegates<T> ExecuteCallback { get; }
		protected ResultCallbackDelegates<T> ResultCallback { get; }
		protected ScheduledCallbackDelegates<T> ScheduledCallback { get; }
		protected FinalizeCallbackDelegates<T> FinalizeCallback { get; }
		protected CallbackDelegates<T> WorkStartedCallback { get; }
		protected CallbackDelegates<T> WorkCompletedCallback { get; }

		[NotNull]
		protected CancellationTokenSource InitializeCancellationTokenSource()
		{
			if (_cts != null) return _cts;
			Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
			Token = _cts.Token;
			return _cts;
		}

		/// <inheritdoc />
		public void InitializeToken(CancellationToken token)
		{
			ThrowIfDisposed();
			if (_cts != null) Interlocked.Exchange(ref _cts, null);
			Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			Token = _cts.Token;
		}

		protected void ReleaseToken()
		{
			if (_cts == null) return;
			Interlocked.Exchange(ref _cts, null);
			Token = CancellationToken.None;
		}

		protected void Cancel()
		{
			_cts.CancelIfNotDisposed();
		}

		protected void InitializeWorkerStart()
		{
			ThrowIfDisposed();
			if (_workerStart != null) return;
			Interlocked.CompareExchange(ref _workerStart, new ManualResetEventSlim(), null);
		}

		protected void SignalWorkerStart()
		{
			_workerStart?.Set();
		}

		protected bool WaitForWorkerStart()
		{
			ThrowIfDisposed();
			if (_workerStart == null) return false;
			if (_workerStart is not { IsSet: false }) return true;
			
			try
			{
				return _workerStart.Wait(TimeSpanHelper.INFINITE, Token);
			}
			catch (OperationCanceledException)
			{
				return true;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		protected void ReleaseWorkStart()
		{
			if (_workerStart == null) return;
			Interlocked.Exchange(ref _workerStart, null);
		}

		protected void InitializeWorkersCountDown(int threads = 0)
		{
			ThrowIfDisposed();
			if (_workersCountdown != null) return;
			Interlocked.CompareExchange(ref _workersCountdown, new CountdownEvent((threads < 1 ? Threads : threads) + 1), null);
			Interlocked.Exchange(ref _workCompleted, 0);
		}

		protected void AddWorkersCountDown()
		{
			ThrowIfDisposed();
			_workersCountdown?.AddCount();
		}

		protected void SignalWorkersCountDown()
		{
			if (_workersCountdown is { CurrentCount: > 1 }) _workersCountdown.Signal();
			if (!CompleteMarked || !IsEmpty || _workersCountdown is { CurrentCount: > 1 }) return;
			if (_workCompleted != 0 || Interlocked.CompareExchange(ref _workCompleted, 1, 0) != 0) return;

			if (_workersCountdown == null)
			{
				WorkCompletedCallback?.Invoke(this);
				return;
			}
			
			if (IsPaused || Running > 0)
			{
				SpinWait.SpinUntil(() => IsDisposed || !IsPaused && Running == 0);
				if (IsDisposed) return;
				Thread.Sleep(0);
			}

			if (_tasksCountdown is { CurrentCount: 1 }) SignalTasksCountDown(true);

			if (!WaitForTasksCountDown(TimeSpanHelper.SECOND))
			{
				if (IsDisposed) return;
				// last chance to recover
				Cancel();

				if (!WaitForTasksCountDown(TimeSpanHelper.SECOND))
				{
					// Ok, this is a shitty situation, we must recover here!
					_tasksCountdown.SignalAll();
				}
			}

			WorkCompletedCallback?.Invoke(this);
			Thread.Sleep(0);
			_workersCountdown.SignalAll();
			ReleaseWorkStart();
			ReleaseWorkersCountDown();
		}

		protected bool WaitForWorkersCountDown(int millisecondsTimeout = TimeSpanHelper.INFINITE)
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (_workersCountdown == null || _workersCountdown.IsSet) return true;
			
			try
			{
				return _workersCountdown.Wait(millisecondsTimeout, Token) && !Token.IsCancellationRequested;
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			catch (TimeoutException)
			{
				// ignored
			}
	
			return false;
		}

		protected void ReleaseWorkersCountDown()
		{
			if (_workersCountdown == null) return;
			Interlocked.Exchange(ref _workersCountdown, null);
		}

		protected void InitializeTaskStart()
		{
			ThrowIfDisposed();
			if (_taskStart != null) return;
			Interlocked.CompareExchange(ref _taskStart, new AutoResetEvent(false), null);
		}

		protected void SignalTaskStart()
		{
			_taskStart?.Set();
		}

		protected bool WaitForTaskStart()
		{
			ThrowIfDisposed();
			if (_taskStart == null || _taskStart.WaitOne(0)) return true;
			
			try
			{
				return _taskStart.WaitOne(TimeSpanHelper.INFINITE, Token);
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

		protected void ReleaseTaskStart()
		{
			if (_taskStart == null) return;
			Interlocked.Exchange(ref _taskStart, null);
		}

		protected void InitializeTaskComplete()
		{
			ThrowIfDisposed();
			if (_taskComplete != null) return;
			Interlocked.CompareExchange(ref _taskComplete, new AutoResetEvent(false), null);
		}

		protected void SignalTaskComplete()
		{
			_taskComplete?.Set();
		}

		protected bool WaitForTaskComplete()
		{
			ThrowIfDisposed();
			if (_taskComplete == null || _taskComplete.WaitOne(0)) return true;
			
			try
			{
				return _taskComplete.WaitOne(TimeSpanHelper.INFINITE, Token);
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

		protected void ReleaseTaskComplete()
		{
			if (_taskComplete == null) return;
			Interlocked.Exchange(ref _taskComplete, null);
		}

		protected void InitializeTasksCountDown(int tasks = 0)
		{
			ThrowIfDisposed();
			if (tasks < 0) throw new ArgumentOutOfRangeException(nameof(tasks));
			if (_tasksCountdown != null) return;
			Interlocked.CompareExchange(ref _tasksCountdown, new CountdownEvent(tasks + 1), null);
			Interlocked.Exchange(ref _tasksCompleted, 0);
		}

		protected void AddTasksCountDown()
		{
			ThrowIfDisposed();
			_tasksCountdown?.AddCount();
		}

		protected void SignalTasksCountDown(bool signalAll = false)
		{
			if (_tasksCountdown is { CurrentCount: > 1 }) _tasksCountdown.Signal();
			if (_tasksCountdown is null or { CurrentCount: > 1 } || !signalAll || !CompleteMarked || !IsEmpty) return;
			if (_tasksCompleted != 0 || Interlocked.CompareExchange(ref _tasksCompleted, 1, 0) != 0) return;
			_tasksCountdown.SignalAll();
			ReleaseTaskStart();
			ReleaseTaskComplete();
			ReleaseTasksCountDown();
		}

		protected bool WaitForTasksCountDown(int millisecondsTimeout = TimeSpanHelper.INFINITE)
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (_tasksCountdown == null || _tasksCountdown.IsSet) return true;
			
			try
			{
				return _tasksCountdown.Wait(millisecondsTimeout, Token) && !Token.IsCancellationRequested;
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			catch (TimeoutException)
			{
				// ignored
			}
	
			return false;
		}

		protected void ReleaseTasksCountDown()
		{
			if (_tasksCountdown == null) return;
			Interlocked.Exchange(ref _tasksCountdown, null);
		}

		/// <inheritdoc />
		public void Enqueue(T item)
		{
			ThrowIfDisposed();
			if (CompleteMarked) throw new InvalidOperationException("Completion marked.");
			EnqueueInternal(item);
			if (SleepAfterEnqueue > 0) TimeSpanHelper.WasteTime(SleepAfterEnqueue, Token);
		}

		/// <inheritdoc />
		public void Clear()
		{
			ThrowIfDisposed();
			ClearInternal();
		}

		/// <inheritdoc />
		public void Complete()
		{
			ThrowIfDisposed();
			if (CompleteMarked) return;
			CompleteInternal();
		}

		/// <inheritdoc />
		public void Pause()
		{
			ThrowIfDisposed();
			if (!CanPause) throw new NotSupportedException();
			if (IsPaused) return;
			IsPaused = true;
			if (Running == 0) return;
			SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || Running == 0);
		}

		/// <inheritdoc />
		public void Resume()
		{
			ThrowIfDisposed();
			if (!CanPause) throw new NotSupportedException();
			IsPaused = false;
		}

		/// <inheritdoc />
		public void Stop() { Stop(!WaitOnDispose); }
		/// <inheritdoc />
		public void Stop(bool enforce)
		{
			ThrowIfDisposed();
			IsPaused = false;
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) Wait(TimeSpanHelper.INFINITE);
			Cancel();
			ClearInternal();
			if (Running == 0) return;
			SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || Running == 0);
		}

		/// <inheritdoc />
		public bool Wait()
		{
			return Wait(TimeSpanHelper.INFINITE);
		}

		/// <inheritdoc />
		public bool Wait(TimeSpan timeout)
		{
			return Wait(timeout.TotalIntMilliseconds());
		}

		/// <inheritdoc />
		public bool Wait(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			return WaitForWorkersCountDown(millisecondsTimeout);
		}

		protected abstract void EnqueueInternal([NotNull] T item);

		protected abstract void CompleteInternal();

		protected abstract void ClearInternal();

		protected virtual void Run(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			try
			{
				TaskResult result = ExecuteCallback(this, item);
				ResultCallback?.Invoke(this, item, result, null);
			}
			catch (TimeoutException)
			{
				ResultCallback?.Invoke(this, item, TaskResult.Timeout, null);
			}
			catch (OperationCanceledException)
			{
				ResultCallback?.Invoke(this, item, TaskResult.Canceled, null);
			}
			catch (Exception e)
			{
				ResultCallback?.Invoke(this, item, TaskResult.Error, e);
			}
			finally
			{
				FinalizeCallback?.Invoke(item);
			}
		}

		private void WorkStartedSendOrPostCallback([NotNull] object state) { _workStartedCallback((IProducerConsumer<T>)state); }
		private void WorkCompletedSendOrPostCallback([NotNull] object state) { _workCompletedCallback((IProducerConsumer<T>)state); }
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
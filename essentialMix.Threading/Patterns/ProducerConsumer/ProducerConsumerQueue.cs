using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using essentialMix.Threading.Patterns.ProducerConsumer.Queue;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer;

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
	private IDisposable _tokenRegistration;
	private ManualResetEventSlim _workerStart;
	private CountdownEvent _workersCountdown;
	private CountdownEvent _tasksCountdown;

	private volatile int _isPaused;
	private volatile int _isCompleted;
	private volatile int _tasksCompleted;
	private volatile int _workCompleted;

	protected ProducerConsumerQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
	{
		Threads = options.Threads;
		WaitOnDispose = options.WaitOnDispose;

		_context = options.SynchronizeContext
						? SynchronizationContext.Current
						: null;
		InitializeToken(token);

		_workStartedCallback = options.WorkStartedCallback;
		_workCompletedCallback = options.WorkCompletedCallback;

		ExecuteCallback = options.ExecuteCallback;
		ResultCallback = options.ResultCallback;
		ScheduledCallback = options.ScheduledCallback;
		FinalizeCallback = options.FinalizeCallback;

		if (_context != null)
		{
			_context.OperationStarted();
			if (_workStartedCallback != null) WorkStartedCallback = que => _context.Post(SendWorkStartedCallback, que);
			if (_workCompletedCallback != null) WorkCompletedCallback = que => _context.Post(SendWorkCompletedCallback, que);
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
			Stop();
			ReleaseWorkStart();
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
			return _isPaused != 0;
		}
		protected set => Interlocked.CompareExchange(ref _isPaused, value
																		? 1
																		: 0, _isPaused);
	}

	/// <inheritdoc />
	public bool IsCompleted
	{
		get
		{
			// ensure we have the latest value
			Thread.MemoryBarrier();
			return _isCompleted != 0;
		}
		protected set => Interlocked.CompareExchange(ref _isCompleted, value
																			? 1
																			: 0, _isCompleted);
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

	/// <inheritdoc />
	public void InitializeToken(CancellationToken token)
	{
		ThrowIfDisposed();
		if (_cts != null) Interlocked.Exchange(ref _cts, null);
		ObjectHelper.Dispose(ref _tokenRegistration);
		Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
		if (token.CanBeCanceled) _tokenRegistration = token.Register(() => _cts.CancelIfNotDisposed(), false);
		Token = _cts.Token;
	}

	protected void ReleaseToken()
	{
		if (_cts == null) return;
		Interlocked.Exchange(ref _cts, null);
		ObjectHelper.Dispose(ref _tokenRegistration);
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
		if (!IsCompleted || !IsEmpty || _workersCountdown is { CurrentCount: > 1 }) return;
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

	protected Task<bool> WaitForWorkersCountDownAsync(int millisecondsTimeout = TimeSpanHelper.INFINITE)
	{
		ThrowIfDisposed();
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		if (_workersCountdown == null || _workersCountdown.IsSet) return Task.FromResult(true);
		return TaskHelper.FromWaitHandle(_workersCountdown.WaitHandle, millisecondsTimeout, Token);
	}

	protected void ReleaseWorkersCountDown()
	{
		if (_workersCountdown == null) return;
		Interlocked.Exchange(ref _workersCountdown, null);
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
		if (_tasksCountdown is null or { CurrentCount: > 1 } || !signalAll || !IsCompleted || !IsEmpty) return;
		if (_tasksCompleted != 0 || Interlocked.CompareExchange(ref _tasksCompleted, 1, 0) != 0) return;
		_tasksCountdown.SignalAll();
		ReleaseTasksCountDown();
	}

	protected bool WaitForTasksCountDown(int millisecondsTimeout = TimeSpanHelper.INFINITE)
	{
		ThrowIfDisposed();
		if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
		if (_workersCountdown == null || _workersCountdown.IsSet) return true;

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
		if (IsCompleted) throw new InvalidOperationException("Completion marked.");
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
		if (IsCompleted) return;
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
		SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || Running == 0, TimeSpanHelper.SECOND);
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
	public bool Wait() { return Wait(TimeSpanHelper.INFINITE); }
	/// <inheritdoc />
	public bool Wait(TimeSpan timeout) { return Wait(timeout.TotalIntMilliseconds()); }
	/// <inheritdoc />
	public bool Wait(int millisecondsTimeout)
	{
		ThrowIfDisposed();
		return WaitForWorkersCountDown(millisecondsTimeout);
	}

	/// <inheritdoc />
	public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }
	/// <inheritdoc />
	public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }
	/// <inheritdoc />
	public Task<bool> WaitAsync(int millisecondsTimeout) { return WaitForWorkersCountDownAsync(millisecondsTimeout); }

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

	private void SendWorkStartedCallback([NotNull] object state) { _workStartedCallback((IProducerConsumer<T>)state); }
	private void SendWorkCompletedCallback([NotNull] object state) { _workCompletedCallback((IProducerConsumer<T>)state); }
}

public static class ProducerConsumerQueue
{
	[NotNull]
	public static IProducerConsumer<T> Create<T>(ThreadQueueMode mode, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return mode switch
		{
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
			ThreadQueueMode.ThreadPool => new ThreadPoolQueue<TQueue, T>(queue, options, token),
			ThreadQueueMode.ThresholdTaskGroup => new ThresholdTaskGroupQueue<TQueue, T>(queue, options, token),
			_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
		};
	}
}
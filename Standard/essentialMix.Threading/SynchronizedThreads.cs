using System;
using System.Threading;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading
{
	// based on TwoWaySignaling
	public sealed class SynchronizedThreads<T> : Disposable
	{
		private readonly Thread[] _workers;
		[NotNull]
		private readonly Func<T, bool> _mainRoutine;
		[NotNull]
		private readonly Func<T, bool> _workerRoutine;

		private readonly Action<SynchronizedThreads<T>> _workStartedCallback;
		private readonly Action<SynchronizedThreads<T>> _workCompletedCallback;

		private ManualResetEventSlim _mainWaitEvent = new ManualResetEventSlim(false);
		private ManualResetEventSlim _workerWaitEvent = new ManualResetEventSlim(false);
		private CountdownEvent _workersCountdown;
		private CancellationTokenSource _cts;

		private volatile int _isPaused;
		private volatile int _workCompleted;

		private object _syncRoot;

		public SynchronizedThreads([NotNull] Func<T, bool> mainRoutine, [NotNull] Func<T, bool> workerRoutine, CancellationToken token = default(CancellationToken))
			: this(null, mainRoutine, workerRoutine, token)
		{
		}

		public SynchronizedThreads(SynchronizedThreadsOptions<T> options, [NotNull] Func<T, bool> mainRoutine, [NotNull] Func<T, bool> workerRoutine, CancellationToken token = default(CancellationToken))
		{
			options ??= new SynchronizedThreadsOptions<T>();
			IsBackground = options.IsBackground;
			Priority = options.Priority;
			WaitOnDispose = options.WaitOnDispose;

			InitializeToken(token);
			
			_workStartedCallback = options.WorkStartedCallback;
			_workCompletedCallback = options.WorkCompletedCallback;

			_workersCountdown = new CountdownEvent(1);

			_mainRoutine = mainRoutine;
			_workerRoutine = workerRoutine;
			_workers = new[]
			{
				new Thread(Main)
				{
					IsBackground = IsBackground,
					Priority = Priority
				},
				new Thread(Worker)
				{
					IsBackground = IsBackground,
					Priority = Priority
				}
			};
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop(WaitOnDispose);
				ObjectHelper.Dispose(ref _mainWaitEvent);
				ObjectHelper.Dispose(ref _workerWaitEvent);
				ObjectHelper.Dispose(ref _workersCountdown);
				ObjectHelper.Dispose(ref _cts);
			}

			base.Dispose(disposing);
		}

		public T State { get; set; }

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }

		public bool WaitOnDispose { get; set; }

		public CancellationToken Token { get; private set; }

		public bool IsBusy => _workersCountdown is { CurrentCount: > 1 };

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		public bool IsPaused
		{
			get
			{
				// ensure we have the latest value
				Thread.MemoryBarrier();
				return _isPaused != 0;
			}
			private set => Interlocked.CompareExchange(ref _isPaused, value
																			? 1
																			: 0, _isPaused);
		}

		public void InitializeToken(CancellationToken token)
		{
			ThrowIfDisposed();
			if (_cts != null) Interlocked.Exchange(ref _cts, null);
			Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			Token = _cts.Token;
		}

		public void Start()
		{
			ThrowIfDisposed();
			if (IsBusy) return;

			lock(SyncRoot)
			{
				_workStartedCallback?.Invoke(this);
				_workersCountdown.AddCount(_workers.Length);

				foreach (Thread thread in _workers) 
					thread.Start();
			}
		}

		public void Stop() { Stop(WaitOnDispose); }
		public void Stop(bool waitForQueue)
		{
			ThrowIfDisposed();
			Cancel();
			// Wait for the consumer's thread to finish.
			if (waitForQueue) Wait(TimeSpanHelper.INFINITE);
		}

		public void Pause()
		{
			ThrowIfDisposed();
			if (IsPaused) return;
			IsPaused = true;
		}

		public void Resume()
		{
			ThrowIfDisposed();
			IsPaused = false;
		}

		public bool Wait() { return Wait(TimeSpanHelper.INFINITE); }
		public bool Wait(TimeSpan timeout) { return Wait(timeout.TotalIntMilliseconds()); }
		public bool Wait(int millisecondsTimeout)
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

		private void Cancel()
		{
			_cts.CancelIfNotDisposed();
		}

		private void Main()
		{
			if (Token.IsCancellationRequested)
			{
				SignalWorkersCountDown();
				return;
			}

			bool more = true;

			do
			{
				if (IsPaused)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
					continue;
				}

				if (Token.IsCancellationRequested || IsDisposed) break;
				_workerWaitEvent.Reset();
				_workerWaitEvent.Wait(Token);
				if (Token.IsCancellationRequested || IsDisposed) break;

				lock(SyncRoot)
				{
					if (IsDisposed || Token.IsCancellationRequested || IsPaused) continue;
					more = _mainRoutine(State);
				}

				_mainWaitEvent.Set();
			}
			while (more);

			SignalWorkersCountDown();
		}

		private void Worker()
		{
			if (IsDisposed || Token.IsCancellationRequested)
			{
				SignalWorkersCountDown();
				return;
			}

			bool more = true;

			do
			{
				if (IsPaused)
				{
					SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
					continue;
				}

				if (Token.IsCancellationRequested || IsDisposed) break;
				_mainWaitEvent.Reset();
				_workerWaitEvent.Set();
				_mainWaitEvent.Wait(Token);
				if (Token.IsCancellationRequested || IsDisposed) break;

				lock(SyncRoot)
				{
					if (IsDisposed || Token.IsCancellationRequested || IsPaused) continue;
					more = _workerRoutine(State);
				}
			}
			while (more);

			SignalWorkersCountDown();
		}

		private void SignalWorkersCountDown()
		{
			if (_workersCountdown is { CurrentCount: > 1 }) _workersCountdown.Signal();
			if (_workersCountdown is { CurrentCount: > 1 }) return;
			if (_workCompleted != 0 || Interlocked.CompareExchange(ref _workCompleted, 1, 0) != 0) return;

			if (_workersCountdown == null)
			{
				_workCompletedCallback?.Invoke(this);
				return;
			}

			_workCompletedCallback?.Invoke(this);
			Thread.Sleep(0);
			_workersCountdown.SignalAll();
		}
	}
}
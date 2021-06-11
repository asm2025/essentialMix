using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading
{
	// based on TwoWaySignaling
	public sealed class SynchronizedThreads<T> : Disposable
	{
		private readonly object _lock = new object();
		private readonly Thread[] _workers;
		private ManualResetEventSlim _mainWaitEvent = new ManualResetEventSlim(false);
		private ManualResetEventSlim _workerWaitEvent = new ManualResetEventSlim(false);
		private CountdownEvent _countdown;
		private CancellationTokenSource _cts;

		private readonly CancellationToken _token;
		private readonly bool _isBackground;
		private readonly ThreadPriority _priority;
		private bool _waitForQueuedItems = true;

		public SynchronizedThreads([NotNull] Func<T, bool> mainRoutine, [NotNull] Func<T, bool> workerRoutine, CancellationToken token = default(CancellationToken))
			: this(mainRoutine, workerRoutine, ThreadPriority.Normal, false, token)
		{
		}

		public SynchronizedThreads([NotNull] Func<T, bool> mainRoutine, [NotNull] Func<T, bool> workerRoutine, bool isBackground, CancellationToken token = default(CancellationToken))
			: this(mainRoutine, workerRoutine, ThreadPriority.Normal, isBackground, token)
		{
		}

		public SynchronizedThreads([NotNull] Func<T, bool> mainRoutine, [NotNull] Func<T, bool> workerRoutine, ThreadPriority priority, bool isBackground, CancellationToken token = default(CancellationToken))
		{
			MainRoutine = mainRoutine;
			WorkerRoutine = workerRoutine;
			_isBackground = isBackground;
			_priority = priority;
			_cts = new CancellationTokenSource();
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			_token = _cts.Token;
			_countdown = new CountdownEvent(1);
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
				StopInternal(WaitForQueuedItems);

				for (int i = 0; i < _workers.Length; i++)
					ObjectHelper.Dispose(ref _workers[i]);

				ObjectHelper.Dispose(ref _mainWaitEvent);
				ObjectHelper.Dispose(ref _workerWaitEvent);
				ObjectHelper.Dispose(ref _countdown);
				ObjectHelper.Dispose(ref _cts);
			}

			base.Dispose(disposing);
		}

		public event EventHandler WorkStarted;
		public event EventHandler WorkCompleted;

		public T State { get; set; }

		public bool IsBackground
		{
			get
			{
				ThrowIfDisposed();
				return _isBackground;
			}
		}

		public ThreadPriority Priority
		{
			get
			{
				ThrowIfDisposed();
				return _priority;
			}
		}

		public bool WaitForQueuedItems
		{
			get
			{
				ThrowIfDisposed();
				return _waitForQueuedItems;
			}
			set
			{
				ThrowIfDisposed();
				_waitForQueuedItems = value;
			}
		}

		public CancellationToken Token
		{
			get
			{
				ThrowIfDisposed();
				return _token;
			}
		}

		public bool IsBusy
		{
			get
			{
				ThrowIfDisposed();
				return IsBusyInternal;
			}
		}

		[NotNull]
		private Func<T, bool> MainRoutine { get; }

		[NotNull]
		private Func<T, bool> WorkerRoutine { get; }

		private bool IsBusyInternal => _countdown is { CurrentCount: > 1 };

		public void Start()
		{
			ThrowIfDisposed();
			if (IsBusyInternal) return;

			lock (_lock)
			{
				OnWorkStarted(EventArgs.Empty);
				_workers.ForEach(t =>
				{
					t.Start();
					_countdown.AddCount();
				});
			}
		}

		public void Stop()
		{
			ThrowIfDisposed();
			StopInternal(WaitForQueuedItems);
		}

		public void Stop(bool waitForQueue)
		{
			ThrowIfDisposed();
			StopInternal(waitForQueue);
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

		private bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusyInternal) return true;

			try
			{
				if (millisecondsTimeout > TimeSpanHelper.INFINITE) return _countdown.Wait(millisecondsTimeout, Token);
				_countdown.Wait(Token);
				return !Token.IsCancellationRequested;

			}
			catch (OperationCanceledException)
			{
			}
			catch (TimeoutException)
			{
			}
			catch (AggregateException ag) when (ag.InnerException is OperationCanceledException || ag.InnerException is TimeoutException)
			{
			}

			return false;
		}

		private void StopInternal(bool waitForQueue)
		{
			lock (_lock)
			{
				Cancel();
				Monitor.PulseAll(_lock);
			}

			// Wait for the consumer's thread to finish.
			if (waitForQueue) WaitInternal(TimeSpanHelper.INFINITE);
		}

		private void OnWorkStarted(EventArgs args) { WorkStarted?.Invoke(this, args); }

		private void OnWorkCompleted(EventArgs args) { WorkCompleted?.Invoke(this, args); }

		private void Cancel()
		{
			_cts.CancelIfNotDisposed();
		}

		private void Main()
		{
			if (Token.IsCancellationRequested)
			{
				SignalAndCheck();
				return;
			}

			bool more;

			do
			{
				_workerWaitEvent.Reset();
				_workerWaitEvent.Wait(Token);
				if (Token.IsCancellationRequested || IsDisposed) break;

				lock(_lock)
				{
					more = MainRoutine(State);
				}

				_mainWaitEvent.Set();
			}
			while (more);

			SignalAndCheck();
		}

		private void Worker()
		{
			if (IsDisposed || Token.IsCancellationRequested)
			{
				SignalAndCheck();
				return;
			}

			bool more;

			do
			{
				_mainWaitEvent.Reset();
				_workerWaitEvent.Set();
				_mainWaitEvent.Wait(Token);
				if (Token.IsCancellationRequested || IsDisposed) break;

				lock(_lock)
				{
					more = WorkerRoutine(State);
				}
			}
			while (more);

			SignalAndCheck();
		}

		private void SignalAndCheck()
		{
			if (IsDisposed || _countdown == null) return;
			Monitor.Enter(_countdown);

			bool completed = false;

			try
			{
				if (IsDisposed || _countdown == null) return;
				_countdown.Signal();
				if (_countdown.CurrentCount > 1) return;
			}
			finally
			{
				if (_countdown is { CurrentCount: < 2 })
				{
					_countdown.SignalAll();
					completed = true;
				}
				
				if (_countdown != null) 
					Monitor.Exit(_countdown);
				else
					completed = true;
			}

			if (!completed) return;
			OnWorkCompleted(EventArgs.Empty);
		}
	}
}
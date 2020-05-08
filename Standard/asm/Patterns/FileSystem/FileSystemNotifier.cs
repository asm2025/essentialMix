using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Patterns.FileSystem
{
	public class FileSystemNotifier : FileSystemWatcher
	{
		private readonly object _lock = new object();
		private readonly Queue<Action> _queue = new Queue<Action>();

		private ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);
		private Thread _worker;
		private CancellationTokenSource _cts;

		/// <inheritdoc />
		public FileSystemNotifier(FileSystemWatcherSettings settings, CancellationToken token = default(CancellationToken))
			: this(settings, false, ThreadPriority.Normal, token)
		{
		}

		/// <inheritdoc />
		public FileSystemNotifier(FileSystemWatcherSettings settings, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: this(settings, false, priority, token)
		{
		}

		/// <inheritdoc />
		public FileSystemNotifier(FileSystemWatcherSettings settings, bool isBackground, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: base(settings)
		{
			IsBackground = isBackground;
			Priority = priority;
			_cts = new CancellationTokenSource();
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			Token = _cts.Token;
			(_worker = new Thread(Consume)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}).Start();
		}

		public int Count => CountInternal;

		public bool WaitForQueuedItems { get; set; } = true;

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }

		public CancellationToken Token { get; }

		public bool IsBusy => IsBusyInternal;

		public bool CompleteMarked { get; protected set; }

		protected virtual int CountInternal
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		protected virtual bool IsBusyInternal { get; private set; }

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopInternal(WaitForQueuedItems);
				ObjectHelper.Dispose(ref _worker);
				ObjectHelper.Dispose(ref _manualResetEventSlim);
				ObjectHelper.Dispose(ref _cts);
			}

			base.Dispose(disposing);
		}

		public void Complete()
		{
			ThrowIfDisposed();
			if (CompleteMarked) return;
			CompleteInternal();
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

		[NotNull] public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }

		[NotNull] public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }

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

		protected virtual void CompleteInternal()
		{
			lock (_lock)
			{
				CompleteMarked = true;
				Monitor.PulseAll(_lock);
			}
		}

		protected virtual void ClearInternal()
		{
			lock (_lock)
			{
				_queue.Clear();
				Monitor.PulseAll(_lock);
			}
		}

		protected virtual bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusyInternal) return true;

			try
			{
				if (millisecondsTimeout < 0)
					_manualResetEventSlim.Wait(Token);
				else if (!_manualResetEventSlim.Wait(millisecondsTimeout, Token))
					return false;

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

		protected virtual void StopInternal(bool waitForQueue)
		{
			lock (_lock)
			{
				Cancel();
				Monitor.PulseAll(_lock);
			}

			// Wait for the consumer's thread to finish.
			if (waitForQueue) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
		}

		protected void Cancel() { _cts.CancelIfNotDisposed(); }

		protected override void OnCreated(FileSystemEventArgs args)
		{
			if (!HasCreatedSubscribers) return;
			Enqueue(() => base.OnCreated(args));
		}

		/// <inheritdoc />
		protected override void OnRenamed(RenamedEventArgs args)
		{
			if (!HasRenamedSubscribers) return;
			Enqueue(() => base.OnRenamed(args));
		}

		/// <inheritdoc />
		protected override void OnDeleted(FileSystemEventArgs args)
		{
			if (!HasDeletedSubscribers) return;
			Enqueue(() => base.OnDeleted(args));
		}

		/// <inheritdoc />
		protected override void OnChanged(FileSystemEventArgs args)
		{
			if (!HasChangedSubscribers) return;
			Enqueue(() => base.OnChanged(args));
		}

		private void Enqueue(Action action)
		{
			if (IsDisposedOrDisposing || Token.IsCancellationRequested || CompleteMarked) return;

			lock (_lock)
			{
				// the repeated check is intentional. The lock might take a while to be entered.
				if (IsDisposedOrDisposing || Token.IsCancellationRequested || CompleteMarked) return;
				_queue.Enqueue(action);
				Monitor.Pulse(_lock);
			}
		}

		private void Consume()
		{
			_manualResetEventSlim.Reset();
			if (IsDisposedOrDisposing || IsBusyInternal) return;
			IsBusyInternal = true;

			try
			{
				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
				{
					Action action = null;

					while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposedOrDisposing || Token.IsCancellationRequested || _queue.Count == 0) continue;
							action = _queue.Dequeue();
						}

						if (action != null) break;
					}
					
					if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;
					action?.Invoke();
				}

				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					Action action;
					
					lock (_lock)
					{
						if (_queue.Count == 0) return;
						action = _queue.Dequeue();
					}

					if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;
					action?.Invoke();
				}
			}
			finally
			{
				IsBusyInternal = false;
				_manualResetEventSlim.Set();
			}
		}
	}
}
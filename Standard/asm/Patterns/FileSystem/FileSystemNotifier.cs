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
		private enum EventType
		{
			None,
			Created,
			Renamed,
			Deleted,
			Changed
		}

		private readonly object _lock = new object();
		private readonly Queue<(EventType Type, FileSystemEventArgs Args)> _queue = new Queue<(EventType Type, FileSystemEventArgs Args)>();

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
				if (millisecondsTimeout < TimeSpanHelper.MINIMUM)
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
			Enqueue(EventType.Created, args);
		}

		/// <inheritdoc />
		protected override void OnRenamed(RenamedEventArgs args)
		{
			if (!HasRenamedSubscribers) return;
			Enqueue(EventType.Renamed, args);
		}

		/// <inheritdoc />
		protected override void OnDeleted(FileSystemEventArgs args)
		{
			if (!HasDeletedSubscribers) return;
			Enqueue(EventType.Deleted, args);
		}

		/// <inheritdoc />
		protected override void OnChanged(FileSystemEventArgs args)
		{
			if (!HasChangedSubscribers) return;
			Enqueue(EventType.Changed, args);
		}

		private void Enqueue(EventType type, FileSystemEventArgs args)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			lock (_lock)
			{
				// the repeated check is intentional. The lock might take a while to be entered.
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
				_queue.Enqueue((type, args));
				Monitor.Pulse(_lock);
			}
		}

		private void Consume()
		{
			_manualResetEventSlim.Reset();
			if (IsDisposed || IsBusyInternal) return;
			IsBusyInternal = true;

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					EventType type = EventType.None;
					FileSystemEventArgs args = null;

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && type == EventType.None)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;
							(type, args) = _queue.Dequeue();
						}
					}
					
					if (type == EventType.None) continue;
					if (IsDisposed || Token.IsCancellationRequested) return;

					switch (type)
					{
						case EventType.Created:
							base.OnCreated(args);
							break;
						case EventType.Renamed:
							base.OnRenamed((RenamedEventArgs)args);
							break;
						case EventType.Deleted:
							base.OnDeleted(args);
							break;
						case EventType.Changed:
							base.OnChanged(args);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					EventType type;
					FileSystemEventArgs args;
					
					lock (_lock)
					{
						if (_queue.Count == 0) return;
						(type, args) = _queue.Dequeue();
					}

					if (type == EventType.None) continue;
					if (IsDisposed || Token.IsCancellationRequested) return;

					switch (type)
					{
						case EventType.Created:
							base.OnCreated(args);
							break;
						case EventType.Renamed:
							base.OnRenamed((RenamedEventArgs)args);
							break;
						case EventType.Deleted:
							base.OnDeleted(args);
							break;
						case EventType.Changed:
							base.OnChanged(args);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
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
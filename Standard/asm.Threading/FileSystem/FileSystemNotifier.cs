using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Threading.FileSystem
{
	public class FileSystemNotifier : FileSystemWatcher
	{
		private enum EventType
		{
			Created,
			Renamed,
			Deleted,
			Changed
		}

		private BlockingCollection<(EventType Type, FileSystemEventArgs Args)> _queue;
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
			_queue = new BlockingCollection<(EventType Type, FileSystemEventArgs Args)>();
			(_worker = new Thread(Consume)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}).Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CompleteInternal();
				StopInternal(WaitForQueuedItems);
				ObjectHelper.Dispose(ref _queue);
				ObjectHelper.Dispose(ref _manualResetEventSlim);
				ObjectHelper.Dispose(ref _cts);
			}
			base.Dispose(disposing);
		}

		public int Count => _queue.Count;

		public bool WaitForQueuedItems { get; set; } = true;

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }

		public CancellationToken Token { get; }

		public bool IsBusy => _manualResetEventSlim != null && !_manualResetEventSlim.IsSet;

		public bool CompleteMarked => _queue.IsAddingCompleted;

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
			_queue.CompleteAdding();
		}

		protected virtual void ClearInternal()
		{
			_queue.Clear();
		}

		protected virtual bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusy) return true;

			try
			{
				if (millisecondsTimeout < TimeSpanHelper.ZERO)
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
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (waitForQueue) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
			ObjectHelper.Dispose(ref _worker);
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
			_queue.Add((type, args), Token);
		}

		private void Consume()
		{
			if (IsDisposed) return;

			_manualResetEventSlim.Reset();

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !_queue.IsCompleted)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryTake(out (EventType Type, FileSystemEventArgs Args) item, TimeSpanHelper.FAST_SCHEDULE, Token))
						Process(item.Type, item.Args);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryTake(out (EventType Type, FileSystemEventArgs Args) item, TimeSpanHelper.FAST_SCHEDULE, Token))
					Process(item.Type, item.Args);
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				_manualResetEventSlim.Set();
			}
		}

		private void Process(EventType type, FileSystemEventArgs args)
		{
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
			}
		}
	}
}
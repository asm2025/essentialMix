using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.FileSystem
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

		private readonly SynchronizationContext _context;
		private readonly Action<FileSystemNotifier> _workStartedCallback;
		private readonly Action<FileSystemNotifier> _workCompletedCallback;

		private BlockingCollection<(EventType Type, FileSystemEventArgs Args)> _queue;
		private ManualResetEventSlim _workCompletedEvent = new ManualResetEventSlim(false);
		private CancellationTokenSource _cts;
		private IDisposable _tokenRegistration;

		/// <inheritdoc />
		public FileSystemNotifier(FileSystemWatcherSettings options, CancellationToken token = default(CancellationToken))
			: base(options)
		{
			IsBackground = options.IsBackground;
			Priority = options.Priority;
			WaitOnDispose = options.WaitOnDispose;
			
			_context = options.SynchronizeContext
							? SynchronizationContext.Current
							: null;
			_workStartedCallback = options.WorkStartedCallback;
			_workCompletedCallback = options.WorkCompletedCallback;
			
			if (_context != null)
			{
				_context.OperationStarted();
				if (_workStartedCallback != null) WorkStartedCallback = fsn => _context.Post(SendWorkStartedCallback, fsn);
				if (_workCompletedCallback != null) WorkCompletedCallback = fsn => _context.Post(SendWorkCompletedCallback, fsn);
			}
			else
			{
				WorkStartedCallback = _workStartedCallback;
				WorkCompletedCallback = _workCompletedCallback;
			}

			_cts = new CancellationTokenSource();
			if (token.CanBeCanceled) _tokenRegistration = token.Register(state => ((CancellationTokenSource)state).CancelIfNotDisposed(), _cts, false);
			Token = _cts.Token;
			
			_queue = new BlockingCollection<(EventType Type, FileSystemEventArgs Args)>();
			new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}.Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
				ObjectHelper.Dispose(ref _queue);
				ObjectHelper.Dispose(ref _workCompletedEvent);
				ObjectHelper.Dispose(ref _tokenRegistration);
				ObjectHelper.Dispose(ref _cts);
				_context?.OperationCompleted();
			}
			base.Dispose(disposing);
		}

		public bool IsBackground { get; }
		public ThreadPriority Priority { get; }
		public int Count => _queue.Count;
		public bool WaitOnDispose { get; set; }
		public CancellationToken Token { get; }
		public bool IsBusy => _workCompletedEvent is { IsSet: false };
		public bool IsCompleted => _queue.IsAddingCompleted;
		protected Action<FileSystemNotifier> WorkStartedCallback { get; }
		protected Action<FileSystemNotifier> WorkCompletedCallback { get; }

		public void Complete()
		{
			ThrowIfDisposed();
			if (IsCompleted) return;
			_queue.CompleteAdding();
		}

		public void Stop() { Stop(WaitOnDispose); }

		public void Stop(bool waitForQueue)
		{
			Complete();
			
			if (waitForQueue)
			{
				Wait(TimeSpanHelper.INFINITE);
			}
			else
			{
				Clear();
				Cancel();
			}
		}

		public bool Wait() { return Wait(TimeSpanHelper.INFINITE, CancellationToken.None); }
		public bool Wait(CancellationToken token) { return Wait(TimeSpanHelper.INFINITE, token); }
		public bool Wait(TimeSpan timeout) { return Wait(timeout.TotalIntMilliseconds(), CancellationToken.None); }
		public bool Wait(TimeSpan timeout, CancellationToken token) { return Wait(timeout.TotalIntMilliseconds(), token); }
		public bool Wait(int millisecondsTimeout) { return Wait(millisecondsTimeout, CancellationToken.None); }
		public bool Wait(int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (Token.IsCancellationRequested || token.IsCancellationRequested) return false;

			IDisposable tokenRegistration = null;

			try
			{
				if (token.CanBeCanceled) tokenRegistration = token.Register(() => _cts.CancelIfNotDisposed(), false);

				if (millisecondsTimeout < TimeSpanHelper.ZERO) _workCompletedEvent.Wait(Token);
				else if (!_workCompletedEvent.Wait(millisecondsTimeout, Token)) return false;

				return !Token.IsCancellationRequested;
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			catch (TimeoutException)
			{
				// ignored
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenRegistration);
			}

			return false;
		}

		public void Clear()
		{
			ThrowIfDisposed();
			_queue.Clear();
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
			ThrowIfDisposed();
			if (IsCompleted) throw new InvalidOperationException("Completion marked.");
			if (Token.IsCancellationRequested) return;
			_queue.Add((type, args), Token);
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			_workCompletedEvent.Reset();

			try
			{
				// BlockingCollection<T> is thread safe
				while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted)
				{
					if (!Enabled)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || Enabled);
						continue;
					}

					while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0 && _queue.TryTake(out (EventType Type, FileSystemEventArgs Args) item, TimeSpanHelper.FAST, Token))
						Process(item.Type, item.Args);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				IEnumerator<(EventType Type, FileSystemEventArgs Args)> enumerator = null;

				try
				{
					enumerator = _queue.GetConsumingEnumerable(Token).GetEnumerator();

					while (!IsDisposed && !Token.IsCancellationRequested)
					{
						if (!Enabled)
						{
							SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !Enabled);
							if (IsDisposed || Token.IsCancellationRequested) return;
						}

						if (!enumerator.MoveNext()) break;
						(EventType type, FileSystemEventArgs args) = enumerator.Current;
						Process(type, args);
					}
				}
				finally
				{
					ObjectHelper.Dispose(ref enumerator);
				}
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				_workCompletedEvent?.Set();
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
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private void SendWorkStartedCallback([NotNull] object state) { _workStartedCallback((FileSystemNotifier)state); }
		private void SendWorkCompletedCallback([NotNull] object state) { _workCompletedCallback((FileSystemNotifier)state); }
	}
}
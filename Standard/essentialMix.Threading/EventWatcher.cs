using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading
{
	public class EventWatcher<TSource, TArgs> : Disposable
		where TArgs : EventArgs
	{
		private readonly SynchronizationContext _context;
		private readonly Delegate _listener;
		private readonly Action<EventWatcher<TSource, TArgs>, TArgs> _watcherCallback;

		private ManualResetEventSlim _workCompletedEvent;
		private CancellationTokenSource _cts;

		/// <inheritdoc />
		protected internal EventWatcher([NotNull] TSource target, [NotNull] EventInfo eventInfo, Action<EventWatcher<TSource, TArgs>, TArgs> watcherCallback)
		{
			Type type = target.GetType();
			if (eventInfo.DeclaringType == null || !eventInfo.DeclaringType.IsAssignableFrom(type)) throw new MemberAccessException($"Type '{type}' does not contain a definition for event '{eventInfo.Name}'.");
			Target = target;
			EventInfo = eventInfo;
			_context = SynchronizationContext.Current;
			_watcherCallback = watcherCallback;

			if (_context != null)
			{
				_context.OperationStarted();
				if (_watcherCallback != null) WatcherCallback = (_, args) => SendWatcherCallback(args);
			}
			else
			{
				WatcherCallback = _watcherCallback;
			}

			_workCompletedEvent = new ManualResetEventSlim(false);
			_cts = new CancellationTokenSource();
			Token = _cts.Token;

			if (_watcherCallback != null)
			{
				_listener = typeof(TArgs) == typeof(EventArgs)
								? EventInfo.CreateDelegate((_, args) =>
								{
									_workCompletedEvent.Set();
									WatcherCallback.Invoke(this, (TArgs)args);
								})
								: EventInfo.CreateDelegate<TArgs>((_, args) =>
								{
									_workCompletedEvent.Set();
									WatcherCallback.Invoke(this, args);
								});
			}
			else
			{
				_listener = typeof(TArgs) == typeof(EventArgs)
								? EventInfo.CreateDelegate((_, _) => _workCompletedEvent.Set())
								: EventInfo.CreateDelegate<TArgs>((_, _) => _workCompletedEvent.Set());
			}

			EventInfo.AddEventHandler(Target, _listener);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
				EventInfo.RemoveEventHandler(Target, _listener);
				ObjectHelper.Dispose(ref _workCompletedEvent);
				ObjectHelper.Dispose(ref _cts);
				_context?.OperationCompleted();
			}
			base.Dispose(disposing);
		}

		protected Action<EventWatcher<TSource, TArgs>, TArgs> WatcherCallback { get; }
		
		[NotNull]
		public TSource Target { get; }

		[NotNull]
		public EventInfo EventInfo { get; }

		public bool IsSignaled => _workCompletedEvent is { IsSet: true };

		public CancellationToken Token { get; }

		public void Stop(bool enforce = false)
		{
			if (!enforce) Wait(TimeSpanHelper.INFINITE);
			_cts.CancelIfNotDisposed();
		}

		public bool Wait() { return Wait(TimeSpanHelper.INFINITE, CancellationToken.None); }
		public bool Wait(CancellationToken token) { return Wait(TimeSpanHelper.INFINITE, token); }
		public bool Wait(TimeSpan timeout) { return Wait(timeout.TotalIntMilliseconds(), CancellationToken.None); }
		public bool Wait(TimeSpan timeout, CancellationToken token) { return Wait(timeout.TotalIntMilliseconds(), token); }
		public bool Wait(int millisecondsTimeout) { return Wait(millisecondsTimeout, CancellationToken.None); }
		public bool Wait(int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (IsSignaled) return true;
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

		[NotNull]
		public Task<bool> WatchAsync() { return WatchAsync(TimeSpanHelper.INFINITE, CancellationToken.None); }
		[NotNull]
		public Task<bool> WatchAsync(CancellationToken token) { return WatchAsync(TimeSpanHelper.INFINITE, token); }
		[NotNull]
		public Task<bool> WatchAsync(TimeSpan timeout, CancellationToken token) { return WatchAsync(timeout.TotalIntMilliseconds(), token); }
		[NotNull]
		public Task<bool> WatchAsync(int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (IsSignaled) return Task.FromResult(true);
			if (Token.IsCancellationRequested || token.IsCancellationRequested) return Task.FromResult(false);
			return TaskHelper.FromWaitHandle(_workCompletedEvent.WaitHandle, millisecondsTimeout, token);
		}

		private void SendWatcherCallback(object state) { _watcherCallback(this, (TArgs)state); }
	}

	public class EventWatcher<TSource> : EventWatcher<TSource, EventArgs>
	{
		/// <inheritdoc />
		protected internal EventWatcher([NotNull] TSource target, [NotNull] EventInfo eventInfo, Action<EventWatcher<TSource, EventArgs>, EventArgs> watcherCallback)
			: base(target, eventInfo, watcherCallback)
		{
		}
	}

	public class EventWatcher : EventWatcher<object, EventArgs>
	{
		/// <inheritdoc />
		private protected EventWatcher([NotNull] object target, [NotNull] EventInfo eventInfo, Action<EventWatcher<object, EventArgs>, EventArgs> watcherCallback)
			: base(target, eventInfo, watcherCallback)
		{
		}

		[NotNull]
		public static EventWatcher Create([NotNull] WaitForEventSettings settings)
		{
			return new EventWatcher(settings.Target, settings.EventInfo, settings.WatcherCallback);
		}

		[NotNull]
		public static EventWatcher<TSource> Create<TSource>([NotNull] WaitForEventSettings<TSource> settings)
		{
			return new EventWatcher<TSource>(settings.Target, settings.EventInfo, settings.WatcherCallback);
		}

		[NotNull]
		public static EventWatcher<TSource, TArgs> Create<TSource, TArgs>([NotNull] WaitForEventSettings<TSource, TArgs> settings)
			where TArgs : EventArgs
		{
			return new EventWatcher<TSource, TArgs>(settings.Target, settings.EventInfo, settings.WatcherCallback);
		}
	}
}
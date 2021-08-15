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
		private readonly Delegate _listener;

		private ManualResetEventSlim _manualResetEventSlim;
		private CancellationTokenSource _cts;
		private bool _completed;

		/// <inheritdoc />
		protected internal EventWatcher([NotNull] TSource target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<TSource, TArgs>, TArgs> watcherCallback)
		{
			Type type = target.GetType();
			if (eventInfo.DeclaringType == null || !eventInfo.DeclaringType.IsAssignableFrom(type)) throw new MemberAccessException($"Type '{type}' does not contain a definition for event '{eventInfo.Name}'.");
			Target = target;
			EventInfo = eventInfo;
			WatcherCallback = watcherCallback;
			_manualResetEventSlim = new ManualResetEventSlim(false);
			_cts = new CancellationTokenSource();
			Token = _cts.Token;
			_listener = typeof(TArgs) == typeof(EventArgs)
							? EventInfo.CreateDelegate((_, args) =>
							{
								Completed = true;
								WatcherCallback.Invoke(this, (TArgs)args);
							})
							: EventInfo.CreateDelegate<TArgs>((_, args) =>
							{
								Completed = true;
								WatcherCallback.Invoke(this, args);
							});
			EventInfo.AddEventHandler(Target, _listener);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
				EventInfo.RemoveEventHandler(Target, _listener);
				ObjectHelper.Dispose(ref _manualResetEventSlim);
				ObjectHelper.Dispose(ref _cts);
			}
			base.Dispose(disposing);
		}

		[NotNull]
		protected Action<EventWatcher<TSource, TArgs>, TArgs> WatcherCallback { get; }
		
		[NotNull]
		public TSource Target { get; }

		[NotNull]
		public EventInfo EventInfo { get; }

		public bool Completed
		{
			get => _completed;
			private set
			{
				_completed = value;
				_manualResetEventSlim.Set();
			}
		}

		public CancellationToken Token { get; }

		public void Stop(bool enforce = false)
		{
			if (!enforce) Wait(TimeSpanHelper.INFINITE);
			_cts.CancelIfNotDisposed();
		}

		public bool Wait() { return Wait(TimeSpanHelper.INFINITE); }

		public bool Wait(TimeSpan timeout) { return Wait(timeout.TotalIntMilliseconds()); }

		public bool Wait(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (Token.IsCancellationRequested) return false;
			if (Completed) return true;

			try
			{
				if (millisecondsTimeout < TimeSpanHelper.ZERO)
					_manualResetEventSlim.Wait(Token);
				else if (!_manualResetEventSlim.Wait(millisecondsTimeout, Token))
					return false;

				return !Token.IsCancellationRequested && Completed;
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

		[NotNull] 
		public Task<bool> WaitAsync(CancellationToken token = default(CancellationToken)) { return WaitAsync(TimeSpanHelper.INFINITE, token); }

		[NotNull] 
		public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken token = default(CancellationToken)) { return WaitAsync(timeout.TotalIntMilliseconds(), token); }

		[NotNull]
		public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed(), false);
			return Task.Run(() => Wait(millisecondsTimeout), Token);
		}
	}

	public class EventWatcher<TSource> : EventWatcher<TSource, EventArgs>
	{
		/// <inheritdoc />
		protected internal EventWatcher([NotNull] TSource target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<TSource, EventArgs>, EventArgs> watcherCallback)
			: base(target, eventInfo, watcherCallback)
		{
		}
	}

	public class EventWatcher : EventWatcher<object, EventArgs>
	{
		/// <inheritdoc />
		private protected EventWatcher([NotNull] object target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<object, EventArgs>, EventArgs> watcherCallback)
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
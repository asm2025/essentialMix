using System;
using System.Reflection;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Threading
{
	public class WaitForEventSettings<TSource, TArgs>
		where TArgs : EventArgs
	{
		private TimeSpan _timeout;

		protected internal WaitForEventSettings([NotNull] TSource target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<TSource, TArgs>, TArgs> watcherCallback)
		{
			Target = target;
			EventInfo = eventInfo;
			WatcherCallback = watcherCallback;
		}

		[NotNull]
		public TSource Target { get; set; }

		[NotNull]
		public EventInfo EventInfo { get; set; }

		[NotNull]
		public Action<EventWatcher<TSource, TArgs>, TArgs> WatcherCallback { get; }

		public TimeSpan Timeout
		{
			get => _timeout;
			set
			{
				if (value.TotalMilliseconds < -1.0d) throw new ArgumentOutOfRangeException(nameof(value));
				_timeout = value;
			}
		}
	}

	public class WaitForEventSettings<TSource> : WaitForEventSettings<TSource, EventArgs>
	{
		/// <inheritdoc />
		protected internal WaitForEventSettings([NotNull] TSource target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<TSource, EventArgs>, EventArgs> watcherCallback) 
			: base(target, eventInfo, watcherCallback)
		{
		}
	}

	public class WaitForEventSettings : WaitForEventSettings<object, EventArgs>
	{
		/// <inheritdoc />
		private protected WaitForEventSettings([NotNull] object target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<object, EventArgs>, EventArgs> watcherCallback) 
			: base(target, eventInfo, watcherCallback)
		{
		}

		[NotNull]
		public static WaitForEventSettings<TSource, TArgs> Create<TSource, TArgs>([NotNull] TSource target, [NotNull] string eventName, [NotNull] Action<EventWatcher<TSource, TArgs>, TArgs> watcherCallback)
			where TArgs : EventArgs
		{
			return Create(target, GetEventInfo(target, eventName), watcherCallback);
		}

		[NotNull]
		public static WaitForEventSettings<TSource, TArgs> Create<TSource, TArgs>([NotNull] TSource target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<TSource, TArgs>, TArgs> watcherCallback)
			where TArgs : EventArgs
		{
			return new WaitForEventSettings<TSource, TArgs>(target, eventInfo, watcherCallback);
		}

		[NotNull]
		public static WaitForEventSettings<TSource> Create<TSource>([NotNull] TSource target, [NotNull] string eventName, [NotNull] Action<EventWatcher<TSource, EventArgs>, EventArgs> watcherCallback)
		{
			return Create(target, GetEventInfo(target, eventName), watcherCallback);
		}

		[NotNull]
		public static WaitForEventSettings<TSource> Create<TSource>([NotNull] TSource target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<TSource, EventArgs>, EventArgs> watcherCallback)
		{
			return new WaitForEventSettings<TSource>(target, eventInfo, watcherCallback);
		}

		[NotNull]
		public static WaitForEventSettings Create([NotNull] object target, [NotNull] string eventName, [NotNull] Action<EventWatcher<object, EventArgs>, EventArgs> watcherCallback)
		{
			return Create(target, GetEventInfo(target, eventName), watcherCallback);
		}

		[NotNull]
		public static WaitForEventSettings Create([NotNull] object target, [NotNull] EventInfo eventInfo, [NotNull] Action<EventWatcher<object, EventArgs>, EventArgs> watcherCallback)
		{
			return new WaitForEventSettings(target, eventInfo, watcherCallback);
		}

		[NotNull]
		private static EventInfo GetEventInfo<T>([NotNull] T target, [NotNull] string eventName)
		{
			return target.GetType().GetEvent(eventName, Constants.BF_PUBLIC_INSTANCE) ?? throw new ArgumentOutOfRangeException(nameof(eventName));
		}
	}
}
using System;
using System.Reflection;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Threading
{
	public class WaitForEventSettings<TSource, TArgs>
		where TArgs : EventArgs
	{
		private TimeSpan _timeout;

		/// <inheritdoc />
		protected internal WaitForEventSettings([NotNull] TSource target, [NotNull] EventInfo eventInfo)
		{
			Target = target;
			EventInfo = eventInfo;
		}

		[NotNull]
		public TSource Target { get; set; }

		[NotNull]
		public EventInfo EventInfo { get; set; }

		public TimeSpan Timeout
		{
			get => _timeout;
			set
			{
				if (value < TimeSpanHelper.Infinite) throw new ArgumentOutOfRangeException(nameof(value));
				_timeout = value;
			}
		}

		public Action<TSource, TArgs> OnEvent { get; set; }
	}

	public class WaitForEventSettings<TSource> : WaitForEventSettings<TSource, EventArgs>
	{
		/// <inheritdoc />
		protected internal WaitForEventSettings([NotNull] TSource target, [NotNull] EventInfo eventInfo) 
			: base(target, eventInfo)
		{
		}
	}

	public class WaitForEventSettings : WaitForEventSettings<object, EventArgs>
	{
		/// <inheritdoc />
		private protected WaitForEventSettings([NotNull] object target, [NotNull] EventInfo eventInfo) 
			: base(target, eventInfo)
		{
		}

		[NotNull]
		public static WaitForEventSettings<TSource, TArgs> Create<TSource, TArgs>([NotNull] TSource target, [NotNull] string eventName)
			where TArgs : EventArgs
		{
			return Create<TSource, TArgs>(target, GetEventInfo(target, eventName));
		}

		[NotNull]
		public static WaitForEventSettings<TSource, TArgs> Create<TSource, TArgs>([NotNull] TSource target, [NotNull] EventInfo eventInfo)
			where TArgs : EventArgs
		{
			return new WaitForEventSettings<TSource, TArgs>(target, eventInfo);
		}

		[NotNull]
		public static WaitForEventSettings<TSource> Create<TSource>([NotNull] TSource target, [NotNull] string eventName)
		{
			return Create(target, GetEventInfo(target, eventName));
		}

		[NotNull]
		public static WaitForEventSettings<TSource> Create<TSource>([NotNull] TSource target, [NotNull] EventInfo eventInfo)
		{
			return new WaitForEventSettings<TSource>(target, eventInfo);
		}

		[NotNull]
		public static WaitForEventSettings Create([NotNull] object target, [NotNull] string eventName)
		{
			return Create(target, GetEventInfo(target, eventName));
		}

		[NotNull]
		public static WaitForEventSettings Create([NotNull] object target, [NotNull] EventInfo eventInfo)
		{
			return new WaitForEventSettings(target, eventInfo);
		}

		[NotNull]
		private static EventInfo GetEventInfo<T>([NotNull] T target, [NotNull] string eventName)
		{
			return target.GetType().GetEvent(eventName, Constants.BF_PUBLIC_INSTANCE) ?? throw new ArgumentOutOfRangeException(nameof(eventName));
		}
	}
}
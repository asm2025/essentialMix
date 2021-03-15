using System;
using JetBrains.Annotations;
using essentialMix.Events;

namespace essentialMix.Extensions
{
	public static class EventHandlerExtension
	{
		[NotNull]
		public static EventArgs<T> CreateArgs<T>([NotNull] this EventHandler<EventArgs<T>> thisValue, T argument) { return new EventArgs<T>(argument); }

		[NotNull]
		public static ReadOnlyEventArgs<T> CreateArgs<T>([NotNull] this EventHandler<ReadOnlyEventArgs<T>> thisValue, T argument) { return new ReadOnlyEventArgs<T>(argument); }
	}
}
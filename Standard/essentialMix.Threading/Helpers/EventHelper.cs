using System;
using System.Threading;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Threading.Helpers
{
	public static class EventHelper
	{
		public static bool WaitForEvent<TSource>([NotNull] WaitForEventSettings<TSource> settings)
		{
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource> watcher = EventWatcher.Create(settings);
			return watcher.Wait(settings.Timeout.TotalIntMilliseconds());
		}

		public static bool WaitForEvent<TSource>([NotNull] WaitForEventSettings<TSource> settings, CancellationToken token)
		{
			// Short-circuit: already cancelled
			if (token.IsCancellationRequested) return false;
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource> watcher = EventWatcher.Create(settings);
			return watcher.Wait(settings.Timeout.TotalIntMilliseconds(), token);
		}

		public static bool WaitForEvent<TSource, TArgs>([NotNull] WaitForEventSettings<TSource, TArgs> settings)
			where TArgs : EventArgs
		{
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource, TArgs> watcher = EventWatcher.Create(settings);
			return watcher.Wait(settings.Timeout.TotalIntMilliseconds());
		}

		public static bool WaitForEvent<TSource, TArgs>([NotNull] WaitForEventSettings<TSource, TArgs> settings, CancellationToken token)
			where TArgs : EventArgs
		{
			// Short-circuit: already cancelled
			if (token.IsCancellationRequested) return false;
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource, TArgs> watcher = EventWatcher.Create(settings);
			return watcher.Wait(settings.Timeout.TotalIntMilliseconds(), token);
		}
	}
}
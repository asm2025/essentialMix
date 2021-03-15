using System;
using System.Threading;
using System.Threading.Tasks;
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

		public static bool WaitForEvent<TSource, TArgs>([NotNull] WaitForEventSettings<TSource, TArgs> settings)
			where TArgs : EventArgs
		{
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource, TArgs> watcher = EventWatcher.Create(settings);
			return watcher.Wait(settings.Timeout.TotalIntMilliseconds());
		}

		[NotNull]
		public static Task<bool> WaitForEventAsync<TSource>([NotNull] WaitForEventSettings<TSource> settings, CancellationToken token = default(CancellationToken))
		{
			// Short-circuit: already cancelled
			if (token.IsCancellationRequested) return Task.FromResult(false);
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource> watcher = EventWatcher.Create(settings);
			return watcher.WaitAsync(settings.Timeout.TotalIntMilliseconds(), token);
		}

		[NotNull]
		public static Task<bool> WaitForEventAsync<TSource, TArgs>([NotNull] WaitForEventSettings<TSource, TArgs> settings, CancellationToken token = default(CancellationToken))
			where TArgs : EventArgs
		{
			// Short-circuit: already cancelled
			if (token.IsCancellationRequested) return Task.FromResult(false);
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource, TArgs> watcher = EventWatcher.Create(settings);
			return watcher.WaitAsync(settings.Timeout.TotalIntMilliseconds(), token);
		}
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Patterns;
using asm.Threading;

namespace asm.Helpers
{
	public static class EventHelper
	{
		public static bool WaitForEvent<TSource>([NotNull] WaitForEventSettings<TSource> settings)
		{
			return WaitForEvent<TSource, EventArgs>(settings);
		}

		public static bool WaitForEvent<TSource, TArgs>([NotNull] WaitForEventSettings<TSource, TArgs> settings)
			where TArgs : EventArgs
		{
			// WARNING: DO NOT DISPOSE THIS
			EventWatcher<TSource, TArgs> watcher = EventWatcher.Create(settings);
			return watcher.Wait(settings.Timeout.TotalIntMilliseconds());
		}

		public static Task<bool> WaitForEventAsync<TSource>([NotNull] WaitForEventSettings<TSource> settings, CancellationToken token = default(CancellationToken))
		{
			return WaitForEventAsync<TSource, EventArgs>(settings, token);
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
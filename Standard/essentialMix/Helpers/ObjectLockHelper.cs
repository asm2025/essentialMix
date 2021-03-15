using System;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public static class ObjectLockHelper
	{
		public static void Pulse([NotNull] Func<bool> checkCallback, [NotNull] object syncLock)
		{
			if (!checkCallback()) return;
			
			lock(syncLock)
			{
				if (!checkCallback()) return;
				Monitor.PulseAll(syncLock);
			}
		}

		public static void WaitFor([NotNull] Func<bool> checkCallback, [NotNull] object syncLock)
		{
			if (checkCallback()) return;

			lock(syncLock)
			{
				if (checkCallback()) return;
				Monitor.Wait(syncLock);
			}
		}

		public static bool WaitFor([NotNull] Func<bool> checkCallback, [NotNull] object syncLock, TimeSpan timeout)
		{
			if (timeout.TotalMilliseconds < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(timeout));
			if (checkCallback()) return true;

			lock(syncLock)
			{
				return checkCallback() || Monitor.Wait(syncLock, timeout);
			}
		}

		public static bool WaitFor([NotNull] Func<bool> checkCallback, [NotNull] object syncLock, int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (checkCallback()) return true;

			lock(syncLock)
			{
				return checkCallback() || Monitor.Wait(syncLock, millisecondsTimeout);
			}
		}
	}
}
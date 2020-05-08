using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class CountdownEventExtension
	{
		public static void SignalOne([NotNull] this CountdownEvent thisValue) { SignalBy(thisValue, 1); }
		public static void SignalBy([NotNull] this CountdownEvent thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count == 0) return;
			if (thisValue.CurrentCount == 0) return;

			lock (thisValue)
			{
				if (thisValue.CurrentCount == 0) return;
				thisValue.Signal(count.NotAbove(thisValue.CurrentCount));
			}
		}

		public static void SignalAll([NotNull] this CountdownEvent thisValue)
		{
			if (thisValue.CurrentCount == 0) return;

			lock (thisValue)
			{
				if (thisValue.CurrentCount == 0) return;
				thisValue.Signal(thisValue.CurrentCount);
			}
		}
	}
}
using System;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class CountdownEventExtension
	{
		public static bool SignalOne([NotNull] this CountdownEvent thisValue) { return SignalBy(thisValue, 1); }
		public static bool SignalBy([NotNull] this CountdownEvent thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count == 0) return thisValue.CurrentCount == 0;
			return thisValue.CurrentCount == 0 || thisValue.Signal(count.NotAbove(thisValue.CurrentCount));
		}

		public static bool SignalAll([NotNull] this CountdownEvent thisValue)
		{
			return thisValue.CurrentCount == 0 || thisValue.Signal(thisValue.CurrentCount);
		}
	}
}
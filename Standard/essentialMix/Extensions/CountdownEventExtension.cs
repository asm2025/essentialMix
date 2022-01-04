using System;
using System.Threading;

namespace essentialMix.Extensions;

public static class CountdownEventExtension
{
	public static bool SignalOne(this CountdownEvent thisValue) { return SignalBy(thisValue, 1); }
	public static bool SignalBy(this CountdownEvent thisValue, int count)
	{
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		int current = thisValue?.CurrentCount ?? 0;
		return current < 1 || thisValue?.Signal(count.NotAbove(current)) == true;
	}

	public static bool SignalAll(this CountdownEvent thisValue)
	{
		return thisValue == null || thisValue.CurrentCount < 1 || thisValue.Signal(thisValue.CurrentCount);
	}
}
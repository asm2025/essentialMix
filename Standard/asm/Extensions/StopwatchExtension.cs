using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class StopwatchExtension
	{
		public static long Measure([NotNull] this Stopwatch thisValue, [NotNull] Action action)
		{
			thisValue.Restart();

			try
			{
				action();
			}
			finally
			{
				thisValue.Stop();
			}

			return thisValue.ElapsedTicks;
		}

		public static long Measure<TResult>([NotNull] this Stopwatch thisValue, [NotNull] Func<TResult> func, out TResult result)
		{
			thisValue.Restart();

			try
			{
				result = func();
			}
			finally
			{
				thisValue.Stop();
			}

			return thisValue.ElapsedTicks;
		}
	}
}
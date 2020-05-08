using System.Threading;

namespace asm.Extensions
{
	public static class ThreadStateExtension
	{
		public static bool IsReady(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.Unstarted); }

		public static bool IsRunning(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.Running) || IsWaiting(thisValue); }

		public static bool IsSuspending(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.SuspendRequested); }

		public static bool IsSuspended(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.Suspended); }

		public static bool IsStopping(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.StopRequested) || thisValue.HasFlag(ThreadState.AbortRequested); }

		public static bool IsStopped(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.Stopped); }

		public static bool IsStarted(this ThreadState thisValue) { return !thisValue.HasFlag(ThreadState.Unstarted); }

		public static bool IsWaiting(this ThreadState thisValue)
		{
			return thisValue.HasFlag(ThreadState.StopRequested) ||
					thisValue.HasFlag(ThreadState.SuspendRequested) ||
					thisValue.HasFlag(ThreadState.WaitSleepJoin) ||
					thisValue.HasFlag(ThreadState.AbortRequested);
		}

		public static bool IsFinished(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.Stopped) || thisValue.HasFlag(ThreadState.Aborted); }
	}
}
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ThreadStateExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsReady(this ThreadState thisValue) { return (thisValue & ThreadState.Unstarted) != 0; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsRunning(this ThreadState thisValue) { return thisValue == ThreadState.Running || IsWaiting(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsSuspending(this ThreadState thisValue) { return (thisValue & ThreadState.SuspendRequested) != 0; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsSuspended(this ThreadState thisValue) { return (thisValue & ThreadState.Suspended) != 0; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsStopping(this ThreadState thisValue) { return (thisValue & ThreadState.StopRequested) != 0 || (thisValue & ThreadState.AbortRequested) != 0; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsStopped(this ThreadState thisValue) { return (thisValue & ThreadState.Stopped) != 0; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsStarted(this ThreadState thisValue) { return (thisValue & ThreadState.Unstarted) == 0; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsWaiting(this ThreadState thisValue)
		{
			return (thisValue & ThreadState.StopRequested) != 0 ||
					(thisValue & ThreadState.SuspendRequested) != 0 ||
					(thisValue & ThreadState.WaitSleepJoin) != 0 ||
					(thisValue & ThreadState.AbortRequested) != 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsFinished(this ThreadState thisValue) { return thisValue.HasFlag(ThreadState.Stopped) || thisValue.HasFlag(ThreadState.Aborted); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ThreadState Simplify(this ThreadState thisValue)
		{
			return thisValue & (ThreadState.Unstarted | ThreadState.WaitSleepJoin | ThreadState.Stopped);
		}
	}
}
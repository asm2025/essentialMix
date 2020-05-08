using System.Security.Permissions;
using System.Threading;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class ThreadExtension
	{
		public static bool IsAwaitable(this Thread thisValue) { return thisValue != null && !IsFinished(thisValue); }

		public static bool IsReady(this Thread thisValue) { return thisValue != null && thisValue.ThreadState.IsReady(); }

		public static bool IsRunning(this Thread thisValue) { return thisValue != null && thisValue.ThreadState.IsRunning(); }

		public static bool IsSuspending([NotNull] this Thread thisValue) { return thisValue.ThreadState.IsSuspending(); }

		public static bool IsSuspended([NotNull] this Thread thisValue) { return thisValue.ThreadState.IsSuspended(); }

		public static bool IsStopping([NotNull] this Thread thisValue) { return thisValue.ThreadState.IsStopping(); }

		public static bool IsStopped(this Thread thisValue) { return thisValue == null || thisValue.ThreadState.IsStopped(); }

		public static bool IsStarted(this Thread thisValue) { return thisValue != null && thisValue.ThreadState.IsStarted(); }

		public static bool IsWaiting([NotNull] this Thread thisValue) { return thisValue.ThreadState.IsWaiting(); }

		public static bool IsFinished(this Thread thisValue) { return thisValue != null && thisValue.ThreadState.IsFinished(); }

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlThread)]
		public static void Awake([NotNull] this Thread thisValue)
		{
			if (IsSuspending(thisValue)) Thread.Sleep(TimeSpanHelper.SCHEDULE);
			if (!IsSuspended(thisValue)) return;

			try
			{
				thisValue.Interrupt();
			}
			catch (ThreadInterruptedException)
			{
			}
		}

		/// <summary>
		/// This is a terrible idea, might want to try each and every other solution that might work before coming here.
		/// </summary>
		/// <param name="thisValue">The thread.</param>
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlThread)]
		public static bool Die(this Thread thisValue)
		{
			if (!IsAwaitable(thisValue)) return true;
			if (IsWaiting(thisValue)) Thread.Sleep(TimeSpanHelper.SCHEDULE);
			if (IsStopped(thisValue)) return true;
			if (IsSuspended(thisValue)) Awake(thisValue);

			try
			{
				thisValue.Abort();
				while (thisValue.IsAlive) { }
				return true;
			}
			catch (ThreadAbortException)
			{
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
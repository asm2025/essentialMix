using System;
using System.Threading;

namespace essentialMix.Threading
{
	public class SynchronizedThreadsOptions<T>
	{
		public bool WaitOnDispose { get; set; }
		public bool IsBackground { get; set; } = true;
		public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
		public Action<SynchronizedThreads<T>> WorkStartedCallback { get; set; }
		public Action<SynchronizedThreads<T>> WorkCompletedCallback { get; set; }
	}
}
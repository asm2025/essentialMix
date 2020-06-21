using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.MessageQueue
{
	public interface IQueue<in T>
	{
		[NotNull]
		Action<T> Callback { get; }

		bool IsBackground { get; }
		ThreadPriority Priority { get; }
		bool CompleteMarked { get; }
		bool IsBusy { get; }
		CancellationToken Token { get; }
		bool WaitForQueuedItems { get; set; }
		
		void Enqueue([NotNull] T item);
		void Complete();
		void Clear();
		void Stop();
		void Stop(bool waitForQueue);
		bool Wait();
		bool Wait(TimeSpan timeout);
		bool Wait(int millisecondsTimeout);
		Task<bool> WaitAsync();
		Task<bool> WaitAsync(TimeSpan timeout);
		Task<bool> WaitAsync(int millisecondsTimeout);
	}
}
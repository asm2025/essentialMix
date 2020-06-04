using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface IProducerConsumer<in T> : IDisposable
	{
		bool CompleteMarked { get; }
		int Count { get; }
		bool IsBusy { get; }
		int Threads { get; }
		CancellationToken Token { get; }
		bool WaitOnDispose { get; set; }
		int SleepAfterEnqueue { get; set; }

		event EventHandler WorkStarted;
		event EventHandler WorkCompleted;

		void Enqueue([NotNull] T item);
		void Complete();
		void Clear();
		void Stop();
		void Stop(bool enforce);
		bool Wait();
		bool Wait(TimeSpan timeout);
		bool Wait(int millisecondsTimeout);
		Task<bool> WaitAsync();
		Task<bool> WaitAsync(TimeSpan timeout);
		Task<bool> WaitAsync(int millisecondsTimeout);
	}
}
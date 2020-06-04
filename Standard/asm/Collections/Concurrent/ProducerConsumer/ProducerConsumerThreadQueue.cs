using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public abstract class ProducerConsumerThreadQueue<T> : ProducerConsumerQueue<T>, IProducerConsumerThreadQueue<T>, IDisposable
	{
		protected ProducerConsumerThreadQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			if (!(options is ProducerConsumerThreadQueueOptions<T> threadQueueOptions)) return;
			IsBackground = threadQueueOptions.IsBackground;
			Priority = threadQueueOptions.Priority;
		}

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }
	}
}
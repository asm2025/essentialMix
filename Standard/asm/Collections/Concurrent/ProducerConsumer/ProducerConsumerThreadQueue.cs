using System;
using System.Threading;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public abstract class ProducerConsumerThreadQueue : ProducerConsumerQueue, IProducerConsumerThreadQueue, IDisposable
	{
		protected ProducerConsumerThreadQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		protected ProducerConsumerThreadQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			if (options is ProducerConsumerThreadQueueOptions threadQueueOptions)
			{
				IsBackground = threadQueueOptions.IsBackground;
				Priority = threadQueueOptions.Priority;
			}
		}

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }
	}
}
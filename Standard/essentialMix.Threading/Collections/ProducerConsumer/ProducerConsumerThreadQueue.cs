using System;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer
{
	public abstract class ProducerConsumerThreadQueue<T> : ProducerConsumerQueue<T>, IProducerConsumerThreadQueue<T>, IDisposable
	{
		protected ProducerConsumerThreadQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			if (options is not ProducerConsumerThreadQueueOptions<T> threadQueueOptions) return;
			IsBackground = threadQueueOptions.IsBackground;
			Priority = threadQueueOptions.Priority;
		}

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }
	}
}
using System;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public abstract class ProducerConsumerThreadQueue<T> : ProducerConsumerQueue<T>, IProducerConsumerThreadQueue<T>, IDisposable
	{
		protected ProducerConsumerThreadQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			if (options is ProducerConsumerThreadQueueOptions<T> threadQueueOptions)
			{
				IsBackground = threadQueueOptions.IsBackground;
				Priority = threadQueueOptions.Priority;
			}
			else
			{
				IsBackground = true;
				Priority = ThreadPriority.Normal;
			}
		}

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }
	}
}
using System;
using System.Threading;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public abstract class ProducerConsumerThresholdQueue : ProducerConsumerThreadQueue, IProducerConsumerThresholdQueue, IDisposable
	{
		protected ProducerConsumerThresholdQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		protected ProducerConsumerThresholdQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			ThresholdInternal = options is ProducerConsumerThresholdQueueOptions thresholdQueueOptions ? thresholdQueueOptions.Threshold : TimeSpan.Zero;
			HasThreshold = ThresholdInternal > TimeSpan.Zero;
		}

		public TimeSpan Threshold => ThresholdInternal;

		public bool HasThreshold { get; }

		protected TimeSpan ThresholdInternal { get; }
	}
}
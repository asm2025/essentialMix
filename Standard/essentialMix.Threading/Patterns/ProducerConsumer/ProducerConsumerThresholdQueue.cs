using System;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer;

public abstract class ProducerConsumerThresholdQueue<T> : ProducerConsumerThreadQueue<T>, IProducerConsumerThresholdQueue<T>
{
	protected ProducerConsumerThresholdQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		: base(options, token)
	{
		Threshold = options is ProducerConsumerThresholdQueueOptions<T> thresholdQueueOptions ? thresholdQueueOptions.Threshold : TimeSpan.Zero;
		HasThreshold = Threshold > TimeSpan.Zero;
	}

	public TimeSpan Threshold { get; }

	public bool HasThreshold { get; }
}
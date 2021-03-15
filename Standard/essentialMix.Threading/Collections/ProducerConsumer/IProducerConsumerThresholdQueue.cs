using System;

namespace essentialMix.Threading.Collections.ProducerConsumer
{
	public interface IProducerConsumerThresholdQueue<in T> : IProducerConsumerThreadQueue<T>
	{
		TimeSpan Threshold { get; }
		bool HasThreshold { get; }
	}
}
using System;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface IProducerConsumerThresholdQueue<in T> : IProducerConsumerThreadQueue<T>
	{
		TimeSpan Threshold { get; }
		bool HasThreshold { get; }
	}
}
using System;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface IProducerConsumerThresholdQueue : IProducerConsumerThreadQueue
	{
		TimeSpan Threshold { get; }
		bool HasThreshold { get; }
	}
}
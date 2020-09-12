using System;

namespace asm.Threading.Collections.ProducerConsumer
{
	public interface IProducerConsumerThresholdQueue<in T> : IProducerConsumerThreadQueue<T>
	{
		TimeSpan Threshold { get; }
		bool HasThreshold { get; }
	}
}
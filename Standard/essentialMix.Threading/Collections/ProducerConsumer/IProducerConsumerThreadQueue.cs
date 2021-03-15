using System.Threading;

namespace essentialMix.Threading.Collections.ProducerConsumer
{
	public interface IProducerConsumerThreadQueue<in T> : IProducerConsumer<T>
	{
		bool IsBackground { get; }
		ThreadPriority Priority { get; }
	}
}
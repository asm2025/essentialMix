using System.Threading;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public interface IProducerConsumerThreadQueue<in T> : IProducerConsumer<T>
	{
		bool IsBackground { get; }
		ThreadPriority Priority { get; }
	}
}
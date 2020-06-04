using System.Threading;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface IProducerConsumerThreadQueue<in T> : IProducerConsumer<T>
	{
		bool IsBackground { get; }
		ThreadPriority Priority { get; }
	}
}
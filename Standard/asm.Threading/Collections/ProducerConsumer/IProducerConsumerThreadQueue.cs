using System.Threading;

namespace asm.Threading.Collections.ProducerConsumer
{
	public interface IProducerConsumerThreadQueue<in T> : IProducerConsumer<T>
	{
		bool IsBackground { get; }
		ThreadPriority Priority { get; }
	}
}
using System.Threading;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface IProducerConsumerThreadQueue : IProducerConsumer
	{
		bool IsBackground { get; }
		ThreadPriority Priority { get; }
	}
}
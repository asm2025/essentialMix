using System.Threading;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerThreadQueueOptions : ProducerConsumerQueueOptions
	{
		public bool IsBackground { get; set; }

		public ThreadPriority Priority { get; set; }
	}
}
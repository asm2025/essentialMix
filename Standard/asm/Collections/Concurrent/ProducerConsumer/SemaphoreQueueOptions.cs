using System.Security.AccessControl;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class SemaphoreQueueOptions : ProducerConsumerThreadNamedQueueOptions
	{
		public SemaphoreSecurity Security { get; set; }
	}
}
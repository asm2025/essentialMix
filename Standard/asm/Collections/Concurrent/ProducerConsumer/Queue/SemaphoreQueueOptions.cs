using System;
using System.Security.AccessControl;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer.Queue
{
	public class SemaphoreQueueOptions<T> : ProducerConsumerThreadNamedQueueOptions<T>
	{
		/// <inheritdoc />
		public SemaphoreQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
			: base(threads, executeCallback)
		{
		}

		public SemaphoreSecurity Security { get; set; }
	}
}
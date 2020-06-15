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

		/// <summary>
		/// To create a named semaphore with access control security, see this <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphore.openexisting">example</see>
		/// </summary>
		public SemaphoreSecurity Security { get; set; }
	}
}
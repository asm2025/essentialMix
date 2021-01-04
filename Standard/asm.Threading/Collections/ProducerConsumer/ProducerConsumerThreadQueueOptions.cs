using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Threading.Collections.ProducerConsumer
{
	public class ProducerConsumerThreadQueueOptions<T> : ProducerConsumerQueueOptions<T>
	{
		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
			: base(threads, executeCallback)
		{
		}

		public bool IsBackground { get; set; } = true;

		public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
	}
}
using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public class CircularBufferQueueOptions<T> : ProducerConsumerThreadQueueOptions<T>
	{
		/// <inheritdoc />
		public CircularBufferQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public CircularBufferQueueOptions(int capacity, [NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
			Capacity = capacity;
		}

		/// <inheritdoc />
		public CircularBufferQueueOptions(int threads, int capacity, [NotNull] Func<T, TaskResult> executeCallback)
			: base(threads, executeCallback)
		{
			Capacity = capacity;
		}

		public int Capacity { get; set; }
	}
}
using System;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public class ProducerConsumerThreadQueueOptions<T> : ProducerConsumerQueueOptions<T>
	{
		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions([NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(int threads, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions([NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(bool waitOnDispose, [NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(int threads, [NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadQueueOptions(int threads, bool waitOnDispose, [NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		public bool IsBackground { get; set; } = true;

		public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
	}
}
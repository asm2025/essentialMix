using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public class ProducerConsumerThreadNamedQueueOptions<T> : ProducerConsumerThreadQueueOptions<T>
	{
		private string _name;

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions([NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(int threads, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions([NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(bool waitOnDispose, [NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(int threads, [NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(int threads, bool waitOnDispose, [NotNull] Func<IProducerConsumer<T>, T, TaskResult> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		public string Name
		{
			get => _name;
			set
			{
				_name = value?.Trim();
				if (string.IsNullOrEmpty(_name)) _name = null;
			}
		}
	}
}
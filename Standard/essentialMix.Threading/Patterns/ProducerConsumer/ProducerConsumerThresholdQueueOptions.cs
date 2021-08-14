using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public class ProducerConsumerThresholdQueueOptions<T> : ProducerConsumerThreadQueueOptions<T>
	{
		private TimeSpan _threshold = TimeSpan.Zero;

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions([NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}


		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions([NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(bool waitOnDispose, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, bool waitOnDispose, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		public TimeSpan Threshold
		{
			get => _threshold;
			set
			{
				if (value < TimeSpan.Zero) value = TimeSpan.Zero;
				_threshold = value;
			}
		}
	}
}
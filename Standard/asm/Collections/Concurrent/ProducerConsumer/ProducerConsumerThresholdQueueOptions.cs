using System;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerThresholdQueueOptions<T> : ProducerConsumerThreadQueueOptions<T>
	{
		private TimeSpan _threshold = TimeSpan.Zero;

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
			: base(threads, executeCallback)
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
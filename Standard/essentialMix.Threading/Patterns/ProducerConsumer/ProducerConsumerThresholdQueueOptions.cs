﻿using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public class ProducerConsumerThresholdQueueOptions<T> : ProducerConsumerThreadQueueOptions<T>
	{
		private TimeSpan _threshold = TimeSpan.Zero;

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions([NotNull] Action<T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(bool waitOnDispose, [NotNull] Action<T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, [NotNull] Action<T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}


		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(bool waitOnDispose, [NotNull] Func<T, TaskResult> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThresholdQueueOptions(int threads, bool waitOnDispose, [NotNull] Func<T, TaskResult> executeCallback)
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
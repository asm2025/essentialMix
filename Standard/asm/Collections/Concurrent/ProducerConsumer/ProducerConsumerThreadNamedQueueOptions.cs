using System;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerThreadNamedQueueOptions<T> : ProducerConsumerThreadQueueOptions<T>
	{
		private string _name;

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public ProducerConsumerThreadNamedQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
			: base(threads, executeCallback)
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
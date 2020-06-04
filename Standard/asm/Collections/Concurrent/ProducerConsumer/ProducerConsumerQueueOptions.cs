using System;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public class ProducerConsumerQueueOptions<T>
	{
		private int _threads;

		public ProducerConsumerQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: this(TaskHelper.ProcessDefault, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
		{
			Threads = threads;
			ExecuteCallback = executeCallback;
		}

		public int Threads
		{
			get => _threads;
			set
			{
				if (value == 0) value = TaskHelper.ProcessDefault;
				if (!value.InRange(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum)) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(Threads)} must fall within the range of {TaskHelper.QueueMinimum} and {TaskHelper.QueueMaximum}.");
				_threads = value;
			}
		}

		[NotNull]
		public Func<T, TaskResult> ExecuteCallback { get; }
		public Func<T, TaskResult, Exception, bool> ResultCallback { get; set; }
		public Action<T> FinalizeCallback { get; set; }
	}
}
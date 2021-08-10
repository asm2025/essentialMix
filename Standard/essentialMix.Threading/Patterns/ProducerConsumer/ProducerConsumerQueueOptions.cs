using System;
using essentialMix.Extensions;
using essentialMix.Threading.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public class ProducerConsumerQueueOptions<T>
	{
		private int _threads;

		public ProducerConsumerQueueOptions([NotNull] Action<T> executeCallback)
			: this(TaskHelper.ProcessDefault, false, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(bool waitOnDispose, [NotNull] Action<T> executeCallback)
			: this(TaskHelper.ProcessDefault, waitOnDispose, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(int threads, [NotNull] Action<T> executeCallback)
			: this(threads, false, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<T> executeCallback)
		{
			Threads = threads;
			WaitOnDispose = waitOnDispose;
			ExecuteCallback = item =>
			{
				try
				{
					executeCallback(item);
					return TaskResult.Success;
				}
				catch (OperationCanceledException)
				{
					return TaskResult.Canceled;
				}
				catch (TimeoutException)
				{
					return TaskResult.Timeout;
				}
			};
		}

		public ProducerConsumerQueueOptions([NotNull] Func<T, TaskResult> executeCallback)
			: this(TaskHelper.ProcessDefault, false, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(bool waitOnDispose, [NotNull] Func<T, TaskResult> executeCallback)
			: this(TaskHelper.ProcessDefault, waitOnDispose, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(int threads, [NotNull] Func<T, TaskResult> executeCallback)
			: this(threads, false, executeCallback)
		{
		}

		public ProducerConsumerQueueOptions(int threads, bool waitOnDispose, [NotNull] Func<T, TaskResult> executeCallback)
		{
			Threads = threads;
			WaitOnDispose = waitOnDispose;
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

		public bool WaitOnDispose { get; set; }

		[NotNull]
		public Func<T, TaskResult> ExecuteCallback { get; }
		public Func<T, TaskResult, Exception, bool> ResultCallback { get; set; }
		public Action<T> ScheduledCallback { get; set; }
		public Action<T> FinalizeCallback { get; set; }
	}
}
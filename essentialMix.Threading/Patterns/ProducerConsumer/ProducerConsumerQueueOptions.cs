using System;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer;

public class ProducerConsumerQueueOptions<T>
{
	private int _threads;

	public ProducerConsumerQueueOptions([NotNull] Action<IProducerConsumer<T>, T> executeCallback)
		: this(TaskHelper.ProcessDefault, false, executeCallback)
	{
	}

	public ProducerConsumerQueueOptions(bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
		: this(TaskHelper.ProcessDefault, waitOnDispose, executeCallback)
	{
	}

	public ProducerConsumerQueueOptions(int threads, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
		: this(threads, false, executeCallback)
	{
	}

	public ProducerConsumerQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
	{
		Threads = threads;
		WaitOnDispose = waitOnDispose;
		ExecuteCallback = (que, item) =>
		{
			try
			{
				executeCallback(que, item);
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

	public ProducerConsumerQueueOptions([NotNull] ExecuteCallbackDelegates<T> executeCallback)
		: this(TaskHelper.ProcessDefault, false, executeCallback)
	{
	}

	public ProducerConsumerQueueOptions(bool waitOnDispose, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
		: this(TaskHelper.ProcessDefault, waitOnDispose, executeCallback)
	{
	}

	public ProducerConsumerQueueOptions(int threads, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
		: this(threads, false, executeCallback)
	{
	}

	public ProducerConsumerQueueOptions(int threads, bool waitOnDispose, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
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

	public bool SynchronizeContext { get; set; }

	[NotNull]
	public ExecuteCallbackDelegates<T> ExecuteCallback { get; }
	public ResultCallbackDelegates<T> ResultCallback { get; set; }
	public ScheduledCallbackDelegates<T> ScheduledCallback { get; set; }
	public FinalizeCallbackDelegates<T> FinalizeCallback { get; set; }
	public CallbackDelegates<T> WorkStartedCallback { get; set; }
	public CallbackDelegates<T> WorkCompletedCallback { get; set; }
}
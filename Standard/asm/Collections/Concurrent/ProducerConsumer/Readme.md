# Usage
```csharp
// if there is a timeout, will use a CancellationTokenSource.
using (CancellationTokenSource cts = Timeout > 0 ? new CancellationTokenSource(TimeSpan.FromMinutes(Timeout)) : null)
{
	CancellationToken token = cts?.Token ?? CancellationToken.None;

#if DEBUG
	// if in debug mode and LimitThreads is true, use just 1 thread for easier debugging.
	int threads = LimitThreads ? 1 : MaximumThreads;
#else
	// Otherwise, use the default (Best to be TaskHelper.ProcessDefault which = Environment.ProcessorCount)
	int threads = MaximumThreads;
#endif

	if (threads <= 0 || threads > TaskHelper.ProcessDefault) threads = TaskHelper.ProcessDefault;
	
	// This can control time restriction i.e. Number of threads/tasks per second/minute etc.
	TimeSpan threshold = Threshold.Within(TimeSpan.Zero, TimeoutHelper.Hour);
	if (threshold > TimeSpan.Zero && threshold < TimeoutHelper.Second) threshold = TimeoutHelper.Second;

	ThreadQueueMode mode;
	ProducerConsumerQueueOptions options;

	if (threshold == TimeSpan.Zero)
	{
		// This can be any of the thread enqueue modes
		mode = ThreadQueueMode.task;
		options = new ProducerConsumerQueueOptions
		{
			Threads = threads
		};
	}
	else
	{
		// This must be set to thresholdTaskGroup to control number of tasks per time.
		mode = ThreadQueueMode.thresholdTaskGroup;
		options = new ProducerConsumerThresholdQueueOptions
		{
			Threads = threads,
			Threshold = threshold
		};
	}

	// Genericly create a queue producer
	using (IProducerConsumer queue = ProducerConsumerFactory.Create(mode, options, token))
	{
		foreach, while, etc...
		{
			// fill the queue with tasks
			queue.Enqueue(new TaskItem(ti =>
			{
				if (ti.Token.IsCancellationRequested) return TaskQueueResult.Canceled;

				try
				{
					// Your code...
					return ti.Token.IsCancellationRequested
						? TaskQueueResult.Canceled
						: (TaskQueueResult.Success or TaskQueueResult.Error);
				}
				catch
				{
					return TaskQueueResult.Error;
				}
				finally
				{
					// Dispose any objects
				}
			}));
		}

		// Important to inform the queue producer the work is done not to wait for more tasks
		queue.Complete();
		// and then wait for all tasks to finish
		queue.Wait();
	}

	// check if cancellation was triggered
	if (token.IsCancellationRequested) return;
}
```
# Usage
*Important:*
* If you are going to use the DataFlowQueue, you'll need to install System.Threading.Tasks.Dataflow on the main project.
* If you are going to use the SemaphoreQueueOptions with the SemaphoreQueue and set the SemaphoreQueueOptions.Security, you'll need to install System.Security.AccessControl on the main project.

```csharp
// example values
int len = RNGRandomHelper.Next(1000, 5000);
int[] values = new int[GetRandomIntegers(len)];

for (int i = 0; i < len; i++)
{
	values[i] = RNGRandomHelper.Next(1, short.MaxValue);
}

int timeout = RNGRandomHelper.Next(0, 1);
bool limitThreads = RNGRandomHelper.Next(0, 1) != 0;
int maximumThreads = RNGRandomHelper.Next(TaskHelper.QueueMinimum, TaskHelper.QueueMaximum);

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

	if (threads < 1 || threads > TaskHelper.ProcessDefault) threads = TaskHelper.ProcessDefault;
	
	// This can control time restriction i.e. Number of threads/tasks per second/minute etc.
	TimeSpan threshold = Threshold.Within(TimeSpan.Zero, TimeoutHelper.Hour);
	if (threshold > TimeSpan.Zero && threshold < TimeoutHelper.Second) threshold = TimeoutHelper.Second;

	ThreadQueueMode mode;
	ProducerConsumerQueueOptions<int> options;
	Func<int, TaskResult> exec = e =>
	{
		Console.WriteLine(e);
		return TaskResult.Success;
	};

	if (threshold == TimeSpan.Zero)
	{
		// This can be any of the thread enqueue modes
		mode = ThreadQueueMode.task;
		options = new ProducerConsumerQueueOptions<int>(threads, exec);
	}
	else
	{
		// This must be set to thresholdTaskGroup to control number of tasks per time.
		mode = ThreadQueueMode.thresholdTaskGroup;
		options = new ProducerConsumerThresholdQueueOptions<int>(threads, exec)
		{
			Threshold = threshold
		};
	}

	// Create a generic queue producer
	using (IProducerConsumer<int> queue = ProducerConsumerFactory.Create(mode, options, token))
	{
		queue.WorkStarted += (sender, args) =>
		{
			Console.WriteLine();
			Console.WriteLine($"Starting {mode}...");
		};

		queue.WorkCompleted += (sender, args) =>
		{
			Console.WriteLine();
			Console.WriteLine("Finished.");
			Console.WriteLine();
		};

		//foreach, while, etc...
		foreach (int value in values)
		{
			// fill the queue with tasks
			queue.Enqueue(value);
		}

		/*
		* when the queue is being disposed, it will wait until the queued items are processed.
		* this works when queue.WaitOnDispose is true , which it is by default.
		* alternatively, the following can be done to wait for all items to be processed:
		*
		* // Important: marks the completion of queued items, no further items can be queued
		* // after this point. the queue will not to wait for more items other than the already queued.
		* queue.Complete();
		* // wait for the queue to finish
		* queue.Wait();
		*
		* another way to go about it, is not to call queue.Complete(); if this queue will
		* wait indefinitely and maybe use a CancellationTokenSource.
		*
		* for now, the queue will wait for the items to be finished once reached the next
		* dispose curly bracket.
		*/
	}

	// check if cancellation was triggered
	if (token.IsCancellationRequested) return;
}
```
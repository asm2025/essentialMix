namespace essentialMix.Threading.Patterns.ProducerConsumer;

public enum ThreadQueueMode
{
	Task,
	WaitAndPulse,
	Event,
	BlockingCollection,
	TaskGroup,
	SemaphoreSlim,
	Semaphore,
	Mutex,
	ThreadPool,
	ThresholdTaskGroup
}

public static class ThreadQueueModeExtension
{
	public static bool SupportsProducerQueue(this ThreadQueueMode thisValue)
	{
		switch (thisValue)
		{
			case ThreadQueueMode.Task:
			case ThreadQueueMode.WaitAndPulse:
			case ThreadQueueMode.Event:
			case ThreadQueueMode.TaskGroup:
			case ThreadQueueMode.SemaphoreSlim:
			case ThreadQueueMode.Semaphore:
			case ThreadQueueMode.Mutex:
			case ThreadQueueMode.ThreadPool:
			case ThreadQueueMode.ThresholdTaskGroup:
				return true;
			default:
				return false;
		}
	}
}
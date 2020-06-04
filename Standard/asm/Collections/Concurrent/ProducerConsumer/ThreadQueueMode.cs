namespace asm.Collections.Concurrent.ProducerConsumer
{
	public enum ThreadQueueMode
	{
		Task,
		DataFlow,
		WaitAndPulse,
		BlockingCollection,
		TaskGroup,
		SemaphoreSlim,
		Semaphore,
		Mutex,
		ThresholdTaskGroup
	}
}
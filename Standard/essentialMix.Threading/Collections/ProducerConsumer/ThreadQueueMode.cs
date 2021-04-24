namespace essentialMix.Threading.Collections.ProducerConsumer
{
	public enum ThreadQueueMode
	{
		Task,
		DataFlow,
		WaitAndPulse,
		CircularBuffer,
		Deque,
		Event,
		BlockingCollection,
		TaskGroup,
		SemaphoreSlim,
		Semaphore,
		Mutex,
		ThresholdTaskGroup
	}
}
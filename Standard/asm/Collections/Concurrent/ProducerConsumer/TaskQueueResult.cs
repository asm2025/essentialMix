namespace asm.Collections.Concurrent.ProducerConsumer
{
	public enum TaskQueueResult
	{
		None,
		Canceled,
		Timeout,
		Error,
		Success
	}
}
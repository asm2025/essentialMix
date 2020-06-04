namespace asm.Collections.Concurrent.ProducerConsumer
{
	public enum TaskResult
	{
		None,
		Canceled,
		Timeout,
		Error,
		Success
	}
}
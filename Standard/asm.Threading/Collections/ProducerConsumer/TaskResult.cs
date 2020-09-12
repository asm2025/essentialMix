namespace asm.Threading.Collections.ProducerConsumer
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
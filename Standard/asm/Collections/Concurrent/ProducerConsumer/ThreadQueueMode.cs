using System.Diagnostics.CodeAnalysis;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum ThreadQueueMode
	{
		task,
		dataflow,
		waitpulse,
		blockingCollection,
		taskGroup,
		semaphoreslim,
		semaphore,
		thresholdTaskGroup
	}
}
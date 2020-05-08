namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface INamedProducerConsumerThreadQueue : IProducerConsumerThreadQueue
	{
		string Name { get; }
		bool IsOwner { get; }
	}
}
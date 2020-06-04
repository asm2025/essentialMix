namespace asm.Collections.Concurrent.ProducerConsumer
{
	public interface INamedProducerConsumerThreadQueue<in T> : IProducerConsumerThreadQueue<T>
	{
		string Name { get; }
		bool IsOwner { get; }
	}
}
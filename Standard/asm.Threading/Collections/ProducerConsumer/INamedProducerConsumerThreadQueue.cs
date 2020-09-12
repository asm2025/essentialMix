namespace asm.Threading.Collections.ProducerConsumer
{
	public interface INamedProducerConsumerThreadQueue<in T> : IProducerConsumerThreadQueue<T>
	{
		string Name { get; }
		bool IsOwner { get; }
	}
}
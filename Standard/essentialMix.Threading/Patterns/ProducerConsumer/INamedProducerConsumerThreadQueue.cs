namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public interface INamedProducerConsumerThreadQueue<in T> : IProducerConsumerThreadQueue<T>
	{
		string Name { get; }
		bool IsOwner { get; }
	}
}
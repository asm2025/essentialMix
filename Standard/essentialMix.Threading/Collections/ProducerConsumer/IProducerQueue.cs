namespace essentialMix.Threading.Collections.ProducerConsumer
{
	public interface IProducerQueue<T>
	{
		bool TryDequeue(out T item);
		bool TryPeek(out T item);
	}
}

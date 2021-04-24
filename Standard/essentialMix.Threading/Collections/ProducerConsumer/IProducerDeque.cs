namespace essentialMix.Threading.Collections.ProducerConsumer
{
	public interface IProducerDeque<T>
	{
		void Enqueue(T item);
		bool TryDequeue(out T item);
		void Push(T item);
		bool TryPop(out T item);
		bool TryPeekHead(out T item);
		bool TryPeekTail(out T item);
	}
}

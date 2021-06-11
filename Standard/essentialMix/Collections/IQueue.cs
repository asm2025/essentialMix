namespace essentialMix.Collections
{
	public interface IQueue<T>
	{
		void Enqueue(T item);
		T Dequeue();
		void Clear();
	}
}
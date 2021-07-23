namespace essentialMix.Collections
{
	public interface IQueue<T> : IQueueBase<T>
	{
		T Peek();
		bool TryPeek(out T item);
		void Clear();
	}
}
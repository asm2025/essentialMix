namespace essentialMix.Collections
{
	public interface IDeque<T> : IQueueBase<T>, IStackBase<T>
	{
		T Peek();
		bool TryPeek(out T item);
		T PeekTail();
		bool TryPeekTail(out T item);
		void Clear();
	}
}
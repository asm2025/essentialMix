namespace essentialMix.Collections
{
	public interface IDeque<T> : IQueue<T>
	{
		void Push(T item);
		T Pop();
		T PeekTail();
	}
}
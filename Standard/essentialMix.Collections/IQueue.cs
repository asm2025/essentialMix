namespace essentialMix.Collections
{
	public interface IQueue<T> : IQueueBase<T>
	{
		T Peek();
	}
}
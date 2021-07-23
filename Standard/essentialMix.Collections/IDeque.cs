namespace essentialMix.Collections
{
	public interface IDeque<T> : IQueueBase<T>, IStackBase<T>
	{
		T Peek();
		T PeekTail();
	}
}
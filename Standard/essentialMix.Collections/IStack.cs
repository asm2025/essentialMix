namespace essentialMix.Collections
{
	public interface IStack<T> : IStackBase<T>
	{
		T Peek();
		bool TryPeek(out T item);
		void Clear();
	}
}
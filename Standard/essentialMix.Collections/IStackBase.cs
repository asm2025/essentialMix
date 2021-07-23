using System.Collections;

namespace essentialMix.Collections
{
	public interface IStackBase<T> : ICollection
	{
		void Push(T item);
		T Pop();
		bool TryPop(out T item);
	}
}
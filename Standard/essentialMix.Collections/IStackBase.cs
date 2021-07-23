using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface IStackBase<T> : ICollection<T>
	{
		void Push(T item);
		T Pop();
	}
}
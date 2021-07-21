using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface IStack<T> : ICollection<T>
	{
		void Push(T item);
		T Pop();
		T Peek();
	}
}
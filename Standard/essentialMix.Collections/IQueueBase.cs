using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface IQueueBase<T> : ICollection<T>
	{
		void Enqueue(T item);
		T Dequeue();
	}
}
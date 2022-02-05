using System.Collections;

namespace essentialMix.Collections;

public interface IQueueBase<T> : ICollection
{
	void Enqueue(T item);
	T Dequeue();
	bool TryDequeue(out T item);
}
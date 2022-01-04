using System.Collections;
using System.Collections.Generic;

namespace essentialMix.Collections;

public interface ICircularBuffer<T> : ICollection, IEnumerable<T>, IEnumerable
{
	void Enqueue(T item);
	T Dequeue();
	T Pop();
	T Peek();
	T PeekTail();
	void Clear();
}
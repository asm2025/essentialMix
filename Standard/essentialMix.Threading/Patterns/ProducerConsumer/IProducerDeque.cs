using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public interface IProducerDeque<T>
	{
		void Enqueue(T item);
		bool TryDequeue(out T item);
		void Push(T item);
		bool TryPop(out T item);
		bool TryPeekHead(out T item);
		bool TryPeekTail(out T item);
		void RemoveWhile([NotNull] Predicate<T> predicate);
		void RemoveLastWhile([NotNull] Predicate<T> predicate);
	}
}
